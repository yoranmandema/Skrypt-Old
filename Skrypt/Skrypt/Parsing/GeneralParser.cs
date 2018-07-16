using System;
using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;

namespace Skrypt.Parsing
{
    /// <summary>
    ///     The general parser class.
    ///     Contains all methods to parse higher-level code, e.g code that contains statements AND expressions
    /// </summary>
    public class GeneralParser
    {
        public delegate Node ParseArgumentsMethod(List<List<Token>> args, List<Token> tokens);

        public delegate Node ParseMethod(List<Token> tokens);

        public static List<string> Keywords = new List<string>
        {
            "if",
            "while",
            "for",
            "func",
            "class",
            "using",
            "break",
            "continue",
            "const"
        };

        public static List<string> NotPermittedInExpression = new List<string>
        {
            "if",
            "while",
            "for",
            "func",
            "class",
            "using",
            "const"
        };

        public static List<string> ExpressionBreakWords = new List<string>
        {
            "if",
            "while",
            "for",
            "func",
            "class",
            "using",
            //"break",
            //"continue",
        };

        public static List<string> Modifiers = new List<string>
        {
            "public",
            "private",
            "const",
            "static"
        };

        private readonly SkryptEngine _engine;

        public GeneralParser(SkryptEngine e)
        {
            _engine = e;
        }
      
        public List<Token> GetSurroundedTokens(string open, string close, int start, List<Token> tokens)
        {
            var index = start;

            var i = index + 1;
            var skip = _engine.ExpressionParser.SkipFromTo(open, close, tokens, index);
            var end = skip.End;
            index = skip.End;

            return tokens.GetRange(i, end - i);
        }

        /// <summary>
        ///     Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult ParseSurrounded(string open, string close, int start, List<Token> tokens,
            ParseMethod parseMethod)
        {
            var surroundedTokens = GetSurroundedTokens(open, close, start, tokens);

            var node = parseMethod(surroundedTokens);

            return new ParseResult {Node = node, Delta = surroundedTokens.Count + 1};
        }

        /// <summary>
        ///     Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult ParseSurroundedExpressions(string open, string close, int start, List<Token> tokens)
        {
            var surroundedTokens = GetSurroundedTokens(open, close, start, tokens);

            var node = new Node();
            var arguments = new List<List<Token>>();
            ExpressionParser.SetArguments(arguments, surroundedTokens);

            foreach (var argument in arguments)
            {
                var argNode = _engine.ExpressionParser.ParseClean(argument);
                node.Add(argNode);
            }

            return new ParseResult {Node = node, Delta = surroundedTokens.Count + 1};
        }

        private ParseResult TryParse(List<Token> tokens)
        {
            var i = 0;
            var t = tokens[i];
            var usedModifiers = new List<string>();
            Modifier appliedModifiers = Modifier.None;

            while (Modifiers.Contains(t.Value)) {
                if (usedModifiers.Contains(t.Value)) {
                    _engine.ThrowError("Object can't be marked with multiple of the same modifiers", t);
                }

                if (usedModifiers.Contains("public") && t.Value == "private") {
                    _engine.ThrowError("Object can't be marked as both private and public", t);
                }

                if (usedModifiers.Contains("private") && t.Value == "public") {
                    _engine.ThrowError("Object can't be marked as both private and public", t);
                }

                usedModifiers.Add(t.Value);

                switch (t.Value) {
                    case "public":
                        appliedModifiers = appliedModifiers | Modifier.Public;
                        break;
                    case "private":
                        appliedModifiers = appliedModifiers & ~Modifier.Public;
                        break;
                    case "static":
                        appliedModifiers = appliedModifiers | Modifier.Static;
                        break;
                    case "const":
                        appliedModifiers = appliedModifiers | Modifier.Const;
                        break;
                }

                i++;
                t = tokens[i];
            }

            var parseTokens = tokens.GetRange(i, tokens.Count - i);
            ParseResult result = null;

            if (parseTokens[0].Value == "if" || parseTokens[0].Value == "while" || parseTokens[0].Value == "for")
                result = _engine.StatementParser.Parse(parseTokens);
            else if (parseTokens[0].Value == "func")
                result = _engine.MethodParser.Parse(parseTokens);
            else if (parseTokens[0].Value == "class")
                result = _engine.ClassParser.Parse(parseTokens);
            else if (parseTokens[0].Value == "using")
                result = _engine.ExpressionParser.ParseUsing(parseTokens);
            else
                result = _engine.ExpressionParser.Parse(parseTokens);

            result.Node.Modifiers = appliedModifiers;

            _engine.ModifierChecker.CheckModifiers(result.Node);

            result.Delta += usedModifiers.Count;

            return result;
        }

        public Node Parse(List<Token> tokens)
        {
            // Create main node
            var node = new Node {Body = "Block", TokenType = "Block"};

            if (tokens.Count == 0) return node;

            var i = 0;

            while (i < tokens.Count - 1)
            {
                var test = TryParse(tokens.GetRange(i, tokens.Count - i));
                i += test.Delta;

                if (test.Node.TokenType == "MethodDeclaration")
                {
                    node.AddAsFirst(test.Node);
                    continue;
                }

                node.Add(test.Node);
            }

            return node;
        }
    }
}