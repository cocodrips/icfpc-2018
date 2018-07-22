using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    internal static class Requires
    {
        internal static void Arg(bool condition, string name, string message)
        {
            if (!condition) throw new ArgumentException(message, name);
        }

        internal static void Range(int arg, string name, int min, int max)
        {
            if (arg < min || arg > max) throw new ArgumentOutOfRangeException(name);
        }

        internal static void NotNull<T>(T arg, string name)
            where T : class
        {
            if (arg == null) throw new ArgumentNullException(name);
        }

        internal static void NoneNull<T>(IEnumerable<T> arg, string name)
            where T : class
        {
            if (arg.Contains(null))
                throw new ArgumentException("Sequence may not contain null.", name);
        }

        internal static void State(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
