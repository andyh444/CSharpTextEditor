using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTextEditor.Languages.PlainText
{
    internal class PlainTextSyntaxHighlighter : ISyntaxHighlighter
    {
        private List<string>? _sourceLines;

        public SyntaxHighlightingCollection GetHighlightings(SyntaxPalette palette)
            => new SyntaxHighlightingCollection([], [], []);

        public IReadOnlyList<CodeCompletionSuggestion> GetSuggestionsAtPosition(int characterPosition, SyntaxPalette palette, out int argumentIndex)
        {
            argumentIndex = -1;
            return [];
        }

        public CodeCompletionSuggestion? GetSymbolInfoAtPosition(int characterPosition, SyntaxPalette palette)
            => null;

        public IEnumerable<(int start, int end)> GetSymbolSpansAfterPosition(int characterPosition)
        {
            if (_sourceLines == null)
            {
                yield break;
            }

            int currentIndex = 0;
            foreach (var line in _sourceLines)
            {
                if (characterPosition < currentIndex + line.Length)
                {
                    foreach ((int start, int end) in GetSpans(line))
                    {
                        if (currentIndex + end >= characterPosition)
                        {
                            yield return (currentIndex + start, currentIndex + end);
                        }
                    }
                }
                currentIndex += line.Length + Environment.NewLine.Length;
            }
        }

        private IEnumerable<(int start, int end)> GetSpans(string textLine)
        {
            int wordStart = 0;
            for (int i = 0; i < textLine.Length; i++)
            {
                char c = textLine[i];
                if (char.IsWhiteSpace(c))
                {
                    yield return (wordStart, i);
                    wordStart = i;
                }
            }
            yield return (wordStart, textLine.Length);
        }

        public IEnumerable<(int start, int end)> GetSymbolSpansBeforePosition(int characterPosition)
        {
            if (_sourceLines == null)
            {
                return [];
            }

            int currentIndex = 0;
            var spans = new List<(int start, int end)>();

            foreach (var line in _sourceLines)
            {
                if (characterPosition > currentIndex)
                {
                    foreach ((int start, int end) in GetSpans(line))
                    {
                        if (currentIndex + start < characterPosition)
                        {
                            spans.Add((currentIndex + start, currentIndex + end));
                        }
                    }
                }
                currentIndex += line.Length + Environment.NewLine.Length;
            }

            return Enumerable.Reverse(spans);
        }

        public void Update(IEnumerable<string> sourceLines)
        {
            _sourceLines = sourceLines.ToList();
        }
    }
}
