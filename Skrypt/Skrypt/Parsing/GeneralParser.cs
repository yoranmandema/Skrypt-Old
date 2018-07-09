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
        public delegate Node parseArgumentsMethod(List<List<Token>> Args, List<Token> Tokens);

        public delegate Node parseMethod(List<Token> Tokens);

        public static List<string> Keywords = new List<string>
        {
            "if",
            "while",
            "for",
            "func",
            "class"
        };

        private readonly SkryptEngine engine;

        public GeneralParser(SkryptEngine e)
        {
            engine = e;
        }

        private List<Token> GetSurroundedTokens(string open, string close, int start, List<Token> Tokens)
        {
            var index = start;

            var i = index + 1;
            var skip = engine.expressionParser.SkipFromTo(open, close, Tokens, index);
            var end = skip.end;
            index = skip.end;

            return Tokens.GetRange(i, end - i);
        }

        /// <summary>
        ///     Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult parseSurrounded(string open, string close, int start, List<Token> Tokens,
            parseMethod parseMethod)
        {
            var SurroundedTokens = GetSurroundedTokens(open, close, start, Tokens);

            var node = parseMethod(SurroundedTokens);

            return new ParseResult {node = node, delta = SurroundedTokens.Count + 1};
        }

        private Exception GetExceptionBasedOnUrgency(Exception e, ref int highestErrorUrgency)
        {
            Exception error = null;

            if (e.GetType() == typeof(SkryptException))
            {
                var cast = (SkryptException) e;

                if (cast.urgency >= highestErrorUrgency)
                {
                    error = e;
                    highestErrorUrgency = cast.urgency;
                }
            }

            return error;
        }

        /// <summary>
        ///     Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult parseSurroundedExpressions(string open, string close, int start, List<Token> Tokens)
        {
            var SurroundedTokens = GetSurroundedTokens(open, close, start, Tokens);

            var node = new Node();
            var Arguments = new List<List<Token>>();
            ExpressionParser.SetArguments(Arguments, SurroundedTokens);

            foreach (var Argument in Arguments)
            {
                var argNode = engine.expressionParser.ParseClean(Argument);
                node.Add(argNode);
            }

            return new ParseResult {node = node, delta = SurroundedTokens.Count + 1};
        }

        private ParseResult TryParse(List<Token> Tokens)
        {
            if (Tokens[0].Value == "if" || Tokens[0].Value == "while")
                return engine.statementParser.Parse(Tokens);
            if (Tokens[0].Value == "func")
                return engine.methodParser.Parse(Tokens);
            if (Tokens[0].Value == "class")
                return engine.classParser.Parse(Tokens);
            return engine.expressionParser.Parse(Tokens);
        }

        public Node Parse(List<Token> Tokens)
        {
            // Create main node
            var Node = new Node {Body = "Block", TokenType = "Block"};

            if (Tokens.Count == 0) return Node;

            var i = 0;

            while (i < Tokens.Count - 1)
            {
                var Test = TryParse(Tokens.GetRange(i, Tokens.Count - i));
                i += Test.delta;

                if (Test.node.TokenType == "MethodDeclaration")
                {
                    Node.AddAsFirst(Test.node);
                    continue;
                }

                Node.Add(Test.node);
            }

            return Node;
        }
    }
}