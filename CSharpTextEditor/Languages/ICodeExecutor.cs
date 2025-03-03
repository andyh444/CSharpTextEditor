using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Languages
{
    public interface ICodeExecutor
    {
        void Execute(TextWriter output);
    }
}
