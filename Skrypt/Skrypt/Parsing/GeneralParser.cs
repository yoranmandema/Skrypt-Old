using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Engine;

namespace Skrypt.Parsing {
    /// <summary>
    /// The general parser class.
    /// Contains all methods to parse higher-level code, e.g code that contains statements AND expressions
    /// </summary>
    public class GeneralParser {
        SkryptEngine engine;

        public GeneralParser(SkryptEngine e) {
            engine = e;
        }

        public delegate Node parseMethod(List<Token> Tokens);
        public delegate Node parseArgumentsMethod(List<List<Token>> Args, List<Token> Tokens);

        List<Token> GetSurroundedTokens (string open, string close, int start, List<Token> Tokens) {
            int index = start;

            int i = index + 1;
            skipInfo skip = engine.expressionParser.SkipFromTo(open, close, Tokens, index);
            int end = skip.end;
            index = skip.end;

            return Tokens.GetRange(i, end - i);
        }

        /// <summary>
        /// Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult parseSurrounded(string open, string close, int start, List<Token> Tokens, parseMethod parseMethod) {
            List<Token> SurroundedTokens = GetSurroundedTokens(open, close, start, Tokens);

            Node node = parseMethod(SurroundedTokens);

            return new ParseResult { node = node, delta = SurroundedTokens.Count + 1 };
        }

        Exception GetExceptionBasedOnUrgency (Exception e, ref int highestErrorUrgency) {
            Exception error = null;

            if (e.GetType() == typeof(SkryptException)) {
                SkryptException cast = (SkryptException)e;

                if (cast.urgency >= highestErrorUrgency) {
                    error = e;
                    highestErrorUrgency = cast.urgency;
                }
            }

            return error;
        }

        /// <summary>
        /// Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult parseSurroundedExpressions (string open, string close, int start, List<Token> Tokens) {
            List<Token> SurroundedTokens = GetSurroundedTokens(open, close, start, Tokens);

            Node node = new Node ();
            List<List<Token>> Arguments = new List<List<Token>>();
            ExpressionParser.SetArguments(Arguments, SurroundedTokens);

            foreach (List<Token> Argument in Arguments) {
                Node argNode = engine.expressionParser.ParseClean(Argument);
                node.Add(argNode);
            }

            return new ParseResult { node = node, delta = SurroundedTokens.Count + 1 };
        }

        ParseResult TryParse (List<Token> Tokens) {

            int highestErrorUrgency = -1;
            Exception error = null;
            ParseResult ExpressionResult = null;
            ParseResult StatementResult = null;
            ParseResult MethodResult = null;

            try {
                ParseResult result = engine.expressionParser.Parse(Tokens);

                ExpressionResult = result;
            }
            catch (Exception e) {
                Exception test = GetExceptionBasedOnUrgency(e, ref highestErrorUrgency);

                if (test != null)
                    error = test;
            }

            try {
                ParseResult result = engine.statementParser.Parse(Tokens);

                StatementResult = result;
            }
            catch (Exception e) {
                Exception test = GetExceptionBasedOnUrgency(e, ref highestErrorUrgency);

                if (test != null)
                    error = test;
            }

            try {
                ParseResult result = engine.methodParser.Parse(Tokens);

                MethodResult = result;
            }
            catch (Exception e) {
                Exception test = GetExceptionBasedOnUrgency(e, ref highestErrorUrgency);

                if (test != null)
                    error = test;
            }

            if (highestErrorUrgency > -1) {
                engine.throwError(error.Message);
                return null;
            }

            if (StatementResult != null) {
                return StatementResult;
            } else if (ExpressionResult != null) {
                return ExpressionResult;
            } else if (MethodResult != null) {
                return MethodResult;
            } else {
                //throw error;
                engine.throwError(error.Message, Tokens[0]);
                return null;
            }
        }

        public Node Parse(List<Token> Tokens) {
            // Create main node
            Node Node = new Node { Body = "Block", TokenType = "Block" };

            if (Tokens.Count == 0) {
                return Node;
            }

            int i = 0;

            while (i < Tokens.Count - 1) {
                var Test = TryParse(Tokens.GetRange(i,Tokens.Count - i));
                i += Test.delta;

                Node.Add(Test.node);
            }

            return Node;
        }
    }
}
