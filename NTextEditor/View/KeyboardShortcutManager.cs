using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.View
{
    public class KeyboardShortcutManager
    {
        private readonly List<ShortcutItem> _shortcuts = new List<ShortcutItem>();

        public KeyboardShortcutManager(IEnumerable<ShortcutItem> shortcuts)
        {
            _shortcuts = shortcuts.ToList();
        }

        public static KeyboardShortcutManager CreateDefault()
        {
            return new KeyboardShortcutManager(new[]
            {
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.A, true, viewManager => viewManager.SelectAll()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.X, true, viewManager => viewManager.CutSelectedToClipboard()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.C, true, viewManager => viewManager.CopySelectedToClipboard()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.V, true, viewManager => viewManager.PasteFromClipboard()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Z, true, viewManager => viewManager.Undo()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Y, true, viewManager => viewManager.Redo()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.D, true, viewManager => viewManager.DuplicateSelection()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.L, true, viewManager => viewManager.RemoveLineAtActivePosition()),
                new ShortcutItem(NTextModifierKey.Control | NTextModifierKey.Shift, NTextEditorKey.U, true, viewManager => viewManager.SelectionToUpperCase()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.U, true, viewManager => viewManager.SelectionToLowerCase()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Left, true, viewManager => viewManager.ShiftActivePositionOneWordToTheLeft(false)),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Right, true, viewManager => viewManager.ShiftActivePositionOneWordToTheRight(false)),
                new ShortcutItem(NTextModifierKey.Control | NTextModifierKey.Shift, NTextEditorKey.Left, true, viewManager => viewManager.ShiftActivePositionOneWordToTheLeft(true)),
                new ShortcutItem(NTextModifierKey.Control | NTextModifierKey.Shift, NTextEditorKey.Right, true, viewManager => viewManager.ShiftActivePositionOneWordToTheRight(true)),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Home, true, viewManager => viewManager.GoToFirstPosition()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.End, true, viewManager => viewManager.GoToLastPosition()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Back, true, viewManager => viewManager.RemoveWordBeforeActivePosition()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Delete, true, viewManager => viewManager.RemoveWordAfterActivePosition()),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Up, false, viewManager => viewManager.ScrollView(-1)),
                new ShortcutItem(NTextModifierKey.Control, NTextEditorKey.Down, false, viewManager => viewManager.ScrollView(1)),

                new ShortcutItem(NTextModifierKey.Alt, NTextEditorKey.Up, true, viewManager => viewManager.SwapLineUpAtActivePosition()),
                new ShortcutItem(NTextModifierKey.Alt, NTextEditorKey.Down, true, viewManager => viewManager.SwapLineDownAtActivePosition()),

                new ShortcutItem(NTextModifierKey.Shift, NTextEditorKey.Delete, true, viewManager => viewManager.RemoveLineAtActivePosition()),
            });
        }

        public bool ProcessShortcut(bool controlPressed, bool shiftPressed, bool altPressed, NTextEditorKey keyCode, ViewManager viewManager, out bool ensureInView)
        {
            ensureInView = false;
            NTextModifierKey modifierKeys = 0;
            if (controlPressed)
            {
                modifierKeys |= NTextModifierKey.Control;
            }
            if (shiftPressed)
            {
                modifierKeys |= NTextModifierKey.Shift;
            }
            if (altPressed)
            {
                modifierKeys |= NTextModifierKey.Alt;
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
