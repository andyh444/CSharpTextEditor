using CSharpTextEditor.CS;
using CSharpTextEditor.Winforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox : UserControl
    {
        private const int LEFT_GUTTER_WIDTH = 64;
        private const int LEFT_MARGIN = 6;
        private const int CURSOR_OFFSET = 0;

        private readonly SourceCode _sourceCode;
        private int _characterWidth;
        private int _lineWidth;
        private int? dragLineStart = null;
        private int? dragColumnStart = null;
        private int verticalScrollPositionPX;
        private int horizontalScrollPositionPX;
        private SyntaxHighlightingCollection _highlighting;
        private ISpecialCharacterHandler _specialCharacterHandler;
        private ISyntaxHighlighter _syntaxHighlighter;
        private CodeCompletionSuggestionForm codeCompletionSuggestionForm;

        public CodeEditorBox()
        {
            InitializeComponent();
            _sourceCode = new SourceCode(string.Empty);

            verticalScrollPositionPX = 0;
            horizontalScrollPositionPX = 0;

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox2_MouseWheel;
            _highlighting = null;
            _specialCharacterHandler = new CSharpSpecialCharacterHandler();
            _syntaxHighlighter = new CSharpSyntaxHighlighter();
            codeCompletionSuggestionForm = new CodeCompletionSuggestionForm();
            codeCompletionSuggestionForm.SetEditorBox(this);

            if (Font.Name != "Cascadia Mono")
            {
                Font = new Font("Consolas", Font.Size, Font.Style, Font.Unit);
            }
            UpdateTextSize(panel1.Font);
        }

        public string GetText() => _sourceCode.Text;

        public void SetText(string text)
        {
            _sourceCode.Text = text;
            UpdateSyntaxHighlighting();
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
            int maxColumnInView = (horizontalScrollPositionPX + panel1.Width - _characterWidth - LEFT_GUTTER_WIDTH - LEFT_MARGIN) / _characterWidth;
            if (activeColumn > maxColumnInView)
            {
                UpdateHorizontalScrollPositionPX(activeColumn * _characterWidth - panel1.Width + LEFT_GUTTER_WIDTH + LEFT_MARGIN + _characterWidth);
            }
            else if (activeColumn < minColumnInView)
            {
                UpdateHorizontalScrollPositionPX(Math.Max(0, activeColumn - 6) * _characterWidth);
            }
        }

        private void UpdateSyntaxHighlighting()
        {
            _highlighting = _syntaxHighlighter.GetHighlightings(_sourceCode.Lines, SyntaxPalette.GetLightModePalette());
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
            vScrollBar1.Value = maxScrollPosition == 0 ? 0 : (vScrollBar1.Maximum * verticalScrollPositionPX) / maxScrollPosition;
        }

        private void UpdateHorizontalScrollPositionPX(int newValue)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            horizontalScrollPositionPX = Clamp(newValue, 0, maxScrollPosition);
            hScrollBar1.Value = maxScrollPosition == 0 ? 0 : (hScrollBar1.Maximum * horizontalScrollPositionPX) / maxScrollPosition;
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
            //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // not strictly part of drawing, but close enough
            vScrollBar1.Maximum = GetMaxVerticalScrollPosition() / _lineWidth;
            hScrollBar1.Maximum = GetMaxHorizontalScrollPosition();
            UpdateLineAndCharacterLabel();

            e.Graphics.Clear(Color.White);
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
                        e.Graphics.FillRectangle(Focused ? Brushes.LightBlue : Brushes.LightGray, GetLineSelectionRectangle(range, lineIndex, s.Length));
                        selectionRectangleDrawn = true;
                    }
                }
                if (!selectionRectangleDrawn
                    && !string.IsNullOrEmpty(selectedText))
                {
                    int index = 0;
                    do
                    {
                        index = s.IndexOf(selectedText, index, StringComparison.CurrentCultureIgnoreCase);
                        if (index != -1)
                        {
                            e.Graphics.FillRectangle(Brushes.LightGray, GetLineRectangle(index, index + selectedText.Length, lineIndex));
                            index++;
                        }
                    } while (index != -1);
                }
                int y = GetYCoordinateFromLineIndex(lineIndex);
                if (y > -_lineWidth
                    && y < Height)
                {
                    if (_highlighting == null
                        || !DrawingHelper.TryGetStringsToDraw(s, lineIndex, _highlighting.Highlightings.Where(x => x.IsOnLine(lineIndex)).Distinct(new SyntaxHighlightingEqualityComparer()).ToList(), out var stringsToDraw))
                    {
                        TextRenderer.DrawText(e.Graphics, s, panel1.Font, new Point(GetXCoordinateFromColumnIndex(0), y), Color.Black, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    }
                    else
                    {
                        foreach ((string text, int characterOffset, Color colour) in stringsToDraw)
                        {
                            using (Brush brush = new SolidBrush(colour))
                            {
                                TextRenderer.DrawText(e.Graphics, text, panel1.Font, new Point(GetXCoordinateFromColumnIndex(characterOffset), y), colour, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                            }
                        }
                    }
                }


                lineIndex++;
            }

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
                            DrawSquigglyLine(e.Graphics, p, startX, endX, y);
                        }
                        startColumn = 0;
                    }
                }
            }

            if (Focused)
            {
                foreach (SelectionRange range in _sourceCode.SelectionRangeCollection)
                {
                    Cursor position = range.Head;
                    e.Graphics.DrawLine(Pens.Black,
                        new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber)),
                        new Point(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(position.ColumnNumber), GetYCoordinateFromLineIndex(position.LineNumber) + _lineWidth));
                }
            }
            DrawLeftGutter(e.Graphics);
        }

        private void DrawLeftGutter(Graphics g)
        {
            g.FillRectangle(Brushes.White, 0, 0, LEFT_GUTTER_WIDTH, Height);
            int lastLineCoordinate = GetYCoordinateFromLineIndex(_sourceCode.LineCount);
            g.DrawLine(Pens.Gray, LEFT_GUTTER_WIDTH, 0, LEFT_GUTTER_WIDTH, lastLineCoordinate);
            if (_highlighting != null)
            {
                foreach (var block in _highlighting.BlockLines)
                {
                    int startLineCoordinate = GetYCoordinateFromLineIndex(block.Item1);
                    int endLineCoordinate = GetYCoordinateFromLineIndex(block.Item2);
                    g.DrawLine(Pens.Gray, LEFT_GUTTER_WIDTH, startLineCoordinate, LEFT_GUTTER_WIDTH + LEFT_MARGIN - 2, startLineCoordinate);
                    g.DrawLine(Pens.Gray, LEFT_GUTTER_WIDTH, endLineCoordinate, LEFT_GUTTER_WIDTH + LEFT_MARGIN - 2, endLineCoordinate);
                }
            }
            g.DrawLine(Pens.Gray, LEFT_GUTTER_WIDTH, lastLineCoordinate, LEFT_GUTTER_WIDTH + LEFT_MARGIN - 2, lastLineCoordinate);
            int lineCount = 0;
            foreach (string s in _sourceCode.Lines)
            {
                TextRenderer.DrawText(g,
                    lineCount.ToString(),
                    panel1.Font,
                    new Point(LEFT_GUTTER_WIDTH / 3, GetYCoordinateFromLineIndex(lineCount)),
                    Color.Gray);
                lineCount++;
            }
        }

        private void UpdateLineAndCharacterLabel()
        {
            lineLabel.Text = $"Ln: {_sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber} Ch: {_sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber}";
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
            return LEFT_MARGIN + LEFT_GUTTER_WIDTH + columnIndex * _characterWidth - horizontalScrollPositionPX;
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
                (int line, int column) = GetPositionFromMousePoint(e.Location);
                _sourceCode.SelectTokenAtPosition(new SourceCodePosition(line, column), _syntaxHighlighter);
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
                codeCompletionSuggestionForm.Hide();
                (dragLineStart, dragColumnStart) = GetPositionFromMousePoint(e.Location);
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
                (int currentLine, int currentColumn) = GetPositionFromMousePoint(e.Location);
                if (ModifierKeys.HasFlag(Keys.Alt))
                {
                    _sourceCode.ColumnSelect((int)dragLineStart, (int)dragColumnStart, currentLine, currentColumn);
                }
                else
                {
                    _sourceCode.SelectRange((int)dragLineStart, (int)dragColumnStart, currentLine, currentColumn);
                }
                Refresh();
            }
            else if (_highlighting != null)
            {
                (int currentLine, int currentColumn) = GetPositionFromMousePoint(e.Location);
                string errorMessages = GetErrorMessagesAtPosition(currentLine, currentColumn);
                if (!string.IsNullOrEmpty(errorMessages))
                {
                    if (toolTip1.GetToolTip(panel1) != errorMessages)
                    {
                        toolTip1.SetToolTip(panel1, errorMessages);
                    }
                }
                else
                {
                    toolTip1.SetToolTip(panel1, string.Empty);
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

        private (int line, int column) GetPositionFromMousePoint(Point point)
        {
            return (Math.Max(0, (point.Y + verticalScrollPositionPX) / _lineWidth),
                Math.Max(0, (point.X + horizontalScrollPositionPX - LEFT_GUTTER_WIDTH - LEFT_MARGIN) / _characterWidth));
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
                codeCompletionSuggestionForm.Hide();
                EnsureActivePositionInView();
            }
        }

        private void ShowCodeCompletionForm()
        {
            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            var x = GetXCoordinateFromColumnIndex(head.ColumnNumber);
            var y = GetYCoordinateFromLineIndex(head.LineNumber + 1);
            int position = new SourceCodePosition(head.LineNumber, head.ColumnNumber).ToCharacterIndex(_sourceCode.Lines);
            CodeCompletionSuggestion[] suggestions = _syntaxHighlighter.GetCodeCompletionSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber).Substring(0, head.ColumnNumber), position, SyntaxPalette.GetLightModePalette()).ToArray();
            if (suggestions.Any())
            {
                codeCompletionSuggestionForm.Show(this, new SourceCodePosition(head.LineNumber, head.ColumnNumber), suggestions);
                codeCompletionSuggestionForm.Location = PointToScreen(new Point(Location.X + x, Location.Y + y));
                Focus();
            }
        }

        internal void ChooseCodeCompletionItem(string item)
        {
            CSharpTextEditor.Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            SourceCodePosition? startPosition = codeCompletionSuggestionForm.GetPosition();
            if (startPosition != null)
            {
                _sourceCode.RemoveRange(_sourceCode.GetPosition(startPosition.Value.LineNumber, startPosition.Value.ColumnNumber),
                                        _sourceCode.GetPosition(head.LineNumber, head.ColumnNumber));
                _sourceCode.SetActivePosition(startPosition.Value.LineNumber, startPosition.Value.ColumnNumber);
                _sourceCode.InsertStringAtActivePosition(item);
                codeCompletionSuggestionForm.Hide();
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
                if (e.KeyChar == '.')
                {
                    if (_sourceCode.SelectionRangeCollection.Count == 1)
                    {
                        if (codeCompletionSuggestionForm.Visible)
                        {
                            codeCompletionSuggestionForm.Hide();
                        }
                        ShowCodeCompletionForm();
                    }
                }
                else if (!char.IsLetterOrDigit(e.KeyChar))
                {
                    codeCompletionSuggestionForm.Hide();
                }
                if (codeCompletionSuggestionForm.Visible)
                {
                    Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                    codeCompletionSuggestionForm.FilterSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
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
                        codeCompletionSuggestionForm.Hide();
                        break;
                    case Keys.Back:
                        _sourceCode.RemoveCharacterBeforeActivePosition();
                        UpdateSyntaxHighlighting();
                        if (codeCompletionSuggestionForm.Visible)
                        {
                            Cursor head = _sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
                            if (head.ColumnNumber < codeCompletionSuggestionForm.GetPosition().Value.ColumnNumber)
                            {
                                codeCompletionSuggestionForm.Hide();
                            }
                            else
                            {
                                codeCompletionSuggestionForm.FilterSuggestions(_sourceCode.Lines.ElementAt(head.LineNumber), head.ColumnNumber);
                            }
                        }
                        break;
                    case Keys.Delete:
                        _sourceCode.RemoveCharacterAfterActivePosition();
                        UpdateSyntaxHighlighting();
                        break;

                    case Keys.Left:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.ShiftHeadToTheLeft(e.Shift);
                        break;
                    case Keys.Right:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.ShiftHeadToTheRight(e.Shift);
                        break;
                    case Keys.Up:
                        if (codeCompletionSuggestionForm.Visible)
                        {
                            codeCompletionSuggestionForm.MoveSelectionUp();
                        }
                        else
                        {
                            _sourceCode.ShiftHeadUpOneLine(e.Shift);
                        }
                        break;
                    case Keys.Down:
                        if (codeCompletionSuggestionForm.Visible)
                        {
                            codeCompletionSuggestionForm.MoveSelectionDown();
                        }
                        else
                        {
                            _sourceCode.ShiftHeadDownOneLine(e.Shift);
                        }
                        break;
                    case Keys.End:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.ShiftHeadToEndOfLine(e.Shift);
                        break;
                    case Keys.Home:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.ShiftHeadToStartOfLine(e.Shift);
                        break;
                    case Keys.PageUp:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.ShiftHeadUpLines(Height / _lineWidth, e.Shift);
                        break;
                    case Keys.PageDown:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.ShiftHeadDownLines(Height / _lineWidth, e.Shift);
                        break;

                    case Keys.Enter:
                        codeCompletionSuggestionForm.Hide();
                        _sourceCode.InsertLineBreakAtActivePosition(_specialCharacterHandler);
                        UpdateSyntaxHighlighting();
                        break;
                    case Keys.Tab:
                        if (codeCompletionSuggestionForm.Visible)
                        {
                            ChooseCodeCompletionItem(codeCompletionSuggestionForm.GetSelectedItem());
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
    }
}