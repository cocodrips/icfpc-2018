using System;

namespace Yuizumi.Icfpc2018
{
    public class InvalidCommandException : InvalidOperationException
    {
        public InvalidCommandException(Command command, string message)
            : base(message)
        {
            Command = command;
        }

        public Command Command { get; }

        public override string Message
            => $"{base.Message}\nCommand: {Command}";
    }
}
