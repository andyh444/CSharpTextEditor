namespace NTextEditor.View.Winforms
{
    public class WinformsCanvasImage : ICanvasImage
    {
        public int Width => Image.Width;

        public int Height => Image.Height;

        public Bitmap Image { get; }

        public WinformsCanvasImage(Bitmap image)
        {
            Image = image;
        }

        public void DrawToCanvas(ICanvas canvas, Point point)
        {
            if (canvas is WinformsCanvas winformsCanvas)
            {
                winformsCanvas.Graphics.DrawImage(Image, point);
            }
            else
            {
                throw new ArgumentException("Invalid canvas type", nameof(canvas));
            }
        }
    }
}
