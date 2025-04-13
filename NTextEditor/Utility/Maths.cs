using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Utility
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

        public static int NumberOfDigits(int value)
        {
            if (value == 0)
            {
                return 1;
            }
            value = Math.Abs(value);
            int count = 0;
            while (value > 0)
            {
                value /= 10;
                count++;
            }
            return count;
        }
    }
}
