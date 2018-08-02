using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Tokenization;
using System;
using System.Text.RegularExpressions;

namespace Skrypt.Parsing
{
    public enum Modifier : byte {
        None = 2,
        Public = 4,
        Private = 8,
        Static = 16,
        Const = 32,
        Strong = 64,
    }

    /// <summary>
    ///     Class representing a node or AST
    /// </summary>
    public class Node
    {
        public string Body { get; set; }

        public string TokenType { get; set; }
        [JsonIgnore] public Token Token { get; set; }
        [JsonIgnore] public List<Node> SubNodes { get; set; } = new List<Node>();
        [JsonIgnore] public Modifier Modifiers { get; set; } = Modifier.None;

        /// <summary>
        ///     Adds a subnode
        /// </summary>
        public Node Add (Node node) {

            if (node == null) {
                return node;
            }

            SubNodes.Add(node);
            return node;
        }

        /// <summary>
        ///     Adds a subnode to the beginning of all subnodes
        /// </summary>
        public void AddAsFirst(Node node)
        {
            if (node == null) return;

            SubNodes.Insert(0, node);
        }


        public override string ToString()
        {
            //var s = JsonConvert.SerializeObject(this, Formatting.Indented);

            //s = new Regex(@"[""\[\]\{\}\,\r]").Replace(s,"");
            //s = new Regex(@"\n\s*?(?<!\\)\n").Replace(s, "\n");

            //foreach (var c in s) {
            //    Console.WriteLine(Regex.Escape("" + c));
            //}

            var s = JsonConvert.SerializeObject(this, Formatting.None);
            s = new Regex(@",").Replace(s, "\n");
            s = new Regex(@"""").Replace(s,"");
            s = new Regex(@":").Replace(s, ": ");
            s = new Regex(@"[\{\}]").Replace(s, "");

            foreach (var subnode in SubNodes) {
                var sns = subnode.ToString();
                var lines = sns.Split('\n');

                foreach (var l in lines) {

                    var lineToken = l.StartsWith("Body") ? "| " : "| ";
                        s += $"\n{lineToken}" + l;
                }
            }

            //s = new Regex(@"[""\[\]\{\}\,\r]").Replace(s,"");

            return s;
        }

        public void Print(string indent = "") {
            var s = Body;

            Console.Write(indent);

            switch (TokenType) {
                case "FunctionLiteral":
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write(Body);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "BooleanLiteral":
                case "NumericLiteral":
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write(Body);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "Call":
                case "Punctuator":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(Body);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "Parameter":
                case "Identifier":
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Body);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "StringLiteral":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write('"' + Body + '"');
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "ClassDeclaration":
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("Class " + Body);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case "MethodDeclaration":
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("Function " + Body);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    Console.Write(Body);
                    break;
            }

            Console.Write("\n");

            foreach (var subnode in SubNodes) {
                subnode.Print(indent + "| ");
            }
        }
    }
}