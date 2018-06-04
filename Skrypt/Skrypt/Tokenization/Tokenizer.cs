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

            bool FoundMatch = true;
            int Index = 0;
            string OriginalInput = Input;

            while (FoundMatch) {
                Match FirstMatch = null;
                TokenRule FoundRule = null;

                foreach (TokenRule Rule in TokenRules) {
                    Match match = Rule.Pattern.Match(Input);

                    if (FirstMatch == null) {
                        FirstMatch = match;
                        FoundRule = Rule;
                    } else {
                        if (match.Index < FirstMatch.Index && match.Success) {
                            FirstMatch = match;
                            FoundRule = Rule;
                        }
                    }
                }

                FoundMatch = FirstMatch.Success;

                if (FirstMatch.Index != 0) {
                    Console.WriteLine("Unexpected token \"" + OriginalInput[Index] + "\" found at index " + Index);
                    return null;
                }

                if (FoundMatch) {
                    Token token = new Token {
                        Value = FirstMatch.Value,
                        Type = FoundRule.Type,
                        Start = Index + FirstMatch.Index,
                        End = Index + FirstMatch.Index + FirstMatch.Value.Length - 1,
                    };

                    Tokens.Add(token);
                }

                Index += FirstMatch.Value.Length;
                Index += OriginalInput.Substring(Index).TakeWhile(c => c == ' ').Count();           
                Input = OriginalInput.Substring(Index);
            }

            return Tokens;
        }
    }
}
