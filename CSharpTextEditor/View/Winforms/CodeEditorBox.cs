using CSharpTextEditor.Languages;
using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Cursor = CSharpTextEditor.Source.Cursor;
using CSharpTextEditor.View;
using CSharpTextEditor.View.Winforms;
using System.IO;
using CSharpTextEditor.Utility;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox : UserControl, ICodeCompletionHandler, ICodeEditor, ISourceCodeListener
    {
        private class RangeSelectDraggingInfo(int lineStart, int columnStart, int caretIndex)
        {
            public int LineStart { get; } = lineStart;
            public int ColumnStart { get; } = columnStart;
            public int CaretIndex { get; } = caretIndex;
        }

        private bool _cursorVisible;

        private readonly SourceCode _sourceCode;
        private readonly HistoryManager _historyManager;
        private RangeSelectDraggingInfo? _draggingInfo;
        
        private ISpecialCharacterHandler? _specialCharacterHandler;
        private ISyntaxHighlighter? _syntaxHighlighter;
        private ICodeExecutor? _codeExecutor;

        private CodeCompletionSuggestionForm _codeCompletionSuggestionForm;
        private CodeEditorTooltip _methodToolTip;
        private CodeEditorTooltip _hoverToolTip;
        private KeyboardShortcutManager _keyboardShortcutManager;
        private ViewManager _viewManager;

        public event EventHandler? UndoHistoryChanged;
        public event EventHandler<IReadOnlyCollection<SyntaxDiagnostic>>? DiagnosticsChanged;
        
        public CodeEditorBox()
        {
            InitializeComponent();

            _cursorVisible = true;
            cursorBlinkTimer.Interval = SystemInformation.CaretBlinkTime;
            cursorBlinkTimer.Enabled = true;

            _historyManager = new HistoryManager();
            _historyManager.HistoryChanged += historyManager_HistoryChanged;
            _sourceCode = new SourceCode(string.Empty, _historyManager, this);
            _viewManager = new ViewManager(_sourceCode);
            _viewManager.VerticalScrollChanged += _viewManager_VerticalScrollChanged;
            _viewManager.HorizontalScrollChanged += _viewManager_HorizontalScrollChanged;

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox2_MouseWheel;

            SetLanguageToCSharp(true);
            _codeCompletionSuggestionForm = new CodeCompletionSuggestionForm();
            _codeCompletionSuggestionForm.SetEditorBox(this);
            _methodToolTip = new CodeEditorTooltip();
            _hoverToolTip = new CodeEditorTooltip();
            SetPalette(SyntaxPalette.GetLightModePalette());
            SetKeyboardShortcuts(KeyboardShortcutManager.CreateDefault());

            if (Font.Name != "Cascadia Mono")
            {
                Font = new Font("Consolas", Font.Size, Font.Style, Font.Unit);
            }
            UpdateTextSize(codePanel.Font);
        }

        public bool CanExecuteCode() => _codeExecutor != null;

        public void SetLanguageToCSharp(bool isLibrary)
        {
            CSharpSyntaxHighlighter syntaxHighlighter = new CSharpSyntaxHighlighter(isLibrary);
            CSharpSpecialCharacterHandler specialCharacterHandler = new CSharpSpecialCharacterHandler(syntaxHighlighter);
            SetLanguage(
                syntaxHighlighter: syntaxHighlighter,
                codeExecutor: isLibrary ? null : syntaxHighlighter,
                specialCharacterHandler: specialCharacterHandler);
            UpdateSyntaxHighlighting();
        }

        private void SetLanguage(ISyntaxHighlighter syntaxHighlighter, ICodeExecutor? codeExecutor, ISpecialCharacterHandler specialCharacterHandler)
        {
            _syntaxHighlighter = syntaxHighlighter;
            _codeExecutor = codeExecutor;
            _specialCharacterHandler = specialCharacterHandler;
        }

        private void historyManager_HistoryChanged()
        {
            UndoHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public (IEnumerable<string> undoItems, IEnumerable<string> redoItems) GetUndoAndRedoItems()
        {
            return (_historyManager.UndoNames, _historyManager.RedoNames);
        }

        public void Undo()
        {
            _sourceCode.Undo();
            UpdateSyntaxHighlighting();
            ResetCursorBlinkStatus();
            Refresh();
        }

        public void Redo()
        {
            _sourceCode.Redo();
            UpdateSyntaxHighlighting();
            ResetCursorBlinkStatus();
            Refresh();
        }

        public string GetText() => _sourceCode.Text;

        public void SetText(string text)
        {
            _sourceCode.Text = text;
            UpdateSyntaxHighlighting();
            Refresh();
        }

        public void SetKeyboardShortcuts(KeyboardShortcutManager keyboardShortcuts)
        {
            _keyboardShortcutManager = keyboardShortcuts;
        }

        public void SetPalette(SyntaxPalette palette)
        {
            _viewManager.SyntaxPalette = palette;
            UpdateSyntaxHighlighting();
            Refresh();
        }

        private void UpdateTextSize(Font font)
        {
            Size characterSize = DrawingHelper.GetMonospaceCharacterSize(new WinformsCanvas(codePanel.CreateGraphics(), new Size(), font));
            _viewManager.CharacterWidth = characterSize.Width;
            _viewManager.LineWidth = characterSize.Height;
        }

        private void UpdateSyntaxHighlighting()
        {
            if (_syntaxHighlighter == null)
            {
                return;
            }

            _syntaxHighlighter.Update(_sourceCode.Lines);
            _viewManager.Highlighting = _syntaxHighlighter.GetHighlightings(_viewManager.SyntaxPalette);
            DiagnosticsChanged?.Invoke(this, _viewManager.Highlighting.Diagnostics);
        }

        private void ResetCursorBlinkStatus()
        {
            // TODO: Maybe there needs to be an event for whenever the cursor moves,
            // rather than calling this method in a several places
            _cursorVisible = true;
            cursorBlinkTimer.Stop();
            cursorBlinkTimer.Start();
        }

        private void CodeEditorBox2_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                codePanel.Font = new Font(codePanel.Font.Name, Math.Max(1, codePanel.Font.Size + Math.Sign(e.Delta)), codePanel.Font.Style, codePanel.Font.Unit);
                UpdateTextSize(codePanel.Font);
                Refresh();
            }
            else
            {
                _viewManager.ScrollView(-3 * Math.Sign(e.Delta));
            }
            
        }

        private void _viewManager_HorizontalScrollChanged()
        {
            int maxScrollPosition = _viewManager.GetMaxHorizontalScrollPosition();
            hScrollBar.Value = maxScrollPosition == 0 ? 0 : (int)((hScrollBar.Maximum * (long)_viewManager.HorizontalScrollPositionPX) / maxScrollPosition);
            Refresh();
        }

        private void _viewManager_VerticalScrollChanged()
        {
            int maxScrollPosition = _viewManager.GetMaxVerticalScrollPosition();
            vScrollBar.Value = maxScrollPosition == 0 ? 0 : (int)((vScrollBar.Maximum * (long)_viewManager.VerticalScrollPositionPX) / maxScrollPosition);
            Refresh();

            MoveCodeEditorFormToActivePosition();
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = _viewManager.GetMaxVerticalScrollPosition();
            if (vScrollBar.Maximum == 0)
            {
                _viewManager.VerticalScrollPositionPX = 0;
            }
            else
            {
                _viewManager.VerticalScrollPositionPX = (vScrollBar.Value * maxScrollPosition) / vScrollBar.Maximum;
            }
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = _viewManager.GetMaxHorizontalScrollPosition();
            if (hScrollBar.Maximum == 0)
            {
                _viewManager.HorizontalScrollPositionPX = 0;
            }
            else
            {
                _viewManager.HorizontalScrollPositionPX = (hScrollBar.Value * maxScrollPosition) / hScrollBar.Maximum;
            }
        }

        private void codePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // not strictly part of drawing, but close enough
            vScrollBar.Maximum = _viewManager.GetMaxVerticalScrollPosition() / _viewManager.LineWidth;
            hScrollBar.Maximum = _viewManager.GetMaxHorizontalScrollPosition();
            UpdateLineAndCharacterLabel();

            _viewManager.Draw(new WinformsCanvas(e.Graphics, codePanel.Size, codePanel.Font), new DrawSettings(Focused, _cursorVisible));
        }   

        private void UpdateLineAndCharacterLabel()
        {
            int lineNumber = 1 + _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber;
            int columnNumber = 1 + _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber;
            StringBuilder sb = new StringBuilder();
            if (_sourceCode.OvertypeEnabled)
            {
                sb.Append("OVR ");
            }
            sb.Append($"Ln: {lineNumber} Ch: {columnNumber}");

            lineLabel.Text = sb.ToString();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        private void codePanel_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void codePanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SourceCodePosition position = _viewManager.GetPositionFromScreenPoint(e.Location);
                _sourceCode.SelectTokenAtPosition(position, _syntaxHighlighter);
                Refresh();
            }
        }

        private void codePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }
            else if (e.Button == MouseButtons.Left)
            {
                HideCodeCompletionForm();
                SourceCodePosition position = _viewManager.GetPositionFromScreenPoint(e.Location);
                int caretIndex;
                if (ModifierKeys.HasFlag(Keys.Control)
                    && ModifierKeys.HasFlag(Keys.Alt))
                {
                    caretIndex = _sourceCode.AddCaret(position.LineNumber, position.ColumnNumber);
                }
                else
                {
                    caretIndex = SelectionRangeCollection.PRIMARY_INDEX;
                    _sourceCode.SetActivePosition(position.LineNumber, position.ColumnNumber);
                }
                _draggingInfo = new RangeSelectDraggingInfo(position.LineNumber, position.ColumnNumber, caretIndex);
            }
            ResetCursorBlinkStatus();
            Refresh();
        }

        private void codePanel_MouseUp(object sender, MouseEventArgs e)
        {
            _draggingInfo = null;
        }

        private void codePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggingInfo != null
                && e.Button == MouseButtons.Left)
            {
                SourceCodePosition position = _viewManager.GetPositionFromScreenPoint(e.Location);
                if (_draggingInfo.CaretIndex != 0)
                {
                    // multi-caret mode
                    _sourceCode.SelectRange(_draggingInfo.LineStart, _draggingInfo.ColumnStart, position.LineNumber, position.ColumnNumber, _draggingInfo.CaretIndex);
                }
                else if (ModifierKeys.HasFlag(Keys.Alt))
                {
                    _sourceCode.ColumnSelect(_draggingInfo.LineStart, _draggingInfo.ColumnStart, position.LineNumber, position.ColumnNumber);
                }
                else
                {
                    _sourceCode.SelectRange(_draggingInfo.LineStart, _draggingInfo.ColumnStart, position.LineNumber, position.ColumnNumber);
                }
                _viewManager.EnsureActivePositionInView(codePanel.Size);
                ResetCursorBlinkStatus();
                Refresh();
            }
            else if (_viewManager.Highlighting != null)
            {
                SourceCodePosition position = _viewManager.GetPositionFromScreenPoint(e.Location);
                string errorMessages = _viewManager.Highlighting?.GetErrorMessagesAtPosition(position, _sourceCode) ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessages))
                {
                    _hoverToolTip.Update(_viewManager.SyntaxPalette, new PlainTextToolTipContents(errorMessages));
                    ShowToolTip(_hoverToolTip, e.X, e.Y);
                }
                else if (_syntaxHighlighter != null)
                {
                    int charIndex = position.ToCharacterIndex(_sourceCode.Lines);
                    bool toolTipShown = false;
                    if (charIndex != -1)
                    {
                        var suggestions = _syntaxHighlighter.GetSuggestionAtPosition(charIndex, _viewManager.SyntaxPalette);
                        if (suggestions.Any())
                        {
                            toolTipShown = true;
                            (string text, _) = suggestions.First().ToolTipSource.GetToolTip();
                            _hoverToolTip.Update(_viewManager.SyntaxPalette, new MethodCompletionContents(suggestions.Take(1).ToList(), 0, -1));
                            ShowToolTip(_hoverToolTip, e.X, e.Y);
                        }
                    }
                    if (!toolTipShown)
                    {
                        _hoverToolTip.Hide();
                    }
                }
            }
        }

        private void CodeEditorBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // needed so that the KeyDown event picks up the arrowkeys and tab key
            if (e.KeyData.HasFlag(Keys.Right)
                || e.KeyData.HasFlag(Keys.Left)
                || e.KeyData.HasFlag(Keys.Up)
                || e.KeyData.HasFlag(Keys.Down)
                || e.KeyData.HasFlag(Keys.Tab))
            {
                e.IsInputKey = true;
            }
        }

        public void HideCodeCompletionForm(bool hideMethodToolTipToo = true)
        {
            _codeCompletionSuggestionForm.Hide();
            if (hideMethodToolTipToo)
            {
                _methodToolTip.Hide();
            }
        }

        public void ShowCodeCompletionForm()
        {
            HideCodeCompletionForm();
            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            int position = new SourceCodePosition(head.LineNumber, head.ColumnNumber).ToCharacterIndex(_sourceCode.Lines);
            if (position == -1)
            {
                return;
            }
            if (_syntaxHighlighter == null)
            {
                return;
            }
            CodeCompletionSuggestion[] suggestions = _syntaxHighlighter.GetCodeCompletionSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber).Substring(0, head.ColumnNumber), position, _viewManager.SyntaxPalette).ToArray();
            if (suggestions.Any())
            {
                _codeCompletionSuggestionForm.Show(this, new SourceCodePosition(head.LineNumber, head.ColumnNumber), suggestions, _viewManager.SyntaxPalette);
                MoveCodeEditorFormToActivePosition();
                Focus();
            }
        }

        private void MoveCodeEditorFormToActivePosition()
        {
            if (!_codeCompletionSuggestionForm.Visible)
            {
                return;
            }
            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            var x = _viewManager.GetXCoordinateFromColumnIndex(head.ColumnNumber);
            var y = _viewManager.GetYCoordinateFromLineIndex(head.LineNumber);
            _codeCompletionSuggestionForm.Location = PointToScreen(new Point(Location.X + x, Location.Y + y));
        }

        internal void ChooseCodeCompletionItem(string item)
        {
            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            SourceCodePosition? startPosition = _codeCompletionSuggestionForm.GetPosition();
            if (startPosition != null)
            {
                var start = _sourceCode.GetCursor(startPosition.Value);
                int diff = start.GetPositionDifference(head);
                _sourceCode.InsertStringAtActivePosition(item.Substring(diff));
                HideCodeCompletionForm();
                Refresh();
            }
        }

        private void CodeEditorBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_specialCharacterHandler == null)
            {
                return;
            }
            // handle key presses that add a new character to the text here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar, _specialCharacterHandler);
                _viewManager.EnsureActivePositionInView(codePanel.Size);

                _specialCharacterHandler.HandleCharacterInserted(e.KeyChar, _sourceCode, this, _viewManager.SyntaxPalette);

                if (_codeCompletionSuggestionForm.Visible)
                {
                    Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                    _codeCompletionSuggestionForm.FilterSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
                }
                Refresh();
            }
        }

        private void CodeEditorBox_KeyDown(object sender, KeyEventArgs e)
        {
            bool shortcutProcessed = _keyboardShortcutManager.ProcessShortcut(
                controlPressed: e.Control,
                shiftPressed: e.Shift,
                altPressed: e.Alt,
                keyCode: e.KeyCode,
                codeEditor: this,
                out bool ensureInView);
            if (shortcutProcessed)
            {
                if (ensureInView)
                {
                    HideCodeCompletionForm();
                    _viewManager.EnsureActivePositionInView(codePanel.Size);
                }
            }
            else if (!e.Control)
            {
                HandleCoreKeyDownEvent(e);
            }
            Refresh();
        }

        private void HandleCoreKeyDownEvent(KeyEventArgs e)
        {
            // handles the set of keyboard presses that can't be customised
            bool ensureInView = true;
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    HideCodeCompletionForm();
                    break;
                case Keys.Back:
                    _sourceCode.RemoveCharacterBeforeActivePosition();
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                        if (head.ColumnNumber < _codeCompletionSuggestionForm.GetPosition().ColumnNumber)
                        {
                            HideCodeCompletionForm();
                        }
                        else
                        {
                            _codeCompletionSuggestionForm.FilterSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
                        }
                    }
                    break;
                case Keys.Delete:
                    _sourceCode.RemoveCharacterAfterActivePosition();
                    break;

                case Keys.Left:
                    HideCodeCompletionForm();
                    _sourceCode.ShiftHeadToTheLeft(e.Shift);
                    break;
                case Keys.Right:
                    HideCodeCompletionForm();
                    _sourceCode.ShiftHeadToTheRight(e.Shift);
                    break;
                case Keys.Up:
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        _codeCompletionSuggestionForm.MoveSelectionUp();
                    }
                    else if (_methodToolTip.Visible)
                    {
                        _methodToolTip.DecrementActiveSuggestion();
                    }
                    else
                    {
                        _sourceCode.ShiftHeadUpOneLine(e.Shift);
                    }
                    break;
                case Keys.Down:
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        _codeCompletionSuggestionForm.MoveSelectionDown();
                    }
                    else if (_methodToolTip.Visible)
                    {
                        _methodToolTip.IncrementActiveSuggestion();
                    }
                    else
                    {
                        _sourceCode.ShiftHeadDownOneLine(e.Shift);
                    }
                    break;
                case Keys.End:
                    HideCodeCompletionForm();
                    _sourceCode.ShiftHeadToEndOfLine(e.Shift);
                    break;
                case Keys.Home:
                    HideCodeCompletionForm();
                    _sourceCode.ShiftHeadToStartOfLine(e.Shift);
                    break;
                case Keys.PageUp:
                    HideCodeCompletionForm();
                    _sourceCode.ShiftHeadUpLines(Height / _viewManager.LineWidth, e.Shift);
                    break;
                case Keys.PageDown:
                    HideCodeCompletionForm();
                    _sourceCode.ShiftHeadDownLines(Height / _viewManager.LineWidth, e.Shift);
                    break;

                case Keys.Enter:
                    HideCodeCompletionForm();
                    _sourceCode.InsertLineBreakAtActivePosition(_specialCharacterHandler);
                    break;
                case Keys.Tab:
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        ChooseCodeCompletionItem(_codeCompletionSuggestionForm.GetSelectedItem());
                    }
                    else if (e.Shift)
                    {
                        _sourceCode.DecreaseIndentAtActivePosition();
                    }
                    else
                    {
                        _sourceCode.IncreaseIndentAtActivePosition();
                    }
                    break;
                case Keys.Insert:
                    _sourceCode.OvertypeEnabled = !_sourceCode.OvertypeEnabled;
                    break;
                default:
                    ensureInView = false;
                    break;
            }
            if (ensureInView)
            {
                _viewManager.EnsureActivePositionInView(codePanel.Size);
            }
        }

        public void ShowMethodCompletion(SourceCodePosition position, IReadOnlyCollection<CodeCompletionSuggestion> suggestions, int activeParameterIndex)
        {
            if (suggestions.Any())
            {
                var suggestion = suggestions.First();
                if (!suggestion.IsDeclaration
                    && suggestion.SymbolType == SymbolType.Method)
                {
                    var x = _viewManager.GetXCoordinateFromColumnIndex(position.ColumnNumber);
                    var y = _viewManager.GetYCoordinateFromLineIndex(position.LineNumber + 1);
                    _methodToolTip.Update(_viewManager.SyntaxPalette, new MethodCompletionContents(suggestions.ToList(), 0, activeParameterIndex));
                    ShowToolTip(_methodToolTip, x, y);
                }
            }
        }

        private void ShowToolTip(CodeEditorTooltip tooltip, int x, int y)
        {
            if (!tooltip.Visible)
            {
                var point = PointToScreen(new Point(Location.X + x, Location.Y + y));
                tooltip.Location = point;
                tooltip.Show();
                tooltip.Location = point;
            }
            Focus();
        }

        void ICodeEditor.Undo()
        {
            _sourceCode.Undo();
            UpdateSyntaxHighlighting();
        }

        void ICodeEditor.Redo()
        {
            _sourceCode.Redo();
            UpdateSyntaxHighlighting();
        }

        public void RemoveWordAfterActivePosition()
        {
            _sourceCode.RemoveWordAfterActivePosition(_syntaxHighlighter);
        }

        public void RemoveWordBeforeActivePosition()
        {
            _sourceCode.RemoveWordBeforeActivePosition(_syntaxHighlighter);
        }

        public void GoToLastPosition()
        {
            _sourceCode.SetActivePosition(_sourceCode.LineCount, _sourceCode.Lines.Last().Length);
        }

        public void GoToFirstPosition()
        {
            _sourceCode.SetActivePosition(0, 0);
        }

        public void GoToPosition(int line, int column)
        {
            _sourceCode.SetActivePosition(line, column);
        }

        public void ShiftActivePositionOneWordToTheRight(bool select)
        {
            _sourceCode.ShiftHeadOneWordToTheRight(_syntaxHighlighter, select);
        }

        public void ShiftActivePositionOneWordToTheLeft(bool select)
        {
            _sourceCode.ShiftHeadOneWordToTheLeft(_syntaxHighlighter, select);
        }

        public void PasteFromClipboard()
        {
            _sourceCode.InsertStringAtActivePosition(Clipboard.GetText());
        }

        public void CopySelectedToClipboard()
        {
            string selectedTextForCopy = _sourceCode.GetSelectedText();
            if (!string.IsNullOrEmpty(selectedTextForCopy))
            {
                Clipboard.SetText(selectedTextForCopy);
            }
            ResetCursorBlinkStatus();
        }

        public void CutSelectedToClipboard()
        {
            string selectedTextForCut = _sourceCode.GetSelectedText();
            _sourceCode.RemoveSelectedRange();
            if (!string.IsNullOrEmpty(selectedTextForCut))
            {
                Clipboard.SetText(selectedTextForCut);
            }
        }

        public void SelectAll()
        {
            _sourceCode.SelectAll();
        }

        public void ScrollView(int numberOfLines)
        {
            _viewManager.ScrollView(numberOfLines);
        }

        public void DuplicateSelection()
        {
            _sourceCode.DuplicateSelection();
        }

        public void SelectionToLowerCase()
        {
            _sourceCode.SelectionToLowerCase();
        }

        public void SelectionToUpperCase()
        {
            _sourceCode.SelectionToUpperCase();
        }

        public void Execute(TextWriter output)
        {
            _codeExecutor?.Execute(output);
        }

        private void cursorBlinkTimer_Tick(object sender, EventArgs e)
        {
            _cursorVisible = !_cursorVisible;
            if (Focused)
            {
                Refresh();
            }
        }

        void ISourceCodeListener.TextChanged()
        {
            UpdateSyntaxHighlighting();
        }

        public void CursorsChanged()
        {
            ResetCursorBlinkStatus();
        }

        public void RemoveLineAtActivePosition() => _sourceCode.RemoveLineAtActivePosition();

        public void SwapLineUpAtActivePosition() => _sourceCode.SwapLinesUpAtActivePosition();

        public void SwapLineDownAtActivePosition() => _sourceCode.SwapLinesDownAtActivePosition();
    }
}