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

            foreach (string s in _sourceCode.Lines)
            {
                if (rangeSelected
                    && line >= selectionStartLine
                    && line <= selectionEndLine)
                {
                    if (line == selectionStartLine)
                    {
                        int end = (line == selectionEndLine ? _sourceCode.SelectionEnd.ColumnNumber : s.Length) - 1;
                        e.Graphics.FillRectangle(Brushes.LightBlue,
                            2 + _sourceCode.SelectionStart.ColumnNumber * _characterWidth,
                            line * LINE_WIDTH,
                            2 + end * _characterWidth,
                            line * LINE_WIDTH + LINE_WIDTH);
                    }
                    else if (line == selectionEndLine)
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue,
                            2,
                            line * LINE_WIDTH,
                            2 + _sourceCode.SelectionEnd.ColumnNumber * _characterWidth,
                            line * LINE_WIDTH + LINE_WIDTH);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue,
                            2,
                            line * LINE_WIDTH,
                            2 + s.Length * _characterWidth,
                            line * LINE_WIDTH + LINE_WIDTH);
                    }
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

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        private void CodeEditorBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }
            else if (e.Button == MouseButtons.Left)
            {
                int lineNumber = e.Y / LINE_WIDTH;
                int columnNumber = e.X / _characterWidth;
                _sourceCode.SetActivePosition(lineNumber, columnNumber);
            }
            Refresh();
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

        private void CodeEditorBox2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode) // backspace
            {
                case Keys.Back:
                    _sourceCode.RemoveCharacterBeforePosition();
                    break;
                case Keys.Delete:
                    _sourceCode.RemoveCharacterAfterPosition();
                    break;

                case Keys.Left:
                    _sourceCode.ShiftActivePositionToTheLeft();
                    break;
                case Keys.Right:
                    _sourceCode.ShiftActivePositionToTheRight(e.Shift);
                    break;
                case Keys.Up:
                    _sourceCode.ShiftActivePositionUpOneLine();
                    break;
                case Keys.Down:
                    _sourceCode.ShiftActivePositionDownOneLine();
                    break;
                case Keys.End:
                    _sourceCode.ShiftActivePositionToEndOfLine();
                    break;
                case Keys.Home:
                    _sourceCode.ShiftActivePositionToStartOfLine();
                    break;

                case Keys.Enter:
                    _sourceCode.InsertLineBreakAtPosition();
                    break;
                default:
                    if (GetCharacterFromKeyCode(e.KeyCode, e.Shift, out char? character)
                        && character != null)
                    {
                        _sourceCode.InsertCharacterAtPosition((char)character);
                    }

                    break;
            }
            Refresh();
        }

        private bool GetCharacterFromKeyCode(Keys key, bool shift, out char? character)
        {
            character = null;
            switch (key)
            {
                // letters
                case Keys.A: character = shift ? 'A' : 'a'; break;
                case Keys.B: character = shift ? 'B' : 'b'; break;
                case Keys.C: character = shift ? 'C' : 'c'; break;
                case Keys.D: character = shift ? 'D' : 'd'; break;
                case Keys.E: character = shift ? 'E' : 'e'; break;
                case Keys.F: character = shift ? 'F' : 'f'; break;
                case Keys.G: character = shift ? 'G' : 'g'; break;
                case Keys.H: character = shift ? 'H' : 'h'; break;
                case Keys.I: character = shift ? 'I' : 'i'; break;
                case Keys.J: character = shift ? 'J' : 'j'; break;
                case Keys.K: character = shift ? 'K' : 'k'; break;
                case Keys.L: character = shift ? 'L' : 'l'; break;
                case Keys.M: character = shift ? 'M' : 'm'; break;
                case Keys.N: character = shift ? 'N' : 'n'; break;
                case Keys.O: character = shift ? 'O' : 'o'; break;
                case Keys.P: character = shift ? 'P' : 'p'; break;
                case Keys.Q: character = shift ? 'Q' : 'q'; break;
                case Keys.R: character = shift ? 'R' : 'r'; break;
                case Keys.S: character = shift ? 'S' : 's'; break;
                case Keys.T: character = shift ? 'T' : 't'; break;
                case Keys.U: character = shift ? 'U' : 'u'; break;
                case Keys.V: character = shift ? 'V' : 'v'; break;
                case Keys.W: character = shift ? 'W' : 'w'; break;
                case Keys.X: character = shift ? 'X' : 'x'; break;
                case Keys.Y: character = shift ? 'Y' : 'y'; break;
                case Keys.Z: character = shift ? 'Z' : 'z'; break;

                case Keys.Space: character = ' '; break;
            }
            return character != null;
        }
    }
}
