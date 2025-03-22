using CSharpTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CSharpTextEditor.View.Winforms
{
    public class MethodCompletionContents : IToolTipContents
    {
        private readonly IReadOnlyList<CodeCompletionSuggestion> _suggestions;
        private int _activeSuggestion;
        private readonly int _activeParameterIndex;

        public MethodCompletionContents(IReadOnlyList<CodeCompletionSuggestion> suggestions, int activeSuggestion, int activeParameterIndex)
        {
            _suggestions = suggestions;
            _activeSuggestion = activeSuggestion;
            _activeParameterIndex = activeParameterIndex;
        }

        public bool Cycle(int sign)
        {
            if (sign > 0)
            {
                _activeSuggestion = (_activeSuggestion + 1) % _suggestions.Count;
            }
            else if (sign < 0)
            {
                _activeSuggestion = (_activeSuggestion - 1 + _suggestions.Count) % _suggestions.Count;
            }
            return true;
        }

        public Size Draw(Graphics g, Font font, SyntaxPalette palette)
        {
            CodeCompletionSuggestion suggestion = _suggestions[_activeSuggestion];

            ICanvas canvas = new WinformsCanvas(g, new Size(), font);
            Bitmap? icon = IconCache.GetIcon(suggestion.SymbolType);
            int x = 0;
            int height = 0;
            if (_suggestions.Count > 1)
            {
                string indexText = $"🡹{_activeSuggestion + 1} of {_suggestions.Count}🡻";
                Size indexSize = canvas.GetTextSize(indexText);
                canvas.DrawText(indexText, palette.DefaultTextColour, new Point(x, 0), false);
                x += indexSize.Width;
                height = Math.Max(height, indexSize.Height);
            }
            if (icon != null)
            {
                g.DrawImage(icon, x, 0);
                x += icon.Width;
                height = Math.Max(height, icon.Height);
            }
            (string toolTip, List<SyntaxHighlighting> highlightings) = suggestion.ToolTipSource.GetToolTip();
            Size textSize = DrawingHelper.DrawTextLine(canvas, 0, toolTip, x, 0, highlightings, palette, _activeParameterIndex);
            height = Math.Max(height, textSize.Height);
            x += textSize.Width;

            return new Size(x + 3, height);
        }
    }
}
