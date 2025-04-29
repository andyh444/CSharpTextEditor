using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.VisualBasic
{
    public static class LanguageManagerExtensions
    {
        public static void SetLanguageToVisualBasic(this ILanguageManager languageManager, bool isLibrary)
        {
            VisualBasicSyntaxHighlighter syntaxHighlighter = new VisualBasicSyntaxHighlighter(isLibrary);
            VisualBasicSpecialCharacterHandler specialCharacterHandler = new VisualBasicSpecialCharacterHandler();
            languageManager.SetLanguage(
                syntaxHighlighter: syntaxHighlighter,
                codeExecutor: isLibrary ? null : syntaxHighlighter,
                specialCharacterHandler: specialCharacterHandler);
        }
    }
}
