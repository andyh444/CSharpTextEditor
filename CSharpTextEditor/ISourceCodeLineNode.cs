namespace CSharpTextEditor
{
    internal interface ISourceCodeLineNode
    {
        SourceCodeLineList? List { get; }

        SourceCodeLine Value { get; }

        int LineNumber { get; }

        ISourceCodeLineNode? Next { get; }

        ISourceCodeLineNode? Previous { get; }
    }
}
