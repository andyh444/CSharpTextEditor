using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Tests
{
    public class TestCase<T>(T value, string description)
    {
        public T Value { get; } = value;

        public override string ToString() => description;
    }
}
