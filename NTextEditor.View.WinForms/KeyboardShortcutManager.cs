using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace NTextEditor.View.Winforms
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
                new ShortcutItem(ModifierKeys.Control, Keys.A, true, viewManager => viewManager.SelectAll()),
                new ShortcutItem(ModifierKeys.Control, Keys.X, true, viewManager => viewManager.CutSelectedToClipboard()),
                new ShortcutItem(ModifierKeys.Control, Keys.C, true, viewManager => viewManager.CopySelectedToClipboard()),
                new ShortcutItem(ModifierKeys.Control, Keys.V, true, viewManager => viewManager.PasteFromClipboard()),
                new ShortcutItem(ModifierKeys.Control, Keys.Z, true, viewManager => viewManager.Undo()),
                new ShortcutItem(ModifierKeys.Control, Keys.Y, true, viewManager => viewManager.Redo()),
                new ShortcutItem(ModifierKeys.Control, Keys.D, true, viewManager => viewManager.DuplicateSelection()),
                new ShortcutItem(ModifierKeys.Control, Keys.L, true, viewManager => viewManager.RemoveLineAtActivePosition()),
                new ShortcutItem(ModifierKeys.Control | ModifierKeys.Shift, Keys.U, true, viewManager => viewManager.SelectionToUpperCase()),
                new ShortcutItem(ModifierKeys.Control, Keys.U, true, viewManager => viewManager.SelectionToLowerCase()),
                new ShortcutItem(ModifierKeys.Control, Keys.Left, true, viewManager => viewManager.ShiftActivePositionOneWordToTheLeft(false)),
                new ShortcutItem(ModifierKeys.Control, Keys.Right, true, viewManager => viewManager.ShiftActivePositionOneWordToTheRight(false)),
                new ShortcutItem(ModifierKeys.Control | ModifierKeys.Shift, Keys.Left, true, viewManager => viewManager.ShiftActivePositionOneWordToTheLeft(true)),
                new ShortcutItem(ModifierKeys.Control | ModifierKeys.Shift, Keys.Right, true, viewManager => viewManager.ShiftActivePositionOneWordToTheRight(true)),
                new ShortcutItem(ModifierKeys.Control, Keys.Home, true, viewManager => viewManager.GoToFirstPosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.End, true, viewManager => viewManager.GoToLastPosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.Back, true, viewManager => viewManager.RemoveWordBeforeActivePosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.Delete, true, viewManager => viewManager.RemoveWordAfterActivePosition()),
                new ShortcutItem(ModifierKeys.Control, Keys.Up, false, viewManager => viewManager.ScrollView(-1)),
                new ShortcutItem(ModifierKeys.Control, Keys.Down, false, viewManager => viewManager.ScrollView(1)),

                new ShortcutItem(ModifierKeys.Alt, Keys.Up, true, viewManager => viewManager.SwapLineUpAtActivePosition()),
                new ShortcutItem(ModifierKeys.Alt, Keys.Down, true, viewManager => viewManager.SwapLineDownAtActivePosition()),

                new ShortcutItem(ModifierKeys.Shift, Keys.Delete, true, viewManager => viewManager.RemoveLineAtActivePosition()),
            });
        }

        public bool ProcessShortcut(bool controlPressed, bool shiftPressed, bool altPressed, Keys keyCode, ViewManager viewManager, out bool ensureInView)
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
                shortcut.Action(viewManager);
                return true;
            }
            return false;
        }
    }
}
