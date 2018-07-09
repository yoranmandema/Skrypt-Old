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
            "class"
        };

        private readonly SkryptEngine _engine;

        public GeneralParser(SkryptEngine e)
        {
            _engine = e;
        }
      
        private List<Token> GetSurroundedTokens(string open, string close, int start, List<Token> tokens)
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

        private Exception GetExceptionBasedOnUrgency(Exception e, ref int highestErrorUrgency)
        {
            Exception error = null;

            if (e.GetType() == typeof(SkryptException))
            {
                var cast = (SkryptException) e;

                if (cast.Urgency >= highestErrorUrgency)
                {
                    error = e;
                    highestErrorUrgency = cast.Urgency;
                }
            }

            return error;
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
            if (tokens[0].Value == "if" || tokens[0].Value == "while")
                return _engine.StatementParser.Parse(tokens);
            if (tokens[0].Value == "func")
                return _engine.MethodParser.Parse(tokens);
            if (tokens[0].Value == "class")
                return _engine.ClassParser.Parse(tokens);
            return _engine.ExpressionParser.Parse(tokens);
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