using System;
using Skrypt.Tokenization;

namespace Skrypt.Engine
{
    public class SkryptException : Exception {
        public Token Token {get;set;}

        public SkryptException()
        {
        }

        public SkryptException(string message)
            : base(message)
        {
        }

        public SkryptException(string message, Token token)
            : base(message) {
            Token = token;
        }
    }
}