using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor
{
    public class CSharpTextEditorException : Exception
    {
        public CSharpTextEditorException()
        {
        }

        public CSharpTextEditorException(string message)
            : base(message)
        {
        }

        public CSharpTextEditorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
