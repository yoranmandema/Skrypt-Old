using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    static class StatementParser {
        static public Node Parse(List<Token> Tokens, ref int Index) {
            if (!(Tokens[Index].Value == "if")) {
                return null;
            }

            Node node = new Node { Body = Tokens[Index].Value, TokenType = "Statement" };
            Index += 2;
            int i = Index;
            int endCondition = i;
            ExpressionParser.SkipArguments(ref Index, ref endCondition, "(", ")", Tokens);

            Node conditionNode = new Node {Body = "Condition", TokenType = "Expression"};
            conditionNode.Add(ExpressionParser.ParseExpression(conditionNode, Tokens.GetRange(i, endCondition - i)));

            Index++;
            i = Index;
            int endBlock = i;
            ExpressionParser.SkipArguments(ref Index, ref endBlock, "{", "}", Tokens);

            Node blockNode = GeneralParser.Parse(Tokens.GetRange(i, endBlock - i));
            Index--;

            node.Add(conditionNode);
            node.Add(blockNode);

            return node;
        }
    }
}
