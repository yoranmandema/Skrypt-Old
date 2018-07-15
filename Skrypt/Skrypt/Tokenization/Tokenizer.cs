using System.Collections.Generic;
using System.Text.RegularExpressions;
using Skrypt.Engine;
using System;
using Skrypt.Parsing;
using System.Linq;

namespace Skrypt.Tokenization
{
    public enum TokenTypes : byte
    {
        None,
        NumericLiteral,
        BinaryLiteral,
        HexadecimalLiteral,
        Identifier,
        Keyword,
        BooleanLiteral,
        NullLiteral,
        EndOfExpression,
        NewLine,
        Punctuator,
        StringLiteral,
        HasValue = NumericLiteral | Identifier | BooleanLiteral | NullLiteral | StringLiteral,
        IsLiteral = NumericLiteral | BooleanLiteral | NullLiteral | StringLiteral,
    }

    /// <summary>
    ///     The main Tokenization class.
    ///     Contains all methods for tokenization.
    /// </summary>
    public class Tokenizer
    {
        private readonly SkryptEngine _engine;
        private readonly List<TokenRule> _tokenRules = new List<TokenRule>();

        public Tokenizer(SkryptEngine e)
        {
            _engine = e;
        }

        public void AddRule(Regex pattern, TokenTypes type)
        {
            _tokenRules.Add(new TokenRule
            {
                Pattern = pattern,
                Type = type
            });
        }

        /// <summary>
        ///     Tokenizes the given input string according to the token rules given to this object.
        /// </summary>
        /// <returns>
        ///     A list of tokens.
        /// </returns>
        public List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();

            var index = 0;
            var originalInput = input;
            var pScope = 0;
            var bScope = 0;
            var cScope = 0;

            Token previousToken = null;

            while (index < originalInput.Length)
            {
                Match foundMatch = null;
                TokenRule foundRule = null;

                // Check input string for all token rules
                foreach (var rule in _tokenRules)
                {
                    var match = rule.Pattern.Match(input);

                    // Only permit match if it's found at the start of the string
                    if (match.Index == 0 && match.Success)
                    {
                        foundMatch = match;
                        foundRule = rule;
                    }
                }

                // No match was found; this means we encountered an unexpected token.
                if (foundMatch == null)
                    _engine.ThrowError("Unexpected token '" + originalInput[index] + "' found",
                        new Token {Start = index});

                var token = new Token
                {
                    Value = foundMatch.Value,
                    Type = foundRule.Type,
                    Start = index + foundMatch.Index,
                    End = index + foundMatch.Index + foundMatch.Value.Length - 1
                };

                //switch (foundMatch.Value) {
                //    case "(":
                //        pScope++;
                //        break;
                //    case ")":
                //        pScope--;
                //        break;
                //    case "[":
                //        bScope++;
                //        break;
                //    case "]":
                //        bScope--;
                //        break;
                //    case "{":
                //        cScope++;
                //        break;
                //    case "}":
                //        cScope--;
                //        break;
                //}

                //if (previousToken != null && foundMatch.Value == "\n") {
                //    if (pScope == 0 && bScope == 0 && cScope == 0) {
                //        if (previousToken.IsLiteral() && token.Type == TokenTypes.Identifier) {
                //            //_engine.ThrowError("Identifier starts immediately after literal", new Token { Start = index });
                //            tokens.Add(new Token {
                //                Type = TokenTypes.EndOfExpression,
                //                Start = previousToken.End,
                //                End = index + foundMatch.Index + foundMatch.Value.Length - 1
                //            });
                //        }

                //        if (previousToken.IsValuable() && token.IsValuable()) {
                //            //_engine.ThrowError("Semicolon ; expected", new Token { Start = index });
                //            tokens.Add(new Token {
                //                Type = TokenTypes.EndOfExpression,
                //                Start = previousToken.End,
                //                End = index + foundMatch.Index + foundMatch.Value.Length - 1
                //            });
                //        }

                //        if (GeneralParser.NotPermittedInExpression.Contains(token.Value) && previousToken.IsValuable()) {
                //            tokens.Add(new Token {
                //                Type = TokenTypes.EndOfExpression,
                //                Start = previousToken.End,
                //                End = index + foundMatch.Index + foundMatch.Value.Length - 1
                //            });
                //        }

                //        if (token.IsValuable() && (new[] { "}", ")", "]" }).Contains(previousToken.Value)) {
                //            //addDelta = 1;
                //            //_engine.ThrowError("Semicolon ; expected'", token);
                //            tokens.Add(new Token {
                //                Type = TokenTypes.EndOfExpression,
                //                Start = previousToken.End,
                //                End = index + foundMatch.Index + foundMatch.Value.Length - 1
                //            });
                //        }
                //    }
                //}

                // Ignore token if it's type equals null
                if (foundRule.Type != TokenTypes.None) {
                    tokens.Add(token);
                    previousToken = token;
                }

                // Increase current index and cut away part of the string that got matched so we don't repeat it again.
                index += foundMatch.Value.Length;
                input = originalInput.Substring(index);
            }

            return tokens;
        }
    }
}