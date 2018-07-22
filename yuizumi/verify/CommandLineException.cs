using System;

namespace Yuizumi.Icfpc2018
{
    internal class CommandLineException : Exception
    {
        internal CommandLineException(string message)
            : base(message) {}
    }
}
