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

        class Result {
            public Node node;
            public Exception error;
            public int delta = -1;
        }

        Result TryParse (List<Token> Tokens) {
            Exception error = null;
            Result ExpressionResult = new Result();
            Result StatementResult = new Result();

            try {
                var i = 0;
                Node node = engine.expressionParser.Parse(Tokens, ref i);

                ExpressionResult.node = node;
                ExpressionResult.delta = i + 1;
            } catch (Exception e) {
                error = e;
                ExpressionResult.error = e;
            }

            try {
                var i = 0;
                Node node = engine.statementParser.Parse(Tokens, ref i);

                StatementResult.node = node;
                StatementResult.delta = i + 1;
            }
            catch (Exception e) {
                error = e;
                StatementResult.error = e;
            }

            if (ExpressionResult.node != null) {
                return ExpressionResult;
            } else if (StatementResult.node != null) {
                return StatementResult;
            } else {
                engine.throwError(error.Message, Tokens[0]);
                return null;
            }
        }

        public Node Parse(List<Token> Tokens) {
            if (Tokens.Count == 0) {
                return null;
            }

            // Create main node
            Node Node = new Node { Body = "Block", TokenType = "Block" };

            //// Loop through all tokens
            //for (int i = 0; i < Tokens.Count; i++) {           
            //    var ExpressionNode = engine.expressionParser.Parse(Tokens, ref i);
            //    Node.Add(ExpressionNode);

            //    var StatementNode = engine.statementParser.Parse(Tokens, ref i);
            //    Node.Add(StatementNode);
            //}

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
