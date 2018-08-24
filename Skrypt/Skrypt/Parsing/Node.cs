using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Tokenization;
using System;
using System.Text.RegularExpressions;
using C = Colorful.Console;
using System.Drawing;

namespace Skrypt.Parsing
{
    public enum Modifier : byte {
        None = 2,
        Public = 4,
        Private = 8,
        Instance = 16,
        Const = 32,
        Strong = 64,
    }

    /// <summary>
    ///     Class representing a node or AST
    /// </summary>
    public class Node
    {
        public virtual string Body { get; set; }

        //public string TokenType { get; set; }
        public virtual TokenTypes Type { get; set; }
        [JsonIgnore] public Token Token { get; set; }
        [JsonIgnore] public List<Node> Nodes { get; set; } = new List<Node>();
        [JsonIgnore] public Modifier Modifiers { get; set; } = Modifier.None;

        /// <summary>
        ///     Adds a subnode
        /// </summary>
        public Node Add (Node node) {

            if (node == null) {
                return node;
            }

            Nodes.Add(node);
            return node;
        }

        /// <summary>
        ///     Adds a subnode to the beginning of all subnodes
        /// </summary>
        public void AddAsFirst(Node node)
        {
            if (node == null) return;

            Nodes.Insert(0, node);
        }

        /// <summary>
        ///     Clones the subnodes of another node and adds them
        /// </summary>
        public Node CloneNodes (Node node) {
            this.Nodes.AddRange(new List<Node>(node.Nodes));

            return this;
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

            foreach (var subnode in Nodes) {
                var sns = subnode.ToString();

                var lines = sns.Split('\n');

                foreach (var l in lines) {
                    s += "| " + l;
                }
            }

            //s = new Regex(@"[""\[\]\{\}\,\r]").Replace(s,"");

            return s;
        }

        public void Print(string indent = "") {
            var s = Body;

            C.Write(indent, Color.FromArgb(100, 100, 100));

            switch (Type) {
                case TokenTypes.Conditional:
                    C.WriteLine(Body, Color.FromArgb(217, 220, 220));

                    C.Write("| " + indent, Color.FromArgb(100, 100, 100));
                    C.WriteLine("Condition:", Color.FromArgb(217, 220, 220));
                    ((ConditionalNode)this).Condition.Print(indent + "| | ");
                    C.Write("| " + indent, Color.FromArgb(100, 100, 100));
                    C.WriteLine("Pass:", Color.FromArgb(217, 220, 220));
                    ((ConditionalNode)this).Pass.Print(indent + "| | ");
                    C.Write("| " + indent, Color.FromArgb(100, 100, 100));
                    C.WriteLine("Fail:", Color.FromArgb(217, 220, 220));
                    ((ConditionalNode)this).Fail.Print(indent + "| | ");

                    break;
                case TokenTypes.ArrayLiteral:
                    C.WriteLine(Body, Color.FromArgb(217, 220, 220));

                    foreach (var n in ((ArrayNode)this).Values) {
                        n.Print(indent + "| ");
                    }

                    break;
                case TokenTypes.FunctionLiteral:
                case TokenTypes.NullLiteral:
                    C.Write(Body, Color.FromArgb(208, 25, 208));
                    break;
                case TokenTypes.BooleanLiteral:
                    C.Write(((BooleanNode)this).Value, Color.FromArgb(208, 25, 208));
                    break;
                case TokenTypes.NumericLiteral:
                    C.Write(((NumericNode)this).Value, Color.FromArgb(184, 215, 163));
                    break;
                case TokenTypes.Call:
                case TokenTypes.Punctuator:
                    C.Write(Body, Color.FromArgb(66, 147, 208));
                    break;
                case TokenTypes.Parameter:
                case TokenTypes.Identifier:
                    C.Write(Body, Color.FromArgb(217, 220, 220));
                    break;
                case TokenTypes.StringLiteral:
                    C.Write('"', Color.FromArgb(214, 157, 133));

                    var lines = ((StringNode)this).Value.Split('\n');

                    for (int i = 0; i < lines.Length; i++) {
                        C.Write(lines[i] + (i < lines.Length-1 ? "\n" : ""), Color.FromArgb(214, 157, 133));
                        if(i < lines.Length - 1)
                            C.Write(indent, Color.FromArgb(100, 100, 100));
                    }

                    C.Write('"', Color.FromArgb(214, 157, 133));

                    break;
                case TokenTypes.ClassDeclaration:
                    C.Write("Class " + Body, Color.FromArgb(208, 25, 208));
                    break;
                case TokenTypes.MethodDeclaration:
                    C.Write("Function " + Body, Color.FromArgb(208, 25, 208));
                    break;
                default:
                    C.Write(Body);
                    break;
            }

            C.ResetColor();
            C.Write("\n");

            foreach (var subnode in Nodes) {
                subnode.Print(indent + "| ");
            }
        }
    }
}