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
    class GeneralParser {
        SkryptEngine engine;

        public GeneralParser(SkryptEngine e) {
            engine = e;
        }

        ParseResult TryParse (List<Token> Tokens) {
            Exception error = null;
            ParseResult ExpressionResult = new ParseResult();
            ParseResult StatementResult = new ParseResult();

            try {
                ParseResult result = engine.expressionParser.Parse(Tokens);

                ExpressionResult.node = result.node;
                ExpressionResult.delta = result.delta;
            } catch (Exception e) {
                error = e;
            }

            try {
                ParseResult result = engine.statementParser.Parse(Tokens);

                StatementResult.node = result.node;
                StatementResult.delta = result.delta;
            }
            catch (Exception e) {
                error = e;
            }

            if (ExpressionResult.node != null) {
                return ExpressionResult;
            } else if (StatementResult.node != null) {
                return StatementResult;
            } else {
                engine.throwError(error.StackTrace, Tokens[0]);
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
