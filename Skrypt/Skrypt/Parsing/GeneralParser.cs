using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    static class GeneralParser {
        public static Node Parse(List<Token> Tokens) {
            Node Node = new Node { Body = "Block", TokenType = "Block" };

            for (int i = 0; i < Tokens.Count - 1; i++) {
                var ExpressionNode = ExpressionParser.Parse(Tokens, ref i);

                if (ExpressionNode != null) {
                    Node.Add(ExpressionNode);
                }

                var StatementNode = StatementParser.Parse(Tokens, ref i);

                if (StatementNode != null) { 
                    Node.Add(StatementNode);
                }
            }

            return Node;
        }
    }
}
