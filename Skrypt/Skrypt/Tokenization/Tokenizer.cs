using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Skrypt.Tokenization {
    class Tokenizer {
        public List<TokenRule> TokenRules = new List<TokenRule>();

        public List<Token> Tokenize (string Input) {
            List<Token> Tokens = new List<Token>();

            int Index = 0;
            string OriginalInput = Input;

            while (Index < OriginalInput.Length) {
                Match FoundMatch = null;
                TokenRule FoundRule = null;

                foreach (TokenRule Rule in TokenRules) {
                    Match match = Rule.Pattern.Match(Input);

                    if (match.Index == 0 && match.Success) {
                        FoundMatch = match;
                        FoundRule = Rule;
                    }
                }

                if (FoundMatch == null) {
                    Console.WriteLine(Index);
                    Console.WriteLine("Unexpected token \"" + OriginalInput[Index] + "\" found at index " + Index);
                    return null;
                }

                Token token = new Token {
                    Value = FoundMatch.Value,
                    Type = FoundRule.Type,
                    Start = Index + FoundMatch.Index,
                    End = Index + FoundMatch.Index + FoundMatch.Value.Length - 1,
                };

                Tokens.Add(token);

                Index += FoundMatch.Value.Length;
                Index += OriginalInput.Substring(Index).TakeWhile(Char.IsWhiteSpace).Count();           
                Input = OriginalInput.Substring(Index);
            }

            return Tokens;
        }
    }
}
