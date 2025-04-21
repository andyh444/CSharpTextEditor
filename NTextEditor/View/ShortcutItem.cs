using System;
namespace NTextEditor.View
{
    public class ShortcutItem
    {
        public ShortcutItem(NTextModifierKey modifierKeys, NTextEditorKey keyCode, bool ensureInView, Action<ViewManager> action)
        {
            ModifierKeys = modifierKeys;
            KeyCode = keyCode;
            Action = action;
            EnsureInView = ensureInView;
        }
        public NTextModifierKey ModifierKeys { get; }
        public NTextEditorKey KeyCode { get; }
        public bool EnsureInView { get; }
        public Action<ViewManager> Action { get; }
    }
}
