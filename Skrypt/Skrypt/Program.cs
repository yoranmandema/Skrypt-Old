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
                Pattern = new Regex(@"-?\d+(\.\d*)?([eE][-+]?\d+)?", RegexOptions.IgnoreCase),
                Type = "Numeric"
            });

            var Tokens = T.Tokenize("1. s 2.00 3");

            Console.WriteLine(Tokens);

            Console.ReadKey();
        }
    }
}
