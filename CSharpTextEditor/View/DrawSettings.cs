namespace CSharpTextEditor.View
{
    internal class DrawSettings(bool focused, bool cursorBlinkOn)
    {
        public bool Focused { get; set; } = focused;
        public bool CursorBlinkOn { get; set; } = cursorBlinkOn;
    }
}
