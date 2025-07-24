using NTextEditor.Languages;
using NTextEditor.Source;
using NTextEditor.UndoRedoActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Cursor = NTextEditor.Source.Cursor;
using System.IO;
using NTextEditor.Utility;
using NTextEditor.View.WinForms;
using NTextEditor.Languages.PlainText;
using NTextEditor.View.ToolTips;

namespace NTextEditor.View.Winforms
{
    public partial class CodeEditorBox : UserControl, ICodeCompletionHandler, ISourceCodeListener, ILanguageManager
    {
        private bool _cursorVisible;

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

            _viewManager = new ViewManager(this, new WinformsClipboard());
            _viewManager.HistoryManager.HistoryChanged += historyManager_HistoryChanged;
            _viewManager.VerticalScrollChanged += _viewManager_VerticalScrollChanged;
            _viewManager.HorizontalScrollChanged += _viewManager_HorizontalScrollChanged;
            this.SetLanguageToPlainText();

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox_MouseWheel;

            _codeCompletionSuggestionForm = new CodeCompletionSuggestionForm();
            _codeCompletionSuggestionForm.SetEditorBox(this);
            _methodToolTip = new CodeEditorTooltip();
            _hoverToolTip = new CodeEditorTooltip();
            SetPalette(SyntaxPalette.GetLightModePalette());
            SetKeyboardShortcuts(KeyboardShortcutManager.CreateDefault());

            UpdateTextSize(codePanel.Font);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            codePanel.Font = Font;
            UpdateTextSize(codePanel.Font);
        }

        public bool CanExecuteCode() => _codeExecutor != null;

        public void SetLanguage(ISyntaxHighlighter syntaxHighlighter, ICodeExecutor? codeExecutor, ISpecialCharacterHandler specialCharacterHandler)
        {
            _viewManager.SyntaxHighlighter = syntaxHighlighter;
            _codeExecutor = codeExecutor;
            _viewManager.SpecialCharacterHandler = specialCharacterHandler;
            UpdateSyntaxHighlighting();
        }

        private void historyManager_HistoryChanged()
        {
            UndoHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public (IEnumerable<string> undoItems, IEnumerable<string> redoItems) GetUndoAndRedoItems()
        {
            return (_viewManager.HistoryManager.UndoNames, _viewManager.HistoryManager.RedoNames);
        }

        public void Undo() => _viewManager.SourceCode.Undo();

        public void Redo() => _viewManager.SourceCode.Redo();

        public string GetText() => _viewManager.SourceCode.Text;

        public void SetText(string text) => _viewManager.SourceCode.Text = text;

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
            SizeF characterSize = DrawingHelper.GetMonospaceCharacterSize(new WinformsCanvas(codePanel.CreateGraphics(), new Size(), font));
            if (_viewManager != null)
            {
                _viewManager.CharacterWidth = (int)characterSize.Width;
                _viewManager.LineWidth = (int)characterSize.Height;
            }
        }

        private void UpdateSyntaxHighlighting()
        {
            if (_viewManager.SyntaxHighlighter == null)
            {
                return;
            }

            _viewManager.SyntaxHighlighter.Update(_viewManager.SourceCode.Lines);
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
                Font = new Font(codePanel.Font.Name, Math.Max(1, codePanel.Font.Size + Math.Sign(e.Delta)), codePanel.Font.Style, codePanel.Font.Unit);
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
            lineLabel.Text = _viewManager.GetLineAndCharacterLabel();
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
                _viewManager.HandleLeftMouseDoubleClick(e.Location);
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
                // TODO: Do this inside view manager
                HideCodeCompletionForm();

                _viewManager.HandleLeftMouseDown(e.Location, ModifierKeys.HasFlag(Keys.Control), ModifierKeys.HasFlag(Keys.Alt));
            }
        }

        private void codePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _viewManager.HandleLeftMouseUp(e.Location);
            }
        }

        private void codePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _viewManager.HandleLeftMouseDrag(e.Location, ModifierKeys.HasFlag(Keys.Alt), codePanel.Size);
            }
            else
            {
                _viewManager.HandleMouseMove(e.Location);
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
            Cursor head = _viewManager.SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            int position = new SourceCodePosition(head.LineNumber, head.ColumnNumber).ToCharacterIndex(_viewManager.SourceCode.Lines);
            if (position == -1)
            {
                return;
            }
            if (_viewManager.SyntaxHighlighter == null)
            {
                return;
            }
            IReadOnlyList<CodeCompletionSuggestion> suggestions = _viewManager.SyntaxHighlighter.GetSuggestionsAtPosition(position, _viewManager.SyntaxPalette, out _);
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

            position ??= _viewManager.SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.GetPosition();

            var x = _viewManager.GetXCoordinateFromColumnIndex(position.Value.ColumnNumber);
            var y = _viewManager.GetYCoordinateFromLineIndex(position.Value.LineNumber);

            f.Location = PointToScreen(new Point((int)(Location.X + x), Location.Y + y));
        }

        internal void ChooseCodeCompletionItem(string item)
        {
            Cursor head = _viewManager.SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            SourceCodePosition? startPosition = _codeCompletionSuggestionForm.GetPosition();
            if (startPosition != null)
            {
                var start = _viewManager.SourceCode.GetCursor(startPosition.Value);
                int diff = start.GetPositionDifference(head);
                _viewManager.SourceCode.InsertStringAtActivePosition(item.Substring(diff));
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
                _viewManager.SourceCode.InsertCharacterAtActivePosition(e.KeyChar, _viewManager.SpecialCharacterHandler);
                _viewManager.EnsureActivePositionInView(codePanel.Size);

                _viewManager.SpecialCharacterHandler.HandleCharacterInserted(e.KeyChar, _viewManager.SourceCode, this, _viewManager.SyntaxPalette);

                if (_codeCompletionSuggestionForm.Visible)
                {
                    Cursor head = _viewManager.SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                    _codeCompletionSuggestionForm.FilterSuggestions(_viewManager.SourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
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
                keyCode: e.KeyCode.ToTextEditorKey(),
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
                    _viewManager.SourceCode.RemoveCharacterBeforeActivePosition();
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        Cursor head = _viewManager.SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                        if (head.ColumnNumber < _codeCompletionSuggestionForm.GetPosition().ColumnNumber)
                        {
                            HideCodeCompletionForm();
                        }
                        else
                        {
                            _codeCompletionSuggestionForm.FilterSuggestions(_viewManager.SourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
                        }
                    }
                    break;
                case Keys.Delete:
                    _viewManager.SourceCode.RemoveCharacterAfterActivePosition();
                    break;
                case Keys.Left:
                    HideCodeCompletionForm(false);
                    _viewManager.SourceCode.ShiftHeadToTheLeft(e.Shift);
                    UpdateMethodToolTip();
                    break;
                case Keys.Right:
                    HideCodeCompletionForm(false);
                    _viewManager.SourceCode.ShiftHeadToTheRight(e.Shift);
                    UpdateMethodToolTip();
                    break;
                case Keys.Up:
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        _codeCompletionSuggestionForm.MoveSelectionUp();
                    }
                    else if (!_methodToolTip.Visible || !_methodToolTip.DecrementActiveSuggestion())
                    {
                        _viewManager.SourceCode.ShiftHeadUpOneLine(e.Shift);
                    }
                    break;
                case Keys.Down:
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        _codeCompletionSuggestionForm.MoveSelectionDown();
                    }
                    else if (!_methodToolTip.Visible || !_methodToolTip.IncrementActiveSuggestion())
                    {
                        _viewManager.SourceCode.ShiftHeadDownOneLine(e.Shift);
                    }
                    break;
                case Keys.End:
                    HideCodeCompletionForm();
                    _viewManager.SourceCode.ShiftHeadToEndOfLine(e.Shift);
                    break;
                case Keys.Home:
                    HideCodeCompletionForm();
                    _viewManager.SourceCode.ShiftHeadToStartOfLine(e.Shift);
                    break;
                case Keys.PageUp:
                    HideCodeCompletionForm();
                    _viewManager.SourceCode.ShiftHeadUpLines(Height / _viewManager.LineWidth, e.Shift);
                    break;
                case Keys.PageDown:
                    HideCodeCompletionForm();
                    _viewManager.SourceCode.ShiftHeadDownLines(Height / _viewManager.LineWidth, e.Shift);
                    break;

                case Keys.Enter:
                    HideCodeCompletionForm();
                    _viewManager.SourceCode.InsertLineBreakAtActivePosition(_viewManager.SpecialCharacterHandler);
                    break;
                case Keys.Tab:
                    if (_codeCompletionSuggestionForm.Visible
                        && _codeCompletionSuggestionForm.TryGetSelectedItem(out string? selectedItem))
                    {
                        ChooseCodeCompletionItem(selectedItem!);
                    }
                    else if (e.Shift)
                    {
                        _viewManager.SourceCode.DecreaseIndentAtActivePosition();
                    }
                    else
                    {
                        _viewManager.SourceCode.IncreaseIndentAtActivePosition();
                    }
                    break;
                case Keys.Insert:
                    _viewManager.SourceCode.OvertypeEnabled = !_viewManager.SourceCode.OvertypeEnabled;
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

        private void UpdateMethodToolTip()
        {
            // TODO: This but better

            /*Cursor head = _viewManager.SourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.Clone();
            if (_methodToolTip.Visible
                && _methodToolTip.GetContents() is MethodCompletionContents mcc
                && CSharpSpecialCharacterHandler.BacktrackCursorToMethodStartAndCountParameters(head, out int parameterIndex))
            {
                int charIndex = head.GetPosition().ToCharacterIndex(_viewManager.SourceCode.Lines);
                var suggestions = _viewManager.SyntaxHighlighter.GetSuggestionsAtPosition(charIndex, _viewManager.SyntaxPalette);
                if (suggestions.Any())
                {
                    _methodToolTip.Update(_viewManager.SyntaxPalette, new MethodCompletionContents(suggestions, mcc.ActiveSuggestion, parameterIndex));
                }
                else
                {
                    _methodToolTip.Hide();
                }
            }*/
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
                    MethodCompletionContents contents = _methodToolTip.GetContents() is MethodCompletionContents mcc
                        ? mcc.WithNewSuggestions(suggestions.ToList(), activeParameterIndex)
                        : new MethodCompletionContents(suggestions.ToList(), 0, activeParameterIndex);
                    _methodToolTip.Update(_viewManager.SyntaxPalette, contents);
                    ShowToolTip(_methodToolTip, (int)x, y);
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

        public void GoToPosition(int line, int column)
        {
            _viewManager.SourceCode.SetActivePosition(line, column);
            _viewManager.EnsureActivePositionInView(codePanel.Size);
        }

        public void SelectRange(SourceCodePosition positionStart, SourceCodePosition positionEnd)
        {
            _viewManager.SourceCode.SelectRange(positionStart, positionEnd);
            _viewManager.EnsureActivePositionInView(codePanel.Size);
        }

        public void ShowMethodToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, Point point)
        {
            _methodToolTip.Update(_viewManager.SyntaxPalette, toolTipContents);
            ShowToolTip(_methodToolTip, point.X, point.Y);
        }

        public void HideMethodToolTip() => _methodToolTip.Hide();

        public void ShowHoverToolTip(SyntaxPalette palette, IToolTipContents toolTipContents, Point point)
        {
            _hoverToolTip.Update(_viewManager.SyntaxPalette, toolTipContents);
            ShowToolTip(_hoverToolTip, point.X, point.Y);
        }

        public void HideHoverToolTip() => _hoverToolTip.Hide();

        public bool FindNextText(string text, bool matchCase, bool wraparound, out SourceCodePosition? positionStart, out SourceCodePosition? positionEnd)
        {
            return _viewManager.FindNextText(text, matchCase, wraparound, out positionStart, out positionEnd);
        }
    }
}