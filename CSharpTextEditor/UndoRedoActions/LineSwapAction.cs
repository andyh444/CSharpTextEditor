using CSharpTextEditor.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class LineSwapAction : UndoRedoAction
    {
        public bool ForwardDown { get; }

        public LineSwapAction(SourceCodePosition positionBefore, SourceCodePosition positionAfter, bool forwardDown)
            : base(positionBefore, positionAfter)
        {
            ForwardDown = forwardDown;
        }

        public override void Redo(SourceCode sourceCode)
        {
            Cursor c = sourceCode.GetCursor(PositionBefore);
            if (ForwardDown)
            {
                c.Line.List.SwapWithNext(c.Line);
            }
            else
            {
                c.Line.List.SwapWithPrevious(c.Line);
            }
        }

        public override void Undo(SourceCode sourceCode)
        {
            Cursor c = sourceCode.GetCursor(PositionAfter);
            if (ForwardDown)
            {
                c.Line.List.SwapWithPrevious(c.Line);
            }
            else
            {
                c.Line.List.SwapWithNext(c.Line);
            }
        }
    }
}
