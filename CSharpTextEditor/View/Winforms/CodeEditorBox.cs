﻿using CSharpTextEditor.Languages;
using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SelectionRange = CSharpTextEditor.Source.SelectionRange;
using Cursor = CSharpTextEditor.Source.Cursor;
using CSharpTextEditor.View;
using CSharpTextEditor.View.Winforms;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox : UserControl, ICodeCompletionHandler, ICodeEditor
    {
        private class RangeSelectDraggingInfo(int lineStart, int columnStart, int caretIndex)
        {
            public int LineStart { get; } = lineStart;
            public int ColumnStart { get; } = columnStart;
            public int CaretIndex { get; } = caretIndex;
        }

        
        private readonly SourceCode _sourceCode;
        private readonly HistoryManager _historyManager;
        private RangeSelectDraggingInfo? _draggingInfo;
        
        private ISpecialCharacterHandler _specialCharacterHandler;
        private ISyntaxHighlighter _syntaxHighlighter;
        private CodeCompletionSuggestionForm _codeCompletionSuggestionForm;
        private KeyboardShortcutManager _keyboardShortcutManager;
        private ViewManager _viewManager;

        public event EventHandler? UndoHistoryChanged;

// disable nullable warning: we know that syntax palette and keyboard shortcut manager will be set
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public CodeEditorBox()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            InitializeComponent();
            _historyManager = new HistoryManager();
            _historyManager.HistoryChanged += historyManager_HistoryChanged;
            _sourceCode = new SourceCode(string.Empty, _historyManager);
            _viewManager = new ViewManager(_sourceCode);

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox2_MouseWheel;

            CSharpSyntaxHighlighter syntaxHighlighter = new CSharpSyntaxHighlighter();
            _syntaxHighlighter = syntaxHighlighter;
            _specialCharacterHandler = new CSharpSpecialCharacterHandler(syntaxHighlighter);
            _codeCompletionSuggestionForm = new CodeCompletionSuggestionForm();
            _codeCompletionSuggestionForm.SetEditorBox(this);
            SetPalette(SyntaxPalette.GetLightModePalette());
            SetKeyboardShortcuts(KeyboardShortcutManager.CreateDefault());

            if (Font.Name != "Cascadia Mono")
            {
                Font = new Font("Consolas", Font.Size, Font.Style, Font.Unit);
            }
            UpdateTextSize(codePanel.Font);
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
            Refresh();
        }

        public void Redo()
        {
            _sourceCode.Redo();
            UpdateSyntaxHighlighting();
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
            hoverToolTip.BackColor = palette.ToolTipBackColour;
            methodToolTip.BackColor = palette.ToolTipBackColour;
            Refresh();
        }

        private void UpdateTextSize(Font font)
        {
            Size characterSize = DrawingHelper.GetMonospaceCharacterSize(font, codePanel.CreateGraphics());
            _viewManager.CharacterWidth = characterSize.Width;
            _viewManager.LineWidth = characterSize.Height;
        }

        private void EnsureActivePositionInView()
        {
            EnsureVerticalActivePositionInView();
            EnsureHorizontalActivePositionInView();
        }

        private void EnsureVerticalActivePositionInView()
        {
            int activeLine = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber;
            int minLineInView = _viewManager.VerticalScrollPositionPX / _viewManager.LineWidth;
            int maxLineInView = (_viewManager.VerticalScrollPositionPX + codePanel.Height - _viewManager.LineWidth) / _viewManager.LineWidth;
            if (activeLine > maxLineInView)
            {
                UpdateVerticalScrollPositionPX(activeLine * _viewManager.LineWidth - codePanel.Height + _viewManager.LineWidth);
            }
            else if (activeLine < minLineInView)
            {
                UpdateVerticalScrollPositionPX(activeLine * _viewManager.LineWidth);
            }
        }

        private void EnsureHorizontalActivePositionInView()
        {
            int activeColumn = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber;
            int minColumnInView = _viewManager.HorizontalScrollPositionPX / _viewManager.CharacterWidth;
            int maxColumnInView = (_viewManager.HorizontalScrollPositionPX + codePanel.Width - _viewManager.CharacterWidth - _viewManager.GetGutterWidth() - ViewManager.LEFT_MARGIN) / _viewManager.CharacterWidth;
            if (activeColumn > maxColumnInView)
            {
                UpdateHorizontalScrollPositionPX(activeColumn * _viewManager.CharacterWidth - codePanel.Width + _viewManager.GetGutterWidth() + ViewManager.LEFT_MARGIN + _viewManager.CharacterWidth);
            }
            else if (activeColumn < minColumnInView)
            {
                UpdateHorizontalScrollPositionPX(Math.Max(0, activeColumn - 6) * _viewManager.CharacterWidth);
            }
        }

        private void UpdateSyntaxHighlighting()
        {
            _viewManager.Highlighting = _syntaxHighlighter.GetHighlightings(_sourceCode.Lines, _viewManager.SyntaxPalette);
        }

        private void CodeEditorBox2_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                codePanel.Font = new Font(codePanel.Font.Name, Math.Max(1, codePanel.Font.Size + Math.Sign(e.Delta)), codePanel.Font.Style, codePanel.Font.Unit);
                UpdateTextSize(codePanel.Font);
            }
            else
            {
                UpdateVerticalScrollPositionPX(_viewManager.VerticalScrollPositionPX - 3 * _viewManager.LineWidth * Math.Sign(e.Delta));
            }
            Refresh();
        }

        private void UpdateVerticalScrollPositionPX(int newValue)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            _viewManager.VerticalScrollPositionPX = Clamp(newValue, 0, maxScrollPosition);
            vScrollBar.Value = maxScrollPosition == 0 ? 0 : (int)((vScrollBar.Maximum * (long)_viewManager.VerticalScrollPositionPX) / maxScrollPosition);
        }

        private void UpdateHorizontalScrollPositionPX(int newValue)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            _viewManager.HorizontalScrollPositionPX = Clamp(newValue, 0, maxScrollPosition);
            hScrollBar.Value = maxScrollPosition == 0 ? 0 : (int)((hScrollBar.Maximum * (long)_viewManager.HorizontalScrollPositionPX) / maxScrollPosition);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        private int GetMaxHorizontalScrollPosition()
        {
            return _sourceCode.Lines.Max(x => x.Length) * _viewManager.CharacterWidth;
        }

        private int GetMaxVerticalScrollPosition()
        {
            return (_sourceCode.LineCount - 1) * _viewManager.LineWidth;
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            if (vScrollBar.Maximum == 0)
            {
                _viewManager.VerticalScrollPositionPX = 0;
            }
            else
            {
                _viewManager.VerticalScrollPositionPX = (vScrollBar.Value * maxScrollPosition) / vScrollBar.Maximum;
            }
            Refresh();
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            if (hScrollBar.Maximum == 0)
            {
                _viewManager.HorizontalScrollPositionPX = 0;
            }
            else
            {
                _viewManager.HorizontalScrollPositionPX = (hScrollBar.Value * maxScrollPosition) / hScrollBar.Maximum;
            }
            Refresh();
        }

        private void codePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // not strictly part of drawing, but close enough
            vScrollBar.Maximum = GetMaxVerticalScrollPosition() / _viewManager.LineWidth;
            hScrollBar.Maximum = GetMaxHorizontalScrollPosition();
            UpdateLineAndCharacterLabel();

            _viewManager.Draw(new WinformsCanvas(e.Graphics, codePanel.Size, codePanel.Font), Focused);
        }   

        private void UpdateLineAndCharacterLabel()
        {
            int lineNumber = 1 + _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber;
            int columnNumber = 1 + _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber;
            lineLabel.Text = $"Ln: {lineNumber} Ch: {columnNumber}";
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
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
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
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
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
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
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
                EnsureActivePositionInView();
                Refresh();
            }
            else if (_viewManager.Highlighting != null)
            {
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
                string errorMessages = GetErrorMessagesAtPosition(position.LineNumber, position.ColumnNumber);
                if (!string.IsNullOrEmpty(errorMessages))
                {
                    if (hoverToolTip.GetToolTip(codePanel) != errorMessages)
                    {
                        hoverToolTip.SetToolTip(codePanel, errorMessages);
                        hoverToolTip.Tag = null;
                    }
                }
                else
                {
                    int charIndex = position.ToCharacterIndex(_sourceCode.Lines);
                    bool toolTipShown = false;
                    if (charIndex != -1)
                    {

                        CodeCompletionSuggestion? suggestion = _syntaxHighlighter.GetSuggestionAtPosition(charIndex, _viewManager.SyntaxPalette);
                        if (suggestion != null)
                        {
                            toolTipShown = true;
                            (string text, _) = suggestion.ToolTipSource.GetToolTip();
                            if (hoverToolTip.GetToolTip(codePanel) != text)
                            {

                                hoverToolTip.Tag = (suggestion, -1);
                                hoverToolTip.SetToolTip(codePanel, text);
                            }
                        }
                    }
                    if (!toolTipShown)
                    {
                        hoverToolTip.SetToolTip(codePanel, string.Empty);
                        hoverToolTip.Tag = null;
                    }
                }
            }
        }

        private string GetErrorMessagesAtPosition(int currentLine, int currentColumn)
        {
            if (_viewManager.Highlighting == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach ((SourceCodePosition start, SourceCodePosition end, string message) in _viewManager.Highlighting.Errors)
            {
                int startColumn = start.ColumnNumber;
                for (int line = start.LineNumber; line <= end.LineNumber; line++)
                {
                    int endColumn = line == end.LineNumber ? end.ColumnNumber : _sourceCode.Lines.ElementAt(line).Length;
                    if (line == currentLine
                        && currentColumn >= startColumn
                        && currentColumn <= endColumn)
                    {
                        sb.AppendLine(message).AppendLine();
                    }
                    startColumn = 0;
                }
            }
            string errorMessages = sb.ToString();
            return errorMessages;
        }

        private SourceCodePosition GetPositionFromMousePoint(Point point)
        {
            return new SourceCodePosition(Math.Max(0, (point.Y + _viewManager.VerticalScrollPositionPX) / _viewManager.LineWidth),
                Math.Max(0, (point.X + _viewManager.HorizontalScrollPositionPX - _viewManager.GetGutterWidth() - ViewManager.LEFT_MARGIN) / _viewManager.CharacterWidth));
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
                methodToolTip.Hide(codePanel);
            }
        }

        public void ShowCodeCompletionForm()
        {
            HideCodeCompletionForm();
            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            var x = _viewManager.GetXCoordinateFromColumnIndex(head.ColumnNumber);
            var y = _viewManager.GetYCoordinateFromLineIndex(head.LineNumber + 1);
            int position = new SourceCodePosition(head.LineNumber, head.ColumnNumber).ToCharacterIndex(_sourceCode.Lines);
            if (position == -1)
            {
                return;
            }
            CodeCompletionSuggestion[] suggestions = _syntaxHighlighter.GetCodeCompletionSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber).Substring(0, head.ColumnNumber), position, _viewManager.SyntaxPalette).ToArray();
            if (suggestions.Any())
            {
                _codeCompletionSuggestionForm.Show(this, new SourceCodePosition(head.LineNumber, head.ColumnNumber), suggestions, _viewManager.SyntaxPalette);
                _codeCompletionSuggestionForm.Location = PointToScreen(new Point(Location.X + x, Location.Y + y));
                Focus();
            }
        }

        internal void ChooseCodeCompletionItem(string item)
        {
            Source.Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            SourceCodePosition? startPosition = _codeCompletionSuggestionForm.GetPosition();
            if (startPosition != null)
            {
                _sourceCode.RemoveRange(_sourceCode.GetCursor(startPosition.Value.LineNumber, startPosition.Value.ColumnNumber),
                                        _sourceCode.GetCursor(head.LineNumber, head.ColumnNumber));
                _sourceCode.SetActivePosition(startPosition.Value.LineNumber, startPosition.Value.ColumnNumber);
                _sourceCode.InsertStringAtActivePosition(item);
                HideCodeCompletionForm();
                Refresh();
            }
        }

        private void CodeEditorBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // handle key presses that add a new character to the text here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar, _specialCharacterHandler);
                UpdateSyntaxHighlighting();
                EnsureActivePositionInView();

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
                    EnsureActivePositionInView();
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
                    UpdateSyntaxHighlighting();
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
                    UpdateSyntaxHighlighting();
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
                    UpdateSyntaxHighlighting();
                    break;
                case Keys.Tab:
                    if (_codeCompletionSuggestionForm.Visible)
                    {
                        ChooseCodeCompletionItem(_codeCompletionSuggestionForm.GetSelectedItem());
                    }
                    else
                    {
                        if (_sourceCode.SelectionCoversMultipleLines())
                        {
                            if (e.Shift)
                            {
                                _sourceCode.DecreaseIndentOnSelectedLines();
                            }
                            else
                            {
                                _sourceCode.IncreaseIndentOnSelectedLines();
                            }
                        }
                        else
                        {
                            if (e.Shift)
                            {
                                _sourceCode.DecreaseIndentAtActivePosition();
                            }
                            else
                            {
                                _sourceCode.IncreaseIndentAtActivePosition();
                            }
                        }
                    }
                    UpdateSyntaxHighlighting();
                    break;
                default:
                    ensureInView = false;
                    break;
            }
            if (ensureInView)
            {
                EnsureActivePositionInView();
            }
        }

        private void hoverToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            DrawToolTip(hoverToolTip, e);
        }

        private void methodToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            DrawToolTip(methodToolTip, e);
        }

        private void DrawToolTip(ToolTip toolTip, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            Font font = e.Font ?? Font;
            if (toolTip.Tag == null)
            {
                using (Brush brush = new SolidBrush(_viewManager.SyntaxPalette.DefaultTextColour))
                {
                    e.Graphics.DrawString(e.ToolTipText, font, brush, e.Bounds.X, e.Bounds.Y);
                }
                return;
            }
            (CodeCompletionSuggestion tag, int activeParameterIndex) = ((CodeCompletionSuggestion, int))toolTip.Tag;
            if (tag == null
                || tag.ToolTipSource.GetToolTip().toolTip != e.ToolTipText)
            {
                using (Brush brush = new SolidBrush(_viewManager.SyntaxPalette.DefaultTextColour))
                {
                    e.Graphics.DrawString(e.ToolTipText, font, brush, e.Bounds.X, e.Bounds.Y);
                }
            }
            else
            {
                (string toolTipText, List<SyntaxHighlighting> highlightings) = tag.ToolTipSource.GetToolTip();
                Func<int, int> getXCoordinate = characterIndex => e.Bounds.X + 3 + DrawingHelper.GetStringSize(e.ToolTipText.Substring(0, characterIndex), font, e.Graphics).Width;
                DrawingHelper.DrawLine(new WinformsCanvas(e.Graphics, Size.Empty, font), 0, toolTipText, e.Bounds.Y + 1, highlightings, getXCoordinate, _viewManager.SyntaxPalette, activeParameterIndex);
            }
        }

        public void ShowMethodCompletion(SourceCodePosition position, CodeCompletionSuggestion suggestion, int activeParameterIndex)
        {
            //(CodeCompletionSuggestion oldSuggestion, int oldParameterIndex) = ((CodeCompletionSuggestion, int))methodToolTip.Tag;
            methodToolTip.Tag = (suggestion, activeParameterIndex);
            if (suggestion != null
                && !suggestion.IsDeclaration
                && suggestion.SymbolType == SymbolType.Method)
            {
                var x = _viewManager.GetXCoordinateFromColumnIndex(position.ColumnNumber);
                var y = _viewManager.GetYCoordinateFromLineIndex(position.LineNumber + 1);
                methodToolTip.Show(suggestion.ToolTipSource.GetToolTip().toolTip, codePanel, x, y);
            }
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
            UpdateSyntaxHighlighting();
        }

        public void RemoveWordBeforeActivePosition()
        {
            _sourceCode.RemoveWordBeforeActivePosition(_syntaxHighlighter);
            UpdateSyntaxHighlighting();
        }

        public void GoToLastPosition()
        {
            _sourceCode.SetActivePosition(_sourceCode.LineCount, _sourceCode.Lines.Last().Length);
        }

        public void GoToFirstPosition()
        {
            _sourceCode.SetActivePosition(0, 0);
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
            UpdateSyntaxHighlighting();
        }

        public void CopySelectedToClipboard()
        {
            string selectedTextForCopy = _sourceCode.GetSelectedText();
            if (!string.IsNullOrEmpty(selectedTextForCopy))
            {
                Clipboard.SetText(selectedTextForCopy);
            }
        }

        public void CutSelectedToClipboard()
        {
            string selectedTextForCut = _sourceCode.GetSelectedText();
            _sourceCode.RemoveSelectedRange();
            if (!string.IsNullOrEmpty(selectedTextForCut))
            {
                Clipboard.SetText(selectedTextForCut);
            }
            UpdateSyntaxHighlighting();
        }

        public void SelectAll()
        {
            _sourceCode.SelectAll();
        }

        public void ScrollView(int numberOfLines)
        {
            UpdateVerticalScrollPositionPX(_viewManager.VerticalScrollPositionPX + numberOfLines * _viewManager.LineWidth);
        }

        public void DuplicateSelection()
        {
            _sourceCode.DuplicateSelection();
            UpdateSyntaxHighlighting();
        }

        public void SelectionToLowerCase()
        {
            _sourceCode.SelectionToLowerCase();
            UpdateSyntaxHighlighting();
        }

        public void SelectionToUpperCase()
        {
            _sourceCode.SelectionToUpperCase();
            UpdateSyntaxHighlighting();
        }
    }
}