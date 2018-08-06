using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Parsing;
using System;

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

        public ScopeContext GetScopeFromIndex(ScopeContext scope, int index)
        {
            foreach (var s in scope.SubScopes) {
                if (index >= s.Start && index <= s.End)
                    return GetScopeFromIndex(s, index);
            }

            return scope;
        }
    }
}