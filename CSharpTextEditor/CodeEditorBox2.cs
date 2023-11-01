using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox2 : UserControl
    {
        private const int LINE_WIDTH = 16;

        private readonly SourceCode _sourceCode;
        private readonly int _characterWidth;
        private int? dragLineStart = null;
        private int? dragColumnStart = null;

        public CodeEditorBox2()
        {
            InitializeComponent();
            _sourceCode = new SourceCode();
            _characterWidth = TextRenderer.MeasureText("A", Font).Width / 2;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            int line = 0;

            bool rangeSelected = _sourceCode.SelectionStart != null;
            int selectionEndLine = _sourceCode.SelectionEnd.LineNumber;
            int selectionStartLine = _sourceCode.SelectionStart?.LineNumber ?? selectionEndLine;
            if (selectionStartLine > selectionEndLine)
            {
                (selectionStartLine, selectionEndLine) = (selectionEndLine, selectionStartLine);
            }
            foreach (string s in _sourceCode.Lines)
            {
                if (rangeSelected
                    && line >= selectionStartLine
                    && line <= selectionEndLine)
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, GetLineSelectionRectangle(line, s.Length));
                }
                e.Graphics.DrawString(s, Font, Brushes.Black, new PointF(0, line * LINE_WIDTH));
                line++;
            }

            if (Focused)
            {
                ISelectionPosition position = _sourceCode.SelectionEnd;
                e.Graphics.DrawLine(Pens.Black,
                    new Point(2 + position.ColumnNumber * _characterWidth, position.LineNumber * LINE_WIDTH),
                    new Point(2 + position.ColumnNumber * _characterWidth, position.LineNumber * LINE_WIDTH + LINE_WIDTH));
            }
        }

        private Rectangle GetLineSelectionRectangle(int lineNumber, int lineCharacterLength)
        {
            var start = _sourceCode.SelectionStart;
            var end = _sourceCode.SelectionEnd;
            if (start.CompareTo(end) > 0)
            {
                (start, end) = (end, start);
            }

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
            int startX = 2 + startCharacterIndex * _characterWidth;
            int endX = 2 + endCharacterIndex * _characterWidth;
            return Rectangle.FromLTRB(startX, lineNumber * LINE_WIDTH, endX, lineNumber * LINE_WIDTH + LINE_WIDTH);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {

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
                _sourceCode.SelectRange((int)dragLineStart, (int)dragColumnStart, currentLine, currentColumn);
                Refresh();
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragLineStart = null;
            dragColumnStart = null;
        }

        private (int line, int column) GetPositionFromMousePoint(Point point)
        {
            return (point.Y / LINE_WIDTH, point.X / _characterWidth);
        }

        private void CodeEditorBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // needed so that the KeyDown event picks up the arrowkeys
            if (e.KeyData.HasFlag(Keys.Right)
                || e.KeyData.HasFlag(Keys.Left)
                || e.KeyData.HasFlag(Keys.Up)
                || e.KeyData.HasFlag(Keys.Down))
            {
                e.IsInputKey = true;
            }
        }

        private void HandleCtrlShortcut(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    _sourceCode.SelectAll();
                    break;
                case Keys.C:
                    Clipboard.SetText(_sourceCode.GetSelectedText());
                    break;
                case Keys.V:
                    _sourceCode.InsertStringAtActivePosition(Clipboard.GetText());
                    break;
            }
        }

        private void CodeEditorBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Handle all "normal" characters here
            if (!char.IsControl(e.KeyChar))
            {
                _sourceCode.InsertCharacterAtActivePosition(e.KeyChar);
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
                switch (e.KeyCode)
                {
                    case Keys.Back:
                        _sourceCode.RemoveCharacterBeforeActivePosition();
                        break;
                    case Keys.Delete:
                        _sourceCode.RemoveCharacterAfterActivePosition();
                        break;

                    case Keys.Left:
                        _sourceCode.ShiftActivePositionToTheLeft(e.Shift);
                        break;
                    case Keys.Right:
                        _sourceCode.ShiftActivePositionToTheRight(e.Shift);
                        break;
                    case Keys.Up:
                        _sourceCode.ShiftActivePositionUpOneLine(e.Shift);
                        break;
                    case Keys.Down:
                        _sourceCode.ShiftActivePositionDownOneLine(e.Shift);
                        break;
                    case Keys.End:
                        _sourceCode.ShiftActivePositionToEndOfLine(e.Shift);
                        break;
                    case Keys.Home:
                        _sourceCode.ShiftActivePositionToStartOfLine(e.Shift);
                        break;

                    case Keys.Enter:
                        _sourceCode.InsertLineBreakAtActivePosition();
                        break;
                    default:
                        /*if (GetCharacterFromKeyCode(e.KeyCode, e.Shift, out char? character)
                            && character != null)
                        {
                            _sourceCode.InsertCharacterAtActivePosition((char)character);
                        }*/

                        break;
                }
            }
            Refresh();
        }
    }
}
