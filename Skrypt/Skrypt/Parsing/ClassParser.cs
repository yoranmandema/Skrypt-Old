using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Parsing;

namespace Skrypt.Parsing {
    static class ClassParser {



        static public SkryptClass Parse (List<Token> Tokens, ref int Index) {
            bool HasClassKeyword = Tokens[0].Has("Keyword", "class");
            bool HasIdentifier = Tokens[1].Type == "Identifier";
            bool HasClosingBlock = false;
            int BlockDepth = 0;
            int BlockStartIndex = -1;

            foreach (Token token in Tokens) {
                if (token.Has("Punctuator", "{")) {
                    BlockDepth++;

                    if (BlockStartIndex == -1)
                        BlockStartIndex = Index;
                } else if (token.Has("Punctuator", "}")) {
                    BlockDepth--;
                }

                Index++;
            }

            HasClosingBlock = BlockDepth == 0;

            List <Token> BlockTokens = Tokens.GetRange(BlockStartIndex + 1, Tokens.Count - BlockStartIndex - 1);

            SkryptClass Class = new SkryptClass();
            Class.Name = Tokens[1].Value;
            Class.Generate(BlockTokens);

            return Class;
        }
    }
}
