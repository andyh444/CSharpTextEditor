using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages
{
    public interface ILanguageManager
    {
        void SetLanguage(ISyntaxHighlighter syntaxHighlighter, ICodeExecutor? codeExecutor, ISpecialCharacterHandler specialCharacterHandler);
    }
}
