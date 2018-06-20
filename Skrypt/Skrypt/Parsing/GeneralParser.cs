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

        public Node Parse(List<Token> Tokens) {
            // Create main node
            Node Node = new Node { Body = "Block", TokenType = "Block" };

            // Loop through all tokens
            for (int i = 0; i < Tokens.Count - 1; i++) {
                var ExpressionNode = engine.expressionParser.Parse(Tokens, ref i);
                Node.Add(ExpressionNode);

                var StatementNode = engine.statementParser.Parse(Tokens, ref i);
                Node.Add(StatementNode);
            }

            return Node;
        }
    }
}
