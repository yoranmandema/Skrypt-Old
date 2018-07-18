using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Tokenization;
using System;

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
        public List<Node> SubNodes { get; set; } = new List<Node>();
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
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }
    }
}