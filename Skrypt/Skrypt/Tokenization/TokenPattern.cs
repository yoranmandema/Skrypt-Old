using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Tokenization {
    /// <summary>
    /// Represents a sequence of tokens as a pattern
    /// </summary>
    class TokenPattern {
        public List<PatternToken> Sequence = new List<PatternToken>();

        public bool HasPattern (List<Token> Tokens) {
            bool hasPattern = false;
            int i = 0;

            while (i < Tokens.Count - 1) {
                Token token = Tokens[i];

                if (token.GetType() == typeof(PatternToken)) {
                    PatternToken t = (PatternToken)token;

                    bool b = t.goOverTokens(Tokens, ref i);

                    if (b) {
                        hasPattern = true;
                    }
                } else {

                }

                i++;
            }

            return hasPattern;
        }
    }

    class PatternToken : Token {
        public delegate bool tokenMethod(List<Token> Tokens, ref int Index);

        public tokenMethod goOverTokens; 
    }
}
