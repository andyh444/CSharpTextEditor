namespace NTextEditor.View.ToolTips
{
    public class ToolTipImageElement : IToolTipElement
    {
        public ICanvasImage Image { get; }

        public ToolTipImageElement(ICanvasImage image)
        {
            Image = image;
        }

        public void AddToDrawBuilder(IToolTipDrawBuilder drawBuilder)
        {
            drawBuilder.AddImage(Image);
        }
    }
}
