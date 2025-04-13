using CSharpTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.CSharp
{
    public static class LanguageManagerExtensions
    {
        public static void SetLanguageToCSharp(this ILanguageManager languageManager, bool isLibrary)
        {
            CSharpSyntaxHighlighter syntaxHighlighter = new CSharpSyntaxHighlighter(isLibrary);
            CSharpSpecialCharacterHandler specialCharacterHandler = new CSharpSpecialCharacterHandler(syntaxHighlighter);
            languageManager.SetLanguage(
                syntaxHighlighter: syntaxHighlighter,
                codeExecutor: isLibrary ? null : syntaxHighlighter,
                specialCharacterHandler: specialCharacterHandler);
        }
    }
}
