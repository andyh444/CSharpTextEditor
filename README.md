<a href="https://www.nuget.org/packages/CSharpTextEditor/">![](https://img.shields.io/nuget/v/CSharpTextEditor)</a> <a href="https://www.nuget.org/packages/CSharpTextEditor/">![](https://img.shields.io/nuget/dt/CSharpTextEditor)</a>

# NTextEditor
A set of nuget packages providing controls for editing .net language scripts

At present, the following UI libraries are supported:
- Winforms

At present, the following languages are supported:
- C#

It has the following features:
- Roslyn-based syntax highlighting 
- Syntax/Compilation error reporting
- Supports Light mode (VS2022 Blue), Dark mode (VS2022 Dark), and custom palettes
- Handles most VS keyboard edit shortcuts
- Allows multi-caret editing via box select
- Gives code completion suggestions
- Undo/redo history

## Installation
To install the NuGet packages, run the following commands in the Package Manager Console:
```ps
dotnet add package NTextEditor.Views.Winforms
```

```ps
dotnet add package NTextEditor.Languages.CSharp
```
## Getting Started
To get started, just place the CodeEditorBox control into your WinForms control/form, and call the SetLanguageToCSharp extension method on it