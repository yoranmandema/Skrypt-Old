using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    static class StatementParser {
        static public Node ParseStatement (List<Token> Tokens, ref int Index) {

            Node node = new Node { Body = Tokens[Index].Value, TokenType = "Statement" };
            Index += 2;
            int i = Index;
            int endCondition = i;
            ExpressionParser.SkipArguments(ref Index, ref endCondition, "(", ")", Tokens);

            Node conditionNode = new Node { Body = "Condition", TokenType = "Expression" };
            conditionNode.Add(ExpressionParser.ParseExpression(conditionNode, Tokens.GetRange(i, endCondition - i)));

            Index++;
            i = Index;
            int endBlock = i;
            ExpressionParser.SkipArguments(ref Index, ref endBlock, "{", "}", Tokens);

            Node blockNode = GeneralParser.Parse(Tokens.GetRange(i, endBlock - i));

            node.Add(conditionNode);
            node.Add(blockNode);

            Index--;

            return node;
        }

        static public Node ParseElseStatement(List<Token> Tokens, ref int Index) {
            Index += 3;
            int i = Index;
            int endBlock = i;
            ExpressionParser.SkipArguments(ref Index, ref endBlock, "{", "}", Tokens);

            Node node = GeneralParser.Parse(Tokens.GetRange(i, endBlock - i));
            node.Body = "else";

            return node;
        }

        static public Node ParseIfStatement(List<Token> Tokens, ref int Index) {
            Node node = ParseStatement(Tokens, ref Index);
            if (Index < Tokens.Count) {
                Console.WriteLine(Tokens[Index + 1]);

                while (Tokens[Index + 1].Value == "elseif") {
                    Index += 1;
                    Node elseIfNode = ParseStatement(Tokens, ref Index);
                    node.Add(elseIfNode);
                }

                if (Tokens[Index + 1].Value == "else") {
                    Node elseNode = ParseElseStatement(Tokens, ref Index);
                    node.Add(elseNode);
                }
            }

            return node;
        }

        static public Node Parse(List<Token> Tokens, ref int Index) {
            if (!(Tokens[Index].Value == "if")) {
                return null;
            }

            switch (Tokens[Index].Value) {
                case "while":
                    return ParseStatement(Tokens, ref Index);
                case "if":
                    return ParseIfStatement(Tokens, ref Index);
            }

            return null;
        }
    }
}
