using CSharpTextEditor.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.UndoRedoActions
{

    internal class HistoryManager
    {
        private readonly Stack<HistoryItem> _undoStack;
        private readonly Stack<HistoryItem> _redoStack;

        public event Action? HistoryChanged;

        public IEnumerable<string> UndoNames => _undoStack.Select(x => x.DisplayName);

        public IEnumerable<string> RedoNames => _redoStack.Select(x => x.DisplayName);

        public HistoryManager()
        {
            _undoStack = new Stack<HistoryItem>();
            _redoStack = new Stack<HistoryItem>();
        }

        public void AddAction(HistoryItem action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();
            HistoryChanged?.Invoke();
        }

        public void Undo(SourceCode sourceCode)
        {
            if (_undoStack.Count == 0)
            {
                return;
            }
            HistoryItem item = _undoStack.Pop();
            foreach (SelectionRangeActionList actionList in item.Actions.Reverse())
            {
                foreach (UndoRedoAction action in Enumerable.Reverse(actionList.UndoRedoActions))
                {
                    action.Undo(sourceCode);
                }
            }
            item.MoveToBeforePositions(sourceCode);
            _redoStack.Push(item);
            HistoryChanged?.Invoke();
        }

        public void Redo(SourceCode sourceCode)
        {
            if (_redoStack.Count == 0)
            {
                return;
            }
            HistoryItem item = _redoStack.Pop();
            foreach (SelectionRangeActionList actionList in item.Actions)
            {
                foreach (UndoRedoAction action in actionList.UndoRedoActions)
                {
                    action.Redo(sourceCode);
                }
            }
            item.MoveToAfterPositions(sourceCode);
            _undoStack.Push(item);
            HistoryChanged?.Invoke();
        }
    }
}
