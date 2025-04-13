using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Source
{
    /// <summary>
    /// Represents a linkedlist of <see cref="SourceCodeLine"/> objects, and additionally keeps track of the line number of each line.
    /// </summary>
    internal class SourceCodeLineList : IReadOnlyCollection<ISourceCodeLineNode>
    {
        private class SourceCodeLineNode : ISourceCodeLineNode
        {
            public LinkedListNode<SourceCodeLineNode>? Node { get; set; }

            public SourceCodeLine Value { get; }

            public int LineNumber { get; set; }

            public ISourceCodeLineNode? Next => Node?.Next?.Value;

            public ISourceCodeLineNode? Previous => Node?.Previous?.Value;

            public SourceCodeLineList? List { get; set; }

            public SourceCodeLineNode(SourceCodeLine value)
            {
                Value = value;
            }
        }

        private readonly LinkedList<SourceCodeLineNode> _list;

        public int Count => _list.Count;

        public ISourceCodeLineNode? First => _list.First?.Value;

        public ISourceCodeLineNode? Last => _list.Last?.Value;

        public SourceCodeLineList()
        {
            _list = new LinkedList<SourceCodeLineNode>();
        }

        public SourceCodeLineList(IEnumerable<SourceCodeLine> sourceCodeLines)
            : this()
        {
            foreach (var sourceCodeLine in sourceCodeLines)
            {
                AddLast(sourceCodeLine);
            }
        }

        public ISourceCodeLineNode AddFirst(SourceCodeLine line)
        {
            foreach (SourceCodeLineNode sourceCodeLineNode in _list)
            {
                sourceCodeLineNode.LineNumber++;
            }
            SourceCodeLineNode node = new SourceCodeLineNode(line);
            node.Node = _list.AddFirst(node);
            node.LineNumber = 0;
            node.List = this;
            return node;
        }

        public ISourceCodeLineNode AddLast(SourceCodeLine line)
        {
            SourceCodeLineNode node = new SourceCodeLineNode(line);
            node.Node = _list.AddLast(node);
            node.List = this;
            node.LineNumber = Count - 1;
            return node;
        }

        public ISourceCodeLineNode AddAfter(ISourceCodeLineNode node, SourceCodeLine line)
        {
            if (node is SourceCodeLineNode sourceCodeLineNode)
            {
                if (sourceCodeLineNode.List != this
                    || sourceCodeLineNode.Node == null)
                {
                    throw new InvalidOperationException();
                }
                SourceCodeLineNode newNode = new SourceCodeLineNode(line);
                newNode.Node = _list.AddAfter(sourceCodeLineNode.Node, newNode);
                newNode.List = this;
                newNode.LineNumber = sourceCodeLineNode.LineNumber + 1;
                foreach (var item in GetNodesAfterThis(newNode))
                {
                    item.LineNumber++;
                }
                return newNode;
            }
            else
            {
                throw new CSharpTextEditorException();
            }
        }

        public void Remove(ISourceCodeLineNode node)
        {
            if (node is SourceCodeLineNode sourceCodeLineNode)
            {
                if (sourceCodeLineNode.List != this
                    || sourceCodeLineNode.Node == null)
                {
                    throw new InvalidOperationException();
                }
                foreach (var item in GetNodesAfterThis(sourceCodeLineNode))
                {
                    item.LineNumber--;
                }
                _list.Remove(sourceCodeLineNode.Node);
                sourceCodeLineNode.List = null;
            }
            else
            {
                throw new CSharpTextEditorException();
            }
        }

        public void SwapWithPrevious(ISourceCodeLineNode node)
        {
            if (node is SourceCodeLineNode sourceCodeLineNode)
            {
                if (sourceCodeLineNode.List != this
                    || sourceCodeLineNode.Node == null)
                {
                    throw new InvalidOperationException();
                }
                if (sourceCodeLineNode.Node.Previous == null)
                {
                    throw new InvalidOperationException();
                }
                SourceCodeLineNode previous = sourceCodeLineNode.Node.Previous.Value;
                _list.Remove(sourceCodeLineNode.Node);
                sourceCodeLineNode.Node = _list.AddBefore(previous.Node, sourceCodeLineNode);
                sourceCodeLineNode.LineNumber--;
                previous.LineNumber++;
            }
            else
            {
                throw new CSharpTextEditorException();
            }
        }

        public void SwapWithNext(ISourceCodeLineNode node)
        {
            if (node is SourceCodeLineNode sourceCodeLineNode)
            {
                if (sourceCodeLineNode.List != this
                    || sourceCodeLineNode.Node == null)
                {
                    throw new InvalidOperationException();
                }
                if (sourceCodeLineNode.Node.Next == null)
                {
                    throw new InvalidOperationException();
                }
                SourceCodeLineNode next = sourceCodeLineNode.Node.Next.Value;
                _list.Remove(sourceCodeLineNode.Node);
                sourceCodeLineNode.Node = _list.AddAfter(next.Node, sourceCodeLineNode);
                sourceCodeLineNode.LineNumber++;
                next.LineNumber--;
            }
            else
            {
                throw new CSharpTextEditorException();
            }
        }

        private IEnumerable<SourceCodeLineNode> GetNodesAfterThis(SourceCodeLineNode node)
        {
            ISourceCodeLineNode? current = node.Next;
            while (current != null)
            {
                yield return (current as SourceCodeLineNode)!;
                current = current.Next;
            }
        }

        public void Clear()
        {
            foreach (var item in _list)
            {
                item.List = null;
            }
            _list.Clear();
        }

        public IEnumerator<ISourceCodeLineNode> GetEnumerator()
        {
            foreach (var item in _list)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
