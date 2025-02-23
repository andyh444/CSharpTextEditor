using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CSharpTextEditor.Winforms.KeyboardShortcutManager;

namespace CSharpTextEditor.Winforms
{
    public class KeyboardShortcutManager
    {
        public enum ModifierKeys
        {
            Control = 1,
            Shift = 2,
            Alt = 4
        }

        private readonly List<ShortcutItem> _shortcuts = new List<ShortcutItem>();

        public KeyboardShortcutManager(IEnumerable<ShortcutItem> shortcuts)
        {
            _shortcuts = shortcuts.ToList();
        }

        public static KeyboardShortcutManager CreateDefault()
        {
            return new KeyboardShortcutManager(new[]
            {
                new ShortcutItem(ModifierKeys.Control, Keys.A, true, codeEditor => codeEditor.SelectAll()),
                new ShortcutItem(ModifierKeys.Control, Keys.X, true, codeEditor => codeEditor.CutSelectedToClipboard()),
                new ShortcutItem(ModifierKeys.Control, Keys.C, true, codeEditor => codeEditor.CopySelectedToClipboard()),
                new ShortcutItem(ModifierKeys.Control, Keys.V, true, codeEditor => codeEditor.PasteFromClipboard()),
                new ShortcutItem(ModifierKeys.Control, Keys.Z, true, codeEditor => codeEditor.Undo()),
                new ShortcutItem(ModifierKeys.Control, Keys.Y, true, codeEditor => codeEditor.Redo()),
                new ShortcutItem(ModifierKeys.Control, Keys.D, true, codeEditor => codeEditor.DuplicateSelection()),
                new ShortcutItem(ModifierKeys.Control | ModifierKeys.Shift, Keys.U, true, codeEditor => codeEditor.SelectionToUpperCase()),
                new ShortcutItem(ModifierKeys.Control, Keys.U, true, codeEditor => codeEditor.SelectionToLowerCase()),
                new ShortcutItem(ModifierKeys.Control, Keys.Left, true, codeEditor => codeEditor.ShiftActivePositionOneWordToTheLeft(false)),
                new ShortcutItem(ModifierKeys.Control, Keys.Right, true, codeEditor => codeEditor.ShiftActivePositionOneWordToTheRight(false)),
                new ShortcutItem(ModifierKeys.Control | ModifierKeys.Shift, Keys.Left, true, codeEditor => codeEditor.ShiftActivePositionOneWordToTheLeft(true)),
                new ShortcutItem(ModifierKeys.Control | ModifierKeys.Shift, Keys.Right, true, codeEditor => codeEditor.ShiftActivePositionOneWordToTheRight(true)),
                new ShortcutItem(ModifierKeys.Control, Keys.Home, true, codeEditor => codeEditor.GoToFirstPosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.End, true, codeEditor => codeEditor.GoToLastPosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.Back, true, codeEditor => codeEditor.RemoveWordBeforeActivePosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.Delete, true, codeEditor => codeEditor.RemoveWordAfterActivePosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.Up, false, codeEditor => codeEditor.ScrollView(-1)),
                new ShortcutItem(ModifierKeys.Control, Keys.Down, false, codeEditor => codeEditor.ScrollView(1)),
            });
        }

        public bool ProcessShortcut(bool controlPressed, bool shiftPressed, bool altPressed, Keys keyCode, ICodeEditor codeEditor, out bool ensureInView)
        {
            ensureInView = false;
            ModifierKeys modifierKeys = 0;
            if (controlPressed)
            {
                modifierKeys |= ModifierKeys.Control;
            }
            if (shiftPressed)
            {
                modifierKeys |= ModifierKeys.Shift;
            }
            if (altPressed)
            {
                modifierKeys |= ModifierKeys.Alt;
            }
            var shortcut = _shortcuts.FirstOrDefault(s => s.ModifierKeys == modifierKeys && s.KeyCode == keyCode);
            if (shortcut != null)
            {
                ensureInView = shortcut.EnsureInView;
                shortcut.Action(codeEditor);
                return true;
            }
            return false;
        }
    }
}
