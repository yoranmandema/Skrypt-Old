using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    /// <summary>
    /// The general parser class.
    /// Contains all methods to parse higher-level code, e.g code that contains statements AND expressions
    /// </summary>
    static class GeneralParser {
        public static Node Parse(List<Token> Tokens) {
            // Create main node
            Node Node = new Node { Body = "Block", TokenType = "Block" };

            // Loop through all tokens
            for (int i = 0; i < Tokens.Count - 1; i++) {
                var ExpressionNode = ExpressionParser.Parse(Tokens, ref i);
                Node.Add(ExpressionNode);

                var StatementNode = StatementParser.Parse(Tokens, ref i);
                Node.Add(StatementNode);
            }

            return Node;
        }
    }
}
