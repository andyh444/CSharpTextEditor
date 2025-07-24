using NTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NTextEditor.View.ToolTips
{
    public class MethodCompletionContents : IToolTipContents
    {
        public IReadOnlyList<CodeCompletionSuggestion> Suggestions { get; }
        public int ActiveSuggestion { get; private set; }
        public int ActiveParameterIndex { get; }

        public MethodCompletionContents(IReadOnlyList<CodeCompletionSuggestion> suggestions, int activeSuggestion, int activeParameterIndex)
        {
            Suggestions = suggestions;
            ActiveSuggestion = activeSuggestion;
            ActiveParameterIndex = activeParameterIndex;
        }

        public MethodCompletionContents WithNewSuggestions(IReadOnlyList<CodeCompletionSuggestion> newSuggestions, int newActiveParameterIndex)
        {
            // TODO Find the best activesuggestion from newSuggestions
            return new MethodCompletionContents(newSuggestions,
                newSuggestions.Count > ActiveSuggestion ? ActiveSuggestion : 0,
                newActiveParameterIndex);
        }

        public bool Cycle(int sign)
        {
            if (Suggestions.Count <= 1)
            {
                return false;
            }
            if (sign > 0)
            {
                ActiveSuggestion = (ActiveSuggestion + 1) % Suggestions.Count;
            }
            else if (sign < 0)
            {
                ActiveSuggestion = (ActiveSuggestion - 1 + Suggestions.Count) % Suggestions.Count;
            }
            return true;
        }

        public Size Draw(ICanvas canvas, IIconCache iconCache, SyntaxPalette palette)
        {
            CodeCompletionSuggestion suggestion = Suggestions[ActiveSuggestion];

            ICanvasImage? icon = iconCache.GetIcon(suggestion.SymbolType);
            int x = 0;
            int height = 0;
            if (Suggestions.Count > 1)
            {
                string indexText = $"🡹{ActiveSuggestion + 1} of {Suggestions.Count}🡻";
                Size indexSize = canvas.DrawText(indexText, [new ColourTextSpan(0, indexText.Length, palette.DefaultTextColour, false)], new Point(x, 0), false);
                x += indexSize.Width;
                height = Math.Max(height, indexSize.Height);
            }
            if (icon != null)
            {
                canvas.DrawImage(icon, new Point(x, 0));
                x += icon.Width;
                height = Math.Max(height, icon.Height);
            }
            (string toolTip, List<SyntaxHighlighting> highlightings) = suggestion.ToolTipSource.GetToolTip();
            Size textSize = DrawingHelper.DrawTextLine(canvas, 0, toolTip, x, 0, highlightings, palette, ActiveParameterIndex);
            height = Math.Max(height, textSize.Height);
            x += textSize.Width;

            return new Size(x + 3, height);
        }

        public bool Equals(IToolTipContents? other)
        {
            return other is MethodCompletionContents otherMethodCompletion
                   && ActiveSuggestion == otherMethodCompletion.ActiveSuggestion
                   && ActiveParameterIndex == otherMethodCompletion.ActiveParameterIndex
                   && Suggestions.SequenceEqual(otherMethodCompletion.Suggestions);
        }

        public IEnumerable<IToolTipElement> GetElements(IIconCache iconCache, SyntaxPalette palette)
        {
            CodeCompletionSuggestion suggestion = Suggestions[ActiveSuggestion];

            ICanvasImage? icon = iconCache.GetIcon(suggestion.SymbolType);
            if (Suggestions.Count > 1)
            {
                string indexText = $"🡹{ActiveSuggestion + 1} of {Suggestions.Count}🡻";
                yield return new ToolTipTextElement(indexText, new ColourTextSpan(0, indexText.Length, palette.DefaultTextColour, false));
            }
            if (icon != null)
            {
                yield return new ToolTipImageElement(icon);
            }
            (string toolTip, List<SyntaxHighlighting> highlightings) = suggestion.ToolTipSource.GetToolTip();
            foreach (SyntaxHighlighting h in highlightings)
            {
                yield return new ToolTipTextElement(toolTip, new ColourTextSpan(h.Start.ColumnNumber, h.End.ColumnNumber - h.Start.ColumnNumber, h.Colour, false));
            }
        }
    }
}
