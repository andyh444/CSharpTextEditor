namespace NTextEditor.View.ToolTips
{
    public class ToolTipTextElement : IToolTipElement
    {
        public ToolTipTextElement(string fullText, ColourTextSpan span)
        {
            FullText = fullText;
            Span = span;
        }

        public string FullText { get; }
        public ColourTextSpan Span { get; }
        // TODO Bold

        public void AddToDrawBuilder(IToolTipDrawBuilder drawBuilder)
        {
            drawBuilder.AddText(FullText, Span, false);
        }
    }
}
