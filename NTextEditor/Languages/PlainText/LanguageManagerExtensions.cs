using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.PlainText
{
    public static class LanguageManagerExtensions
    {
        public static void SetLanguageToPlainText(this ILanguageManager languageManager)
        {
            languageManager.SetLanguage(
                syntaxHighlighter: new PlainTextSyntaxHighlighter(),
                codeExecutor: null,
                specialCharacterHandler: new PlainTextCharacterHandler());
        }
    }
}
