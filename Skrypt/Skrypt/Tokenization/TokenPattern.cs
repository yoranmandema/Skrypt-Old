using System.Collections.Generic;

namespace Skrypt.Tokenization
{
    /// <summary>
    ///     Represents a sequence of tokens as a pattern
    /// </summary>
    internal class TokenPattern
    {
        public List<PatternToken> Sequence = new List<PatternToken>();

        public bool HasPattern(List<Token> tokens)
        {
            var hasPattern = false;
            var i = 0;

            while (i < tokens.Count - 1)
            {
                var token = tokens[i];

                if (token.GetType() == typeof(PatternToken))
                {
                    var t = (PatternToken) token;

                    var b = t.GoOverTokens(tokens, ref i);

                    if (b) hasPattern = true;
                }

                i++;
            }

            return hasPattern;
        }
    }

    internal class PatternToken : Token
    {
        public delegate bool TokenMethod(List<Token> tokens, ref int index);

        public TokenMethod GoOverTokens;
    }
}