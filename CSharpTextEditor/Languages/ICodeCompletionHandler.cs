using CSharpTextEditor.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Languages
{
    public interface ICodeCompletionHandler
    {
        void ShowCodeCompletionForm();

        void HideCodeCompletionForm(bool hideMethodToolTip);

        void ShowMethodCompletion(SourceCodePosition position, IReadOnlyCollection<CodeCompletionSuggestion> suggestions, int activeParameterIndex);
    }
}
