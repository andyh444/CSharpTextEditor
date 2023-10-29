namespace CSharpTextEditor
{
    public interface ISelectionPosition
    {
        int LineNumber { get; }
        int ColumnNumber { get; }
    }
}
