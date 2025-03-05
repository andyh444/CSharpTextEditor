using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Utility
{
    public static class Maths
    {
        public static bool IsBetweenExclusive<T>(T lower, T value, T upper) where T : IComparable<T>
        {
            return lower.CompareTo(value) < 0 && value.CompareTo(upper) < 0;
        }

        public static bool IsBetweenInclusive<T>(T lower, T value, T upper) where T : IComparable<T>
        {
            return lower.CompareTo(value) <= 0 && value.CompareTo(upper) <= 0;
        }

        public static T Clamp<T>(T lower, T value, T upper) where T : IComparable<T>
        {
            if (value.CompareTo(lower) < 0)
            {
                return lower;
            }
            else if (value.CompareTo(upper) > 0)
            {
                return upper;
            }
            return value;
        }
    }
}
