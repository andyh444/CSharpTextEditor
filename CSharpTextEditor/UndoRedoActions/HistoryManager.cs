using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.UndoRedoActions
{
    internal class HistoryManager
    {
        private readonly Stack<IReadOnlyCollection<UndoRedoAction>> _undoStack;
        private readonly Stack<IReadOnlyCollection<UndoRedoAction>> _redoStack;

        public HistoryManager()
        {
            _undoStack = new Stack<IReadOnlyCollection<UndoRedoAction>>();
            _redoStack = new Stack<IReadOnlyCollection<UndoRedoAction>>();
        }

        public void AddAction(IReadOnlyCollection<UndoRedoAction> action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();
        }

        public void Undo(SourceCode sourceCode)
        {
            if (_undoStack.Count == 0)
            {
                return;
            }
            IReadOnlyCollection<UndoRedoAction> actions = _undoStack.Pop();
            bool multipleCursors = false;
            foreach (UndoRedoAction action in actions.Reverse().OrderBy(x => (x is CursorMoveAction) ? 1 : 0))
            {
                action.Undo(sourceCode, multipleCursors);
                if (action is CursorMoveAction)
                {
                    multipleCursors = true;
                }
            }
            _redoStack.Push(actions);
        }

        public void Redo(SourceCode sourceCode)
        {
            if (_redoStack.Count == 0)
            {
                return;
            }
            IReadOnlyCollection<UndoRedoAction> actions = _redoStack.Pop();
            bool multipleCursors = false;
            foreach (UndoRedoAction action in actions.OrderBy(x => (x is CursorMoveAction) ? 1 : 0))
            {
                action.Redo(sourceCode, multipleCursors);
                if (action is CursorMoveAction)
                {
                    multipleCursors = true;
                }
            }
            _undoStack.Push(actions);
        }
    }
}
