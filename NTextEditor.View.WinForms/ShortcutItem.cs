using System;
using System.Windows.Forms;
using static NTextEditor.View.Winforms.KeyboardShortcutManager;

namespace NTextEditor.View.Winforms
{
    public class ShortcutItem
    {
        public ShortcutItem(ModifierKeys modifierKeys, Keys keyCode, bool ensureInView, Action<ViewManager> action)
        {
            ModifierKeys = modifierKeys;
            KeyCode = keyCode;
            Action = action;
            EnsureInView = ensureInView;
        }
        public ModifierKeys ModifierKeys { get; }
        public Keys KeyCode { get; }
        public bool EnsureInView { get; }
        public Action<ViewManager> Action { get; }
    }
}
