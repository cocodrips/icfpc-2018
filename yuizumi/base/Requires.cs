using System;

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
    }
}
