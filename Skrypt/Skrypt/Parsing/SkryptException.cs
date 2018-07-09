using System;

namespace Skrypt.Parsing
{
    internal class SkryptException : Exception
    {
        public int urgency = -1;

        public SkryptException()
        {
        }

        public SkryptException(string message)
            : base(message)
        {
        }

        public SkryptException(string message, int u)
            : base(message)
        {
            urgency = u;
        }
    }
}