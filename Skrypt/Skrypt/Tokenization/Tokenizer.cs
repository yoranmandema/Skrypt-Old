using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Skrypt.Engine;

namespace Skrypt.Tokenization {

    /// <summary>
    /// The main Tokenization class.
    /// Contains all methods for tokenization.
    /// </summary>
    public class Tokenizer {
        List<TokenRule> TokenRules = new List<TokenRule>();
        SkryptEngine engine;

        public Tokenizer(SkryptEngine e) {
            engine = e;
        }

        public void AddRule (Regex Pattern, string Type) {
            TokenRules.Add(new TokenRule {
                Pattern = Pattern,
                Type = Type
            });
        }

        /// <summary>
        /// Tokenizes the given input string according to the token rules given to this object.
        /// </summary>
        /// <returns>
        /// A list of tokens.
        /// </returns>
        public List<Token> Tokenize (string Input) {
            List<Token> Tokens = new List<Token>();

            int Index = 0;
            string OriginalInput = Input;

            while (Index < OriginalInput.Length) {
                Match FoundMatch = null;
                TokenRule FoundRule = null;

                // Check input string for all token rules
                foreach (TokenRule Rule in TokenRules) {
                    Match match = Rule.Pattern.Match(Input);

                    // Only permit match if it's found at the start of the string
                    if (match.Index == 0 && match.Success) {
                        FoundMatch = match;
                        FoundRule = Rule;
                    }
                }

                // No match was found; this means we encountered an unexpected token.
                if (FoundMatch == null) {
                    engine.throwError("Unexpected token '" + OriginalInput[Index] + "' found", new Token {Start = Index});
                }

                Token token = new Token {
                    Value = FoundMatch.Value,
                    Type = FoundRule.Type,
                    Start = Index + FoundMatch.Index,
                    End = Index + FoundMatch.Index + FoundMatch.Value.Length - 1,
                };

                // Ignore token if it's type equals null
                if (FoundRule.Type != null) {
                    Tokens.Add(token);
                }

                // Increase current index and cut away part of the string that got matched so we don't repeat it again.
                Index += FoundMatch.Value.Length;       
                Input = OriginalInput.Substring(Index);
            }

            return Tokens;
        }
    }
}
