<a href="https://www.nuget.org/packages/CSharpTextEditor/">![](https://img.shields.io/nuget/v/CSharpTextEditor)</a> <a href="https://www.nuget.org/packages/CSharpTextEditor/">![](https://img.shields.io/nuget/dt/CSharpTextEditor)</a>

# CSharpTextEditor
A WinForms Control for displaying and editing C# code. Useful for creating and editing C# scripts from within your WinForms app.

It has the following features:
- Roslyn-based syntax highlighting 
- Syntax/Compilation error reporting
- Supports Light mode (VS2022 Blue), Dark mode (VS2022 Dark), and custom palettes
- Handles most VS keyboard edit shortcuts
- Allows multi-caret editing via box select
- Gives code completion suggestions
- Undo/redo history

## Installation
To install the NuGet package, run the following command in the Package Manager Console:
```ps
dotnet add package CSharpTextEditor
```
## Getting Started
To get started, just place the CodeEditorBox control into your WinForms control/form and start writing some C#! You can set the text that goes into it using the SetText method, and get the text from it using GetText.