using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics; // Using this so we can check how fast everything is happening

namespace Skrypt {
    class Program {
        static void Main(string[] args) {
            Stopwatch sw = new Stopwatch();
            Tokenizer T = new Tokenizer();

            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            T.AddRule(
                new Regex(@"\s"),
                null
            );

            T.AddRule(
                new Regex(@"\d+(\.\d+)?"),
                "NumericLiteral"
            );

            T.AddRule(
                new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                "Identifier"
            );

            T.AddRule(
                new Regex("true|false"),
                "BooleanLiteral"
            );

            T.AddRule(
                new Regex(@"(&&)|(\|\|)|(\|\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>)|(>>>)|(\+\+)|(--)|[~=;:<>+\-*/%^&|!\[\]\(\)\.\,{}]"),
                "Punctuator"
            );

            T.AddRule(
                new Regex(@""".*?(?<!\\)"""),
                "StringLiteral"
            );

            // Multi line comment
            T.AddRule(
                new Regex(@"\/\*(.|\n)*\*\/"),
                null
            );

            // Single line comment
            T.AddRule(
                new Regex(@"\/\/.*\n"),
                null
            );

            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"..\..\SkryptFiles\testcode.txt");
            var code = File.ReadAllText(path);

            sw.Start();
            // Tokenizing the file.
            var Tokens = T.Tokenize(code);
            if (Tokens != null)
                TokenProcessor.ProcessTokens(Tokens);
            sw.Stop();

            // Debug token list print.
            if (Tokens != null) {
                foreach (Token token in Tokens) {
                    Console.WriteLine(token);
                }
            }

            Console.WriteLine("Tokenization time: {0} ms", sw.Elapsed.Milliseconds);

            Console.ReadKey();
        }
    }
}
