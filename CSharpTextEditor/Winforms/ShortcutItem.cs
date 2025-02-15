﻿using System;
using System.Windows.Forms;
using static CSharpTextEditor.Winforms.KeyboardShortcutManager;

namespace CSharpTextEditor.Winforms
{
    public class ShortcutItem
    {
        public ShortcutItem(ModifierKeys modifierKeys, Keys keyCode, bool ensureInView, Action<ICodeEditor> action)
        {
            ModifierKeys = modifierKeys;
            KeyCode = keyCode;
            Action = action;
            EnsureInView = ensureInView;
        }
        public ModifierKeys ModifierKeys { get; }
        public Keys KeyCode { get; }
        public bool EnsureInView { get; }
        public Action<ICodeEditor> Action { get; }
}
}
