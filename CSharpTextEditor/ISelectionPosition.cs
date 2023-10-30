namespace CSharpTextEditor
{
    public interface ISelectionPosition : IComparable<ISelectionPosition>
    {
        int LineNumber { get; }
        int ColumnNumber { get; }
    }
}
