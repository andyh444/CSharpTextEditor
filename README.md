# CSharpTextEditor
A WinForms control I've been putting together that can be used for writing and editing C# code. Useful for scripts and such.

At the moment, it has the following features:
- Do roslyn-based syntax highlighting 
- Highlight syntax and compilation errors
- Support Light mode (VS2022 Blue), Dark mode (VS2022 Dark) and custom themes via CodeEditorBox.SetPalette
- Handle most basic VS keyboard shortcuts
- Multi-caret editing via box select
- Give code completion suggestions when '.' is pressed + give tooltips when invoking methods and hovering over symbols
- Unlimited undo and redo history

My aim is to refine and expand upon all of the above, and maybe expand on this control to work with other languages, like Visual Basic.

## Getting started
To get started, just place the CodeEditorBox control into your WinForms control/form and start writing some C#! You can set the text that goes into it using the SetText method, and get the text from it using GetText.