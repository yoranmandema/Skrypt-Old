using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Skrypt.Tokenization;

namespace Skrypt.Library {
    class SkryptObject {
        public string Name { get; set; }
        public List<SkryptProperty> Properties = new List<SkryptProperty>();
        public List<SkryptMethod> Methods = new List<SkryptMethod>();
        //public List<SkryptOperator> Operators = new List<SkryptOperator>();

        //public override string ToString() {
        //    return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        //}

        public void Generate (List<Token> Tokens) {
            //for (int i = 0; i < Tokens.Count-1; i++) {
            //    var N = ExpressionParser.Parse(Tokens, ref i);
            //    Console.WriteLine(N);
            //}
        }
    }
}
