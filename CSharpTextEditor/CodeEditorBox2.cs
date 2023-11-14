using System.ComponentModel;
using System.Data;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox2 : UserControl
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
        private SyntaxHighlightingCollection? _highlighting;
        private ISpecialCharacterHandler _specialCharacterHandler;
        private ISyntaxHighlighter _syntaxHighlighter;

        [Browsable(true)]
        public new string Text { get; set; }

        public CodeEditorBox2()
        {
            InitializeComponent();
            _sourceCode = new SourceCode(Text ?? string.Empty);
            UpdateTextSize(panel1.Font);
            //_characterWidth = 0.5 * TextRenderer.MeasureText("A", Font).Width;
            verticalScrollPositionPX = 0;
            horizontalScrollPositionPX = 0;

            // the MouseWheel event doesn't show up in the designer for some reason
            MouseWheel += CodeEditorBox2_MouseWheel;
            _highlighting = null;
            _specialCharacterHandler = new CSharpSpecialCharacterHandler();
            _syntaxHighlighter = new CSharpSyntaxHighlighter(charIndex => SourceCodePosition.FromCharacterIndex(charIndex, _sourceCode.Lines));
        }

        private void UpdateTextSize(Font font)
        {
            Size characterSize = TextRenderer.MeasureText("A", font, new Size(), TextFormatFlags.NoPadding);
            _characterWidth = characterSize.Width;
            _lineWidth = characterSize.Height;
        }

        private void EnsureActivePositionInView()
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

        private void UpdateSyntaxHighlighting()
        {
            _highlighting = _syntaxHighlighter.GetHighlightings(_sourceCode.Text, SyntaxPalette.GetLightModePalette());
        }

        private void CodeEditorBox2_MouseWheel(object? sender, MouseEventArgs e)
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
            verticalScrollPositionPX = Math.Clamp(newValue, 0, maxScrollPosition);
            vScrollBar1.Value = maxScrollPosition == 0 ? 0 : (vScrollBar1.Maximum * verticalScrollPositionPX) / maxScrollPosition;
        }

        private int GetMaxHorizontalScrollPosition()
        {
            return _sourceCode.Lines.Max(x => x.Length) * _characterWidth;
        }

        private int GetMaxVerticalScrollPosition()
        {
            return (_sourceCode.Lines.Count - 1) * _lineWidth;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxVerticalScrollPosition();
            verticalScrollPositionPX = (vScrollBar1.Value * maxScrollPosition) / vScrollBar1.Maximum;
            Refresh();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int maxScrollPosition = GetMaxHorizontalScrollPosition();
            horizontalScrollPositionPX = (hScrollBar1.Value * maxScrollPosition) / hScrollBar1.Maximum;
            Refresh();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // not strictly part of drawing, but close enough
            vScrollBar1.Maximum = GetMaxVerticalScrollPosition() / _lineWidth;
            hScrollBar1.Maximum = GetMaxHorizontalScrollPosition();
            UpdateLineAndCharacterLabel();

            e.Graphics.Clear(Color.White);
            int line = 0;

            foreach (string s in _sourceCode.Lines)
            {
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
                        && line >= selectionStartLine
                        && line <= selectionEndLine)
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue, GetLineSelectionRectangle(range, line, s.Length));
                    }
                }
                if (_highlighting == null
                    || !TryGetStringsToDraw(s, line, _highlighting.Highlightings.Where(x => x.IsOnLine(line)).Distinct(new SyntaxHighlightingEqualityComparer()).ToList(), out var stringsToDraw))
                {
                    TextRenderer.DrawText(e.Graphics, s, panel1.Font, new Point(GetXCoordinateFromColumnIndex(0), GetYCoordinateFromLineIndex(line)), Color.Black, TextFormatFlags.NoPadding);
                    //e.Graphics.DrawString(s, Font, Brushes.Black, new PointF(GetXCoordinateFromColumnIndex(0), GetYCoordinateFromLineIndex(line)));
                }
                else
                {
                    foreach ((string text, int characterOffset, Color colour) in stringsToDraw)
                    {
                        using (Brush brush = new SolidBrush(colour))
                        {
                            //e.Graphics.DrawString(text, Font, brush, new PointF(GetXCoordinateFromColumnIndex(characterOffset), GetYCoordinateFromLineIndex(line)));
                            TextRenderer.DrawText(e.Graphics, text, panel1.Font, new Point(GetXCoordinateFromColumnIndex(characterOffset), GetYCoordinateFromLineIndex(line)), colour, TextFormatFlags.NoPadding);
                        }
                    }
                }


                line++;
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
                        DrawSquigglyLine(e.Graphics, Pens.Red, startX, endX, y);
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
            int lastLineCoordinate = GetYCoordinateFromLineIndex(_sourceCode.Lines.Count);
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

        private bool TryGetStringsToDraw(string originalLine, int lineIndex, IEnumerable<SyntaxHighlighting> highlightingsOnLine, out List<(string text, int characterOffset, Color colour)> stringsToDraw)
        {
            int start = 0;
            int characterCount = 0;
            stringsToDraw = new List<(string text, int characterOffset, Color colour)>();
            foreach (SyntaxHighlighting highlighting in highlightingsOnLine)
            {
                if (highlighting.Start.LineNumber == highlighting.End.LineNumber)
                {
                    if (highlighting.Start.ColumnNumber - start < 0)
                    {
                        return false;
                    }
                    string before = originalLine.Substring(start, highlighting.Start.ColumnNumber - start);
                    stringsToDraw.Add((before, characterCount, Color.Black));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber, highlighting.End.ColumnNumber - highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else if (highlighting.Start.LineNumber == lineIndex)
                {
                    string before = originalLine.Substring(0, highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((before, characterCount, Color.Black));

                    characterCount += before.Length;

                    string highlightedText = originalLine.Substring(highlighting.Start.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = originalLine.Length;
                }
                else if (highlighting.End.LineNumber == lineIndex)
                {
                    string highlightedText = originalLine.Substring(0, highlighting.End.ColumnNumber);
                    stringsToDraw.Add((highlightedText, characterCount, highlighting.Colour));

                    characterCount += highlightedText.Length;

                    start = highlighting.End.ColumnNumber;
                }
                else
                {
                    stringsToDraw.Add((originalLine, 0, highlighting.Colour));
                    characterCount += originalLine.Length;
                    start = originalLine.Length;
                }
            }
            if (start != originalLine.Length)
            {
                stringsToDraw.Add((originalLine.Substring(start), characterCount, Color.Black));
            }
            return true;
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
            return Rectangle.FromLTRB(CURSOR_OFFSET + GetXCoordinateFromColumnIndex(startCharacterIndex),
                                      y,
                                      CURSOR_OFFSET + GetXCoordinateFromColumnIndex(endCharacterIndex),
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
                bool hoveringOverError = false;
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
                            if (toolTip1.GetToolTip(panel1) != message)
                            {
                                toolTip1.SetToolTip(panel1, message);
                            }
                            hoveringOverError = true;
                            break;
                        }
                        startColumn = 0;
                    }
                }
                if (!hoveringOverError)
                {
                    toolTip1.SetToolTip(panel1, string.Empty);
                }
            }
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
                    _sourceCode.SetActivePosition(_sourceCode.Lines.Count, _sourceCode.Lines.Last().Length);
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
                EnsureActivePositionInView();
            }
        }

        private void CodeEditorBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Handle all "normal" characters here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar, _specialCharacterHandler);
                UpdateSyntaxHighlighting();
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
                    case Keys.Back:
                        _sourceCode.RemoveCharacterBeforeActivePosition();
                        UpdateSyntaxHighlighting();
                        break;
                    case Keys.Delete:
                        _sourceCode.RemoveCharacterAfterActivePosition();
                        UpdateSyntaxHighlighting();
                        break;

                    case Keys.Left:
                        _sourceCode.ShiftHeadToTheLeft(e.Shift);
                        break;
                    case Keys.Right:
                        _sourceCode.ShiftHeadToTheRight(e.Shift);
                        break;
                    case Keys.Up:
                        _sourceCode.ShiftHeadUpOneLine(e.Shift);
                        break;
                    case Keys.Down:
                        _sourceCode.ShiftHeadDownOneLine(e.Shift);
                        break;
                    case Keys.End:
                        _sourceCode.ShiftHeadToEndOfLine(e.Shift);
                        break;
                    case Keys.Home:
                        _sourceCode.ShiftHeadToStartOfLine(e.Shift);
                        break;
                    case Keys.PageUp:
                        _sourceCode.ShiftHeadUpLines(Height / _lineWidth, e.Shift);
                        break;
                    case Keys.PageDown:
                        _sourceCode.ShiftHeadDownLines(Height / _lineWidth, e.Shift);
                        break;

                    case Keys.Enter:
                        _sourceCode.InsertLineBreakAtActivePosition(_specialCharacterHandler);
                        UpdateSyntaxHighlighting();
                        break;
                    case Keys.Tab:
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
                            _sourceCode.InsertStringAtActivePosition(SourceCode.TAB_REPLACEMENT);
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