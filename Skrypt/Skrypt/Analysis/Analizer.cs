using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Parsing;

namespace Skrypt.Analysis
{
    /// <summary>
    ///     The node analizer class.
    ///     Analizes nodes to check for errors.
    /// </summary>
    public class Analizer
    {
        private readonly SkryptEngine _engine;

        public Analizer(SkryptEngine e)
        {
            _engine = e;
        }

        public void AnalizeStatement(Node node, ScopeContext scopeContext)
        {
            // Check statement
            var conditionNode = node.SubNodes[0];
            var result = _engine.Executor.ExecuteExpression(conditionNode, scopeContext);

            // Check block
            var blockNode = node.SubNodes[1];
            Analize(blockNode, scopeContext);

            // Check else/elseif
            if (node.SubNodes.Count > 2)
                for (var i = 2; i < node.SubNodes.Count; i++)
                {
                    // elseif block
                    var elseNode = node.SubNodes[i];

                    if (elseNode.Body == "elseif")
                        AnalizeStatement(elseNode, scopeContext);
                    else
                        Analize(elseNode, scopeContext);
                }
        }

        public void Analize(Node node, ScopeContext scopeContext)
        {
            foreach (var subNode in node.SubNodes)
                if (subNode.TokenType == "Statement")
                {
                    AnalizeStatement(subNode, scopeContext);
                }
                else
                {
                    var result = _engine.Executor.ExecuteExpression(subNode, scopeContext);
                }
        }
    }
}