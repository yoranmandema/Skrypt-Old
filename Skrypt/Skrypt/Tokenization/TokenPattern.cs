using System.Collections.Generic;

namespace Skrypt.Tokenization
{
    /// <summary>
    ///     Represents a sequence of tokens as a pattern
    /// </summary>
    internal class TokenPattern
    {
        public List<PatternToken> Sequence = new List<PatternToken>();

        public bool HasPattern(List<Token> Tokens)
        {
            var hasPattern = false;
            var i = 0;

            while (i < Tokens.Count - 1)
            {
                var token = Tokens[i];

                if (token.GetType() == typeof(PatternToken))
                {
                    var t = (PatternToken) token;

                    var b = t.goOverTokens(Tokens, ref i);

                    if (b) hasPattern = true;
                }

                i++;
            }

            return hasPattern;
        }
    }

    internal class PatternToken : Token
    {
        public delegate bool tokenMethod(List<Token> Tokens, ref int Index);

        public tokenMethod goOverTokens;
    }
}