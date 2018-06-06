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

            T.TokenRules.Add(new TokenRule {
                Pattern = new Regex(@"\d+(\.\d+)?", RegexOptions.IgnoreCase),
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

            var Tokens = T.Tokenize("doStuff (\"wdwd\")");

            if (Tokens != null) 
            foreach (Token token in Tokens) {
                Console.WriteLine(token);
            }

            Console.ReadKey();
        }
    }
}
