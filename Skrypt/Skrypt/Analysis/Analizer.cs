using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Parsing;
using Skrypt.Library;
using Skrypt.Library.SkryptClasses;

namespace Skrypt.Analysis {
    /// <summary>
    /// The node analizer class.
    /// Analizes nodes to check for errors.
    /// </summary>
    class Analizer {
        SkryptEngine engine;

        public Analizer(SkryptEngine e) {
            engine = e;
        }

        public void AnalizeExpression (Node node) {
            Operator op = Operator.AllOperators.Find(o => o.OperationName == node.Body || o.Operation == node.Body);

            if (op != null) {
                Console.WriteLine(op.OperationName);

                int Members = node.SubNodes.Count;

                if (Members != op.Members) {
                    engine.throwError("Missing member of operation!",node.Token);
                }

                foreach (Node subNode in node.SubNodes) {
                    AnalizeExpression(subNode);
                }
            }
        }

        public void Analize (Node node) {
            foreach (Node subNode in node.SubNodes) {
                AnalizeExpression(subNode);
            }
        }
    }
}
