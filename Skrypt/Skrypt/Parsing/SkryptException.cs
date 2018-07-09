using System;

namespace Skrypt.Parsing
{
    internal class SkryptException : Exception
    {
        public int Urgency = -1;

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
            Urgency = u;
        }
    }
}