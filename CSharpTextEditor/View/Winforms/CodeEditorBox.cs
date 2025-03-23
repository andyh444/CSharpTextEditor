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
    public partial class CodeEditorBox : UserControl, ICodeCompletionHandler, ISourceCodeListener
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
            _viewManager = new ViewManager(_sourceCode, new WinformsClipboard());
            _viewManager.VerticalScrollChanged += _viewManager_VerticalScrollChanged;
            _viewManager.HorizontalScrollChanged += _viewManager_HorizontalScrollChanged;

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox_MouseWheel;

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
            _viewManager.SyntaxHighlighter = syntaxHighlighter;
            _codeExecutor = codeExecutor;
            _viewManager.SpecialCharacterHandler = specialCharacterHandler;
        }

        private void historyManager_HistoryChanged()
        {
            UndoHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public (IEnumerable<string> undoItems, IEnumerable<string> redoItems) GetUndoAndRedoItems()
        {
            return (_historyManager.UndoNames, _historyManager.RedoNames);
        }

        public void Undo() => _sourceCode.Undo();

        public void Redo() => _sourceCode.Redo();

        public string GetText() => _sourceCode.Text;

        public void SetText(string text) => _sourceCode.Text = text;

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
            if (_viewManager.SyntaxHighlighter == null)
            {
                return;
            }

            _viewManager.SyntaxHighlighter.Update(_sourceCode.Lines);
            _viewManager.CurrentHighlighting = _viewManager.SyntaxHighlighter.GetHighlightings(_viewManager.SyntaxPalette);
            DiagnosticsChanged?.Invoke(this, _viewManager.CurrentHighlighting.Diagnostics);
        }

        private void ResetCursorBlinkStatus()
        {
            _cursorVisible = true;
            cursorBlinkTimer.Stop();
            cursorBlinkTimer.Start();
        }

        private void CodeEditorBox_MouseWheel(object? sender, MouseEventArgs e)
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
            ViewChanged();
        }

        private void _viewManager_VerticalScrollChanged()
        {
            int maxScrollPosition = _viewManager.GetMaxVerticalScrollPosition();
            vScrollBar.Value = maxScrollPosition == 0 ? 0 : (int)((vScrollBar.Maximum * (long)_viewManager.VerticalScrollPositionPX) / maxScrollPosition);
            ViewChanged();
        }

        private void ViewChanged()
        {
            Refresh();
            MoveHelperFormToActivePosition(_codeCompletionSuggestionForm);
            MoveHelperFormToActivePosition(_methodToolTip);
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

        private void UpdateScrollBarMaxima()
        {
            vScrollBar.Maximum = _viewManager.GetMaxVerticalScrollPosition() / _viewManager.LineWidth;
            hScrollBar.Maximum = _viewManager.GetMaxHorizontalScrollPosition();
        }

        private void codePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ConfigureHighQuality();
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
                _sourceCode.SelectTokenAtPosition(position, _viewManager.SyntaxHighlighter);
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
                Refresh();
            }
            else if (_viewManager.CurrentHighlighting != null)
            {
                SourceCodePosition position = _viewManager.GetPositionFromScreenPoint(e.Location);
                string errorMessages = _viewManager.CurrentHighlighting?.GetErrorMessagesAtPosition(position, _sourceCode) ?? string.Empty;
                if (!string.IsNullOrEmpty(errorMessages))
                {
                    _hoverToolTip.Update(_viewManager.SyntaxPalette, new PlainTextToolTipContents(errorMessages));
                    ShowToolTip(_hoverToolTip, e.X, e.Y);
                }
                else if (_viewManager.SyntaxHighlighter != null)
                {
                    int charIndex = position.ToCharacterIndex(_sourceCode.Lines);
                    bool toolTipShown = false;
                    if (charIndex != -1)
                    {
                        var suggestions = _viewManager.SyntaxHighlighter.GetSuggestionsAtPosition(charIndex, _viewManager.SyntaxPalette);
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

        private void codePanel_MouseLeave(object sender, System.EventArgs e)
        {
            _methodToolTip.Hide();
            _hoverToolTip.Hide();
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
            if (_viewManager.SyntaxHighlighter == null)
            {
                return;
            }
            IReadOnlyList<CodeCompletionSuggestion> suggestions = _viewManager.SyntaxHighlighter.GetSuggestionsAtPosition(position, _viewManager.SyntaxPalette);
            if (suggestions.Any())
            {
                _codeCompletionSuggestionForm.Show(this, new SourceCodePosition(head.LineNumber, head.ColumnNumber), suggestions, _viewManager.SyntaxPalette);
                MoveHelperFormToActivePosition(_codeCompletionSuggestionForm);
                Focus();
            }
        }

        private void MoveHelperFormToActivePosition(Form f, SourceCodePosition? position = null)
        {
            if (!f.Visible)
            {
                return;
            }

            position ??= _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.GetPosition();

            var x = _viewManager.GetXCoordinateFromColumnIndex(position.Value.ColumnNumber);
            var y = _viewManager.GetYCoordinateFromLineIndex(position.Value.LineNumber);

            f.Location = PointToScreen(new Point(Location.X + x, Location.Y + y));
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
            if (_viewManager.SpecialCharacterHandler == null)
            {
                return;
            }
            // handle key presses that add a new character to the text here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar, _viewManager.SpecialCharacterHandler);
                _viewManager.EnsureActivePositionInView(codePanel.Size);

                _viewManager.SpecialCharacterHandler.HandleCharacterInserted(e.KeyChar, _sourceCode, this, _viewManager.SyntaxPalette);

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
                viewManager: _viewManager,
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
                    _sourceCode.InsertLineBreakAtActivePosition(_viewManager.SpecialCharacterHandler);
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
            UpdateScrollBarMaxima();
        }

        public void CursorsChanged()
        {
            UpdateLineAndCharacterLabel();
            ResetCursorBlinkStatus();
            Refresh();
        }

        public void GoToPosition(int line, int column) => _sourceCode.SetActivePosition(line, column);
    }
}