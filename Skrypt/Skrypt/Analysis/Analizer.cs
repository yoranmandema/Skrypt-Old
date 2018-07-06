using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Parsing;
using Skrypt.Execution;
using Skrypt.Library;
using System.Reflection;

namespace Skrypt.Analysis {
    /// <summary>
    /// The node analizer class.
    /// Analizes nodes to check for errors.
    /// </summary>
    public class Analizer {
        SkryptEngine engine;

        public Analizer(SkryptEngine e) {
            engine = e;
        }
        
        public void AnalizeStatement (Node node, ScopeContext scopeContext) {
            
            // Check statement
            Node conditionNode = node.SubNodes[0];
            SkryptObject result = engine.executor.ExecuteExpression(conditionNode, scopeContext);

            // Check block
            Node blockNode = node.SubNodes[1];
            Analize(blockNode, scopeContext);

            // Check else/elseif
            if (node.SubNodes.Count > 2) {
                for (int i = 2; i < node.SubNodes.Count; i++) {
                    // elseif block
                    Node elseNode = node.SubNodes[i];

                    if (elseNode.Body == "elseif") {
                        AnalizeStatement(elseNode, scopeContext);
                    } else {
                        Analize(elseNode, scopeContext);
                    }
                }
            }
        }

        public void Analize (Node node, ScopeContext scopeContext) {
            foreach (Node subNode in node.SubNodes) {
                if (subNode.TokenType == "Statement") {
                    AnalizeStatement(subNode, scopeContext);
                }
                else {
                    SkryptObject result = engine.executor.ExecuteExpression(subNode, scopeContext);
                }
            }
        }
    }
}
