namespace CSharpTextEditor.Source
{
    internal interface ISourceCodeListener
    {
        void TextChanged();
        void CursorsChanged();
    }
}
