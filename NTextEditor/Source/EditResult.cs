namespace NTextEditor.Source
{
    internal class EditResult(int positionChangeAfter)
    {
        /// <summary>
        /// The amount of characters that were added/removed after the cursor (e.g. for removing characters/words after the caret).
        /// Negative if removed, positive if added.
        /// </summary>
        public int PositionChangeAfter { get; } = positionChangeAfter;
    }
}
