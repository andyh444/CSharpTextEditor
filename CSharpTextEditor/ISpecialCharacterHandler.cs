using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal interface ISpecialCharacterHandler
    {
        void HandleLineBreakInserted(SourceCode sourceCode, Cursor activePosition);

        void HandleCharacterInserting(char character, SourceCode sourceCode);
    }
}
