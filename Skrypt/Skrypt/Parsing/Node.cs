using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skrypt.Parsing {
    class Node {

        public string Body { get; set; }
        public object Value { get; set; }
        public string TokenType { get; set; }
        public List<Node> SubNodes { get; set; }

        public Node () {
            SubNodes = new List<Node>();
        }

        public void Add (Node node) {
            SubNodes.Add(node);
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"","");
        }
    }
}
