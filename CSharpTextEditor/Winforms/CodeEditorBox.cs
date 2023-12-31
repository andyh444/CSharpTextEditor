﻿using CSharpTextEditor.CS;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.Winforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox : UserControl, ICodeCompletionHandler
    {
        private const int LEFT_MARGIN = 6;
        private const int CURSOR_OFFSET = 0;

        private readonly SourceCode _sourceCode;
        private readonly HistoryManager _historyManager;
        private int _characterWidth;
        private int _lineWidth;
        private int? dragLineStart = null;
        private int? dragColumnStart = null;
        private int verticalScrollPositionPX;
        private int horizontalScrollPositionPX;
        private SyntaxHighlightingCollection _highlighting;
        private ISpecialCharacterHandler _specialCharacterHandler;
        private ISyntaxHighlighter _syntaxHighlighter;
        private CodeCompletionSuggestionForm _codeCompletionSuggestionForm;
        private SyntaxPalette _syntaxPalette;

        public event EventHandler UndoHistoryChanged;

        public CodeEditorBox()
        {
            InitializeComponent();
            _historyManager = new HistoryManager();
            _historyManager.HistoryChanged += historyManager_HistoryChanged;
            _sourceCode = new SourceCode(string.Empty, _historyManager);

            verticalScrollPositionPX = 0;
            horizontalScrollPositionPX = 0;

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox2_MouseWheel;
            _highlighting = null;

            CSharpSyntaxHighlighter syntaxHighlighter = new CSharpSyntaxHighlighter();
            _syntaxHighlighter = syntaxHighlighter;
            _specialCharacterHandler = new CSharpSpecialCharacterHandler(syntaxHighlighter);
            _codeCompletionSuggestionForm = new CodeCompletionSuggestionForm();
            _codeCompletionSuggestionForm.SetEditorBox(this);
            SetPalette(SyntaxPalette.GetLightModePalette());

            if (Font.Name != "Cascadia Mono")
            {
                Font = new Font("Consolas", Font.Size, Font.Style, Font.Unit);
            }
            UpdateTextSize(panel1.Font);
        }

        private int GetGutterWidth()
        {
            int digitsInLineCount = NumberOfDigits(_sourceCode.LineCount);
            return Math.Max(4, 1 + digitsInLineCount) * _characterWidth;
        }

        private int NumberOfDigits(int value)
        {
            if (value == 0)
            {
                return 1;
            }
            value = Math.Abs(value);
            int count = 0;
            while (value > 0)
            {
                value /= 10;
                count++;
            }
            return count;
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

        public void SetPalette(SyntaxPalette palette)
        {
            _syntaxPalette = palette;
            UpdateSyntaxHighlighting();
            hoverToolTip.BackColor = palette.ToolTipBackColour;
            methodToolTip.BackColor = palette.ToolTipBackColour;
            Refresh();
        }

        private void UpdateTextSize(Font font)
        {
            Size characterSize = DrawingHelper.GetMonospaceCharacterSize(font, panel1.CreateGraphics());
            _characterWidth = characterSize.Width;
            _lineWidth = characterSize.Height;
        }

        private void EnsureActivePositionInView()
        {
            EnsureVerticalActivePositionInView();
            EnsureHorizontalActivePositionInView();
        }

        private void EnsureVerticalActivePositionInView()
        {
            int activeLine = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber;
            int minLineInView = verticalScrollPositionPX / _lineWidth;
            int maxLineInView = (verticalScrollPositionPX + panel1.Height - _lineWidth) / _lineWidth;
            if (activeLine > maxLineInView)
            {
                UpdateVerticalScrollPositionPX(activeLine * _lineWidth - panel1.Height + _lineWidth);
            }
            else if (activeLine < minLineInView)
            {
                UpdateVerticalScrollPositionPX(activeLine * _lineWidth);
            }
        }

        private void EnsureHorizontalActivePositionInView()
        {
            int activeColumn = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber;
            int minColumnInView = horizontalScrollPositionPX / _characterWidth;
            int maxColumnInView = (horizontalScrollPositionPX + panel1.Width - _characterWidth - GetGutterWidth() - LEFT_MARGIN) / _characterWidth;
            if (activeColumn > maxColumnInView)
            {
                UpdateHorizontalScrollPositionPX(activeColumn * _characterWidth - panel1.Width + GetGutterWidth() + LEFT_MARGIN + _characterWidth);
            }
            else if (activeColumn < minColumnInView)
            {
                UpdateHorizontalScrollPositionPX(Math.Max(0, activeColumn - 6) * _characterWidth);
            }
        }

        private void UpdateSyntaxHighlighting()
        {
            _highlighting = _syntaxHighlighter.GetHighlightings(_sourceCode.Lines, _syntaxPalette);
        }

        private void CodeEditorBox2_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                panel1.Font = new Font(panel1.Font.Name, Math.Max(1, panel1.Font.Size + Math.Sign(e.Delta)), panel1.Font.Style, panel1.Font.Unit);
                UpdateTextSize(panel1.Font);
            }
            else
            {
                UpdateVerticalScrollPositionPX(verticalScrollPositionPX - 3 * _lineWidth * Math.Sign(e.Delta));
            }
            Refresh();
        }

        private void UpdateVerticalScrollPositionPX(int newValue)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            verticalScrollPositionPX = Clamp(newValue, 0, maxScrollPosition);
            vScrollBar1.Value = maxScrollPosition == 0 ? 0 : (int)((vScrollBar1.Maximum * (long)verticalScrollPositionPX) / maxScrollPosition);
        }

        private void UpdateHorizontalScrollPositionPX(int newValue)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            horizontalScrollPositionPX = Clamp(newValue, 0, maxScrollPosition);
            hScrollBar1.Value = maxScrollPosition == 0 ? 0 : (int)((hScrollBar1.Maximum * (long)horizontalScrollPositionPX) / maxScrollPosition);
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
            return _sourceCode.Lines.Max(x => x.Length) * _characterWidth;
        }

        private int GetMaxVerticalScrollPosition()
        {
            return (_sourceCode.LineCount - 1) * _lineWidth;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            if (vScrollBar1.Maximum == 0)
            {
                verticalScrollPositionPX = 0;
            }
            else
            {
                verticalScrollPositionPX = (vScrollBar1.Value * maxScrollPosition) / vScrollBar1.Maximum;
            }
            Refresh();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            if (hScrollBar1.Maximum == 0)
            {
                horizontalScrollPositionPX = 0;
            }
            else
            {
                horizontalScrollPositionPX = (hScrollBar1.Value * maxScrollPosition) / hScrollBar1.Maximum;
            }
            Refresh();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // not strictly part of drawing, but close enough
            vScrollBar1.Maximum = GetMaxVerticalScrollPosition() / _lineWidth;
            hScrollBar1.Maximum = GetMaxHorizontalScrollPosition();
            UpdateLineAndCharacterLabel();

            e.Graphics.Clear(_syntaxPalette.BackColour);
            int lineIndex = 0;

            string selectedText = string.Empty;
            if (_sourceCode.SelectionRangeCollection.Count == 1
                && !_sourceCode.SelectionCoversMultipleLines())
            {
                selectedText = _sourceCode.GetSelectedText();
                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    selectedText = string.Empty;
                }
            }

            foreach (string s in _sourceCode.Lines)
            {
                DrawSelectionRectangleOnLine(e.Graphics, lineIndex, selectedText, s);
                int y = GetYCoordinateFromLineIndex(lineIndex);
                if (y > -_lineWidth
                    && y < Height)
                {
                    DrawingHelper.DrawLine(e.Graphics, lineIndex, s, y, panel1.Font, _highlighting?.Highlightings, GetXCoordinateFromColumnIndex, _syntaxPalette);
                }
                lineIndex++;
            }

            DrawErrorSquiggles(e.Graphics);

            if (Focused)
            {
                DrawCursors(e.Graphics);
            }
            DrawLeftGutter(e.Graphics);
        }

        private void DrawCursors(Graphics g)
        {
            using (Pen pen = new Pen(_syntaxPalette.CursorColour))
            {
                foreach (SelectionRange range in _sourceCode.SelectionRangeCollection)
                {
                    Cursor position = range.Head;
                    g.DrawLine(pen,
                        new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber)),
                        new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber) + _lineWidth));
                }
            }
        }

        private void DrawErrorSquiggles(Graphics g)
        {
            if (_highlighting != null)
            {
                foreach ((SourceCodePosition start, SourceCodePosition end, string _) in _highlighting.Errors)
                {
                    int startColumn = start.ColumnNumber;
                    for (int errorLine = start.LineNumber; errorLine <= end.LineNumber; errorLine++)
                    {
                        int endColumn = errorLine == end.LineNumber ? end.ColumnNumber : _sourceCode.Lines.ElementAt(errorLine).Length;
                        int y = errorLine * _lineWidth + _lineWidth - verticalScrollPositionPX;
                        int thisEndColumn = endColumn;
                        if (startColumn == endColumn)
                        {
                            thisEndColumn++;
                        }
                        int startX = GetXCoordinateFromColumnIndex(startColumn);
                        int endX = GetXCoordinateFromColumnIndex(thisEndColumn);
                        using (Pen p = new Pen(Color.Red))
                        {
                            p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                            DrawSquigglyLine(g, p, startX, endX, y);
                        }
                        startColumn = 0;
                    }
                }
            }
        }

        private void DrawSelectionRectangleOnLine(Graphics g, int lineIndex, string selectedText, string line)
        {
            bool selectionRectangleDrawn = false;
            foreach (SelectionRange range in _sourceCode.SelectionRangeCollection)
            {
                bool rangeSelected = range.IsRangeSelected();
                (Cursor startCursor, Cursor endCursor) = range.GetOrderedCursors();
                int selectionEndLine = endCursor.LineNumber;
                int selectionStartLine = startCursor.LineNumber;
                if (selectionStartLine > selectionEndLine)
                {
                    (selectionStartLine, selectionEndLine) = (selectionEndLine, selectionStartLine);
                }
                if (rangeSelected
                    && lineIndex >= selectionStartLine
                    && lineIndex <= selectionEndLine)
                {
                    using (Brush brush = new SolidBrush(Focused ? _syntaxPalette.SelectionColour : _syntaxPalette.DefocusedSelectionColour))
                    {
                        g.FillRectangle(brush, GetLineSelectionRectangle(range, lineIndex, line.Length));
                    }
                    selectionRectangleDrawn = true;
                }
            }
            if (!selectionRectangleDrawn
                && !string.IsNullOrEmpty(selectedText))
            {
                int index = 0;
                do
                {
                    index = line.IndexOf(selectedText, index, StringComparison.CurrentCultureIgnoreCase);
                    if (index != -1)
                    {
                        g.FillRectangle(Brushes.LightGray, GetLineRectangle(index, index + selectedText.Length, lineIndex));
                        index++;
                    }
                } while (index != -1);
            }
        }

        private void DrawLeftGutter(Graphics g)
        {
            int gutterWidth = GetGutterWidth();
            using (Brush brush = new SolidBrush(_syntaxPalette.BackColour))
            {
                int width = gutterWidth;
                g.FillRectangle(brush, 0, 0, width, Height);
            }
            int lastLineCoordinate = GetYCoordinateFromLineIndex(_sourceCode.LineCount);
            g.DrawLine(Pens.Gray, gutterWidth, 0, gutterWidth, lastLineCoordinate);
            if (_highlighting != null)
            {
                foreach (var block in _highlighting.BlockLines)
                {
                    int startLineCoordinate = GetYCoordinateFromLineIndex(block.Item1);
                    int endLineCoordinate = GetYCoordinateFromLineIndex(block.Item2);
                    g.DrawLine(Pens.Gray, gutterWidth, startLineCoordinate, gutterWidth + LEFT_MARGIN - 2, startLineCoordinate);
                    g.DrawLine(Pens.Gray, gutterWidth, endLineCoordinate, gutterWidth + LEFT_MARGIN - 2, endLineCoordinate);
                }
            }
            g.DrawLine(Pens.Gray, gutterWidth, lastLineCoordinate, gutterWidth + LEFT_MARGIN - 2, lastLineCoordinate);
            int lineNumber = 0;

            foreach (string s in _sourceCode.Lines)
            {
                int y = GetYCoordinateFromLineIndex(lineNumber);

                lineNumber++;

                TextRenderer.DrawText(g,
                    lineNumber.ToString(),
                    panel1.Font,
                    new Rectangle(0, y, gutterWidth, y + _lineWidth),
                    Color.Gray,
                    TextFormatFlags.Right);
            }
        }

        private void UpdateLineAndCharacterLabel()
        {
            int lineNumber = 1 + _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber;
            int columnNumber = 1 + _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber;
            lineLabel.Text = $"Ln: {lineNumber} Ch: {columnNumber}";
        }

        private void DrawSquigglyLine(Graphics g, Pen pen, int startX, int endX, int y)
        {
            List<PointF> points = new List<PointF>();
            int ySign = 1;
            float increment = panel1.Font.Size / 3;
            float halfIncrement = increment / 2;
            for (float x = startX; x < endX; x += increment)
            {
                points.Add(new PointF(x, y + halfIncrement * ySign));
                ySign = -ySign;
            }
            if (points.Last().X != endX)
            {
                points.Add(new PointF(endX, y));
            }
            g.DrawLines(pen, points.ToArray());
        }



        private Rectangle GetLineSelectionRectangle(SelectionRange range, int lineNumber, int lineCharacterLength)
        {
            (var start, var end) = range.GetOrderedCursors();

            int selectionEndLine = end.LineNumber;
            int selectionStartLine = start.LineNumber;

            int startCharacterIndex;
            int endCharacterIndex;
            if (lineNumber == selectionStartLine)
            {
                startCharacterIndex = start.ColumnNumber;
                endCharacterIndex = lineNumber == selectionEndLine ? end.ColumnNumber : lineCharacterLength;
            }
            else if (lineNumber == selectionEndLine)
            {
                startCharacterIndex = 0;
                endCharacterIndex = end.ColumnNumber;
            }
            else
            {
                startCharacterIndex = 0;
                endCharacterIndex = lineCharacterLength;
            }
            if (startCharacterIndex > endCharacterIndex)
            {
                (startCharacterIndex, endCharacterIndex) = (endCharacterIndex, startCharacterIndex);
            }
            if (startCharacterIndex == endCharacterIndex)
            {
                endCharacterIndex++;
            }
            int y = GetYCoordinateFromLineIndex(lineNumber);
            return GetLineRectangle(startCharacterIndex, endCharacterIndex, lineNumber);
        }

        private Rectangle GetLineRectangle(int startColumn, int endColumn, int lineNumber)
        {
            int y = GetYCoordinateFromLineIndex(lineNumber);
            return Rectangle.FromLTRB(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(startColumn),
                                      y,
                                      CURSOR_OFFSET + GetXCoordinateFromColumnIndex(endColumn),
                                      y + _lineWidth);
        }

        private int GetXCoordinateFromColumnIndex(int columnIndex)
        {
            return LEFT_MARGIN + GetGutterWidth() + columnIndex * _characterWidth - horizontalScrollPositionPX;
        }

        private int GetYCoordinateFromLineIndex(int lineIndex)
        {
            return lineIndex * _lineWidth - verticalScrollPositionPX;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
                _sourceCode.SelectTokenAtPosition(position, _syntaxHighlighter);
                Refresh();
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }
            else if (e.Button == MouseButtons.Left)
            {
                HideCodeCompletionForm();
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
                (dragLineStart, dragColumnStart) = (position.LineNumber, position.ColumnNumber);
                _sourceCode.SetActivePosition((int)dragLineStart, (int)dragColumnStart);
            }
            Refresh();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragLineStart != null
                && dragColumnStart != null
                && e.Button == MouseButtons.Left)
            {
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
                if (ModifierKeys.HasFlag(Keys.Alt))
                {
                    _sourceCode.ColumnSelect((int)dragLineStart, (int)dragColumnStart, position.LineNumber, position.ColumnNumber);
                }
                else
                {
                    _sourceCode.SelectRange((int)dragLineStart, (int)dragColumnStart, position.LineNumber, position.ColumnNumber);
                }
                EnsureActivePositionInView();
                Refresh();
            }
            else if (_highlighting != null)
            {
                SourceCodePosition position = GetPositionFromMousePoint(e.Location);
                string errorMessages = GetErrorMessagesAtPosition(position.LineNumber, position.ColumnNumber);
                if (!string.IsNullOrEmpty(errorMessages))
                {
                    if (hoverToolTip.GetToolTip(panel1) != errorMessages)
                    {
                        hoverToolTip.SetToolTip(panel1, errorMessages);
                        hoverToolTip.Tag = null;
                    }
                }
                else
                {
                    int charIndex = position.ToCharacterIndex(_sourceCode.Lines);
                    bool toolTipShown = false;
                    if (charIndex != -1)
                    {

                        CodeCompletionSuggestion suggestion = _syntaxHighlighter.GetSuggestionAtPosition(charIndex, _syntaxPalette);
                        if (suggestion != null)
                        {
                            toolTipShown = true;
                            (string text, _) = suggestion.ToolTipSource.GetToolTip();
                            if (hoverToolTip.GetToolTip(panel1) != text)
                            {

                                hoverToolTip.Tag = (suggestion, -1);
                                hoverToolTip.SetToolTip(panel1, text);
                            }
                        }
                    }
                    if (!toolTipShown)
                    {
                        hoverToolTip.SetToolTip(panel1, string.Empty);
                        hoverToolTip.Tag = null;
                    }
                }
            }
        }

        private string GetErrorMessagesAtPosition(int currentLine, int currentColumn)
        {
            StringBuilder sb = new StringBuilder();
            foreach ((SourceCodePosition start, SourceCodePosition end, string message) in _highlighting.Errors)
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

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragLineStart = null;
            dragColumnStart = null;
        }

        private SourceCodePosition GetPositionFromMousePoint(Point point)
        {
            return new SourceCodePosition(Math.Max(0, (point.Y + verticalScrollPositionPX) / _lineWidth),
                Math.Max(0, (point.X + horizontalScrollPositionPX - GetGutterWidth() - LEFT_MARGIN) / _characterWidth));
        }

        private void CodeEditorBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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

        private void HandleCtrlShortcut(KeyEventArgs e)
        {
            bool ensureInView = true;
            switch (e.KeyCode)
            {
                case Keys.A:
                    _sourceCode.SelectAll();
                    break;
                case Keys.X:
                    string selectedTextForCut = _sourceCode.GetSelectedText();
                    _sourceCode.RemoveSelectedRange();
                    if (!string.IsNullOrEmpty(selectedTextForCut))
                    {
                        Clipboard.SetText(selectedTextForCut);
                    }
                    UpdateSyntaxHighlighting();
                    break;
                case Keys.C:
                    string selectedTextForCopy = _sourceCode.GetSelectedText();
                    if (!string.IsNullOrEmpty(selectedTextForCopy))
                    {
                        Clipboard.SetText(selectedTextForCopy);
                    }
                    break;
                case Keys.V:
                    _sourceCode.InsertStringAtActivePosition(Clipboard.GetText());
                    UpdateSyntaxHighlighting();
                    break;

                case Keys.Y:
                    _sourceCode.Redo();
                    UpdateSyntaxHighlighting();
                    break;
                case Keys.Z:
                    _sourceCode.Undo();
                    UpdateSyntaxHighlighting();
                    break;


                case Keys.Left:
                    _sourceCode.ShiftHeadOneWordToTheLeft(_syntaxHighlighter, e.Shift);
                    break;
                case Keys.Right:
                    _sourceCode.ShiftHeadOneWordToTheRight(_syntaxHighlighter, e.Shift);
                    break;

                case Keys.Home:
                    _sourceCode.SetActivePosition(0, 0);
                    break;
                case Keys.End:
                    _sourceCode.SetActivePosition(_sourceCode.LineCount, _sourceCode.Lines.Last().Length);
                    break;

                case Keys.Back:
                    _sourceCode.RemoveWordBeforeActivePosition(_syntaxHighlighter);
                    UpdateSyntaxHighlighting();
                    break;
                case Keys.Delete:
                    _sourceCode.RemoveWordAfterActivePosition(_syntaxHighlighter);
                    UpdateSyntaxHighlighting();
                    break;
                default:
                    ensureInView = false;
                    break;
            }
            if (ensureInView)
            {
                HideCodeCompletionForm();
                EnsureActivePositionInView();
            }
        }

        public void HideCodeCompletionForm(bool hideMethodToolTipToo = true)
        {
            _codeCompletionSuggestionForm.Hide();
            if (hideMethodToolTipToo)
            {
                methodToolTip.Hide(panel1);
            }
        }

        public void ShowCodeCompletionForm()
        {
            HideCodeCompletionForm();
            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            var x = GetXCoordinateFromColumnIndex(head.ColumnNumber);
            var y = GetYCoordinateFromLineIndex(head.LineNumber + 1);
            int position = new SourceCodePosition(head.LineNumber, head.ColumnNumber).ToCharacterIndex(_sourceCode.Lines);
            if (position == -1)
            {
                return;
            }
            CodeCompletionSuggestion[] suggestions = _syntaxHighlighter.GetCodeCompletionSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber).Substring(0, head.ColumnNumber), position, _syntaxPalette).ToArray();
            if (suggestions.Any())
            {
                _codeCompletionSuggestionForm.Show(this, new SourceCodePosition(head.LineNumber, head.ColumnNumber), suggestions, _syntaxPalette);
                _codeCompletionSuggestionForm.Location = PointToScreen(new Point(Location.X + x, Location.Y + y));
                Focus();
            }
        }

        internal void ChooseCodeCompletionItem(string item)
        {
            CSharpTextEditor.Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
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

        private void CodeEditorBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Handle all "normal" characters here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar, _specialCharacterHandler);
                UpdateSyntaxHighlighting();
                EnsureActivePositionInView();

                _specialCharacterHandler.HandleCharacterInserted(e.KeyChar, _sourceCode, this, _syntaxPalette);

                if (_codeCompletionSuggestionForm.Visible)
                {
                    Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                    _codeCompletionSuggestionForm.FilterSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
                }
                Refresh();
            }
        }

        private void CodeEditorBox2_KeyDown(object sender, KeyEventArgs e)
        {
            // handle control keys here
            if (e.Control)
            {
                HandleCtrlShortcut(e);
            }
            else
            {
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
                            if (head.ColumnNumber < _codeCompletionSuggestionForm.GetPosition().Value.ColumnNumber)
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
                        _sourceCode.ShiftHeadUpLines(Height / _lineWidth, e.Shift);
                        break;
                    case Keys.PageDown:
                        HideCodeCompletionForm();
                        _sourceCode.ShiftHeadDownLines(Height / _lineWidth, e.Shift);
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
            Refresh();
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
            if (toolTip.Tag == null)
            {
                using (Brush brush = new SolidBrush(_syntaxPalette.DefaultTextColour))
                {
                    e.Graphics.DrawString(e.ToolTipText, e.Font, brush, e.Bounds.X, e.Bounds.Y);
                }
                return;
            }
            (CodeCompletionSuggestion tag, int activeParameterIndex) = ((CodeCompletionSuggestion, int))toolTip.Tag;
            if (tag == null
                || tag.ToolTipSource.GetToolTip().toolTip != e.ToolTipText)
            {
                using (Brush brush = new SolidBrush(_syntaxPalette.DefaultTextColour))
                {
                    e.Graphics.DrawString(e.ToolTipText, e.Font, brush, e.Bounds.X, e.Bounds.Y);
                }
            }
            else
            {
                (string toolTipText, List<SyntaxHighlighting> highlightings) = tag.ToolTipSource.GetToolTip();
                Func<int, int> getXCoordinate = characterIndex => e.Bounds.X + 3 + DrawingHelper.GetStringSize(e.ToolTipText.Substring(0, characterIndex), e.Font, e.Graphics).Width;
                DrawingHelper.DrawLine(e.Graphics, 0, toolTipText, e.Bounds.Y + 1, e.Font, highlightings, getXCoordinate, _syntaxPalette, activeParameterIndex);
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
                var x = GetXCoordinateFromColumnIndex(position.ColumnNumber);
                var y = GetYCoordinateFromLineIndex(position.LineNumber + 1);
                methodToolTip.Show(suggestion.ToolTipSource.GetToolTip().toolTip, panel1, x, y);
            }
        }
    }
}