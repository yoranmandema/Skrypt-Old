using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    class Parser {
        public ParserState State;
        public List<OperatorCategory> Operators;

        public Parser() {
            Operators.Add(new OperatorCategory(new string[] { "!", "~", "--", "++" }, true, 1));
            Operators.Add(new OperatorCategory(new string[] { "^" }));
            Operators.Add(new OperatorCategory(new string[] { "*", "/", "%" }));
            Operators.Add(new OperatorCategory(new string[] { "-", "+" }));
            Operators.Add(new OperatorCategory(new string[] { "<<", ">>", ">>>" }));
            Operators.Add(new OperatorCategory(new string[] { "<", ">", ">=", "<=" }));
            Operators.Add(new OperatorCategory(new string[] { "==", "!=" }));
            Operators.Add(new OperatorCategory(new string[] { "&" }));
            Operators.Add(new OperatorCategory(new string[] { "|||" }));
            Operators.Add(new OperatorCategory(new string[] { "|" }));
            Operators.Add(new OperatorCategory(new string[] { "&&" }));
            Operators.Add(new OperatorCategory(new string[] { "||" }));
            Operators.Add(new OperatorCategory(new string[] { "?", ":" }, true, 3));
            Operators.Add(new OperatorCategory(new string[] { "=", "+=", "-=", "*=", "/=", "%=", ">>=", "<<=", ">>>=", "&=", "|||=", "|=" }, true));
            Operators.Add(new OperatorCategory(new string[] { "," }));
            Operators.Add(new OperatorCategory(new string[] { "return" }, true, 1));
            Operators.Reverse();
        }

        public Node Parse (List<Token> tokens) {
            Node Program = new Node() { Body = "Program" };

            return Program;
        }
    }

    public class OperatorCategory {
        public string[] operators;
        public bool rightToLeft = false;
        public byte count = 2;

        public OperatorCategory(string[] ops, bool rtl = false, byte cnt = 2) {
            operators = ops;
            rightToLeft = rtl;
            count = cnt;
        }
    }
}
