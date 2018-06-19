using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skrypt.Parsing {

    /// <summary>
    /// Class representing a node or AST
    /// </summary>
    class Node {
        public string Body { get; set; }
        //public object Value { get; set; }
        public string TokenType { get; set; }
        public List<Node> SubNodes { get; set; } = new List<Node>();

        /// <summary>
        /// Adds a subnode
        /// </summary>
        public void Add (Node node) {

            if (node == null) {
                return;
            }

            SubNodes.Add(node);
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"","");
        }
    }
}
