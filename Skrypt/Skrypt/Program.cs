using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using System.Text.RegularExpressions;

namespace Skrypt {
    class Program {
        static void Main(string[] args) {
            Tokenizer T = new Tokenizer();

            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            T.TokenRules.Add(new TokenRule {
                Pattern = new Regex(@"\s"),
                Type = null
            });

            T.TokenRules.Add(new TokenRule {
                Pattern = new Regex(@"\d+(\.\d+)?"),
                Type = "NumericLiteral"
            });

            T.TokenRules.Add(new TokenRule {
                Pattern = new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                Type = "Identifier"
            });

            T.TokenRules.Add(new TokenRule {
                Pattern = new Regex(@"(&&)|(\|\|)|(\|\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>)|(>>>)|(\+\+)|(--)|[~=;<>+\-*/%^&|!\[\]\(\)\.\,{}]"),
                Type = "Punctuator"
            });

            T.TokenRules.Add(new TokenRule {
                Pattern = new Regex(@"[""\'][^""\\]*(?:\\.[^""\\]*)*[""\']"),
                Type = "StringLiteral"
            });

            // Tokenizing a test string.
            var Tokens = T.Tokenize("doStuff (\"wdwd\")");

            // Debug token list print.
            if (Tokens != null) {
                foreach (Token token in Tokens) {
                    Console.WriteLine(token);
                }
            }

            Console.ReadKey();
        }
    }
}
