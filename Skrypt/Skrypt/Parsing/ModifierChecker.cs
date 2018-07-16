using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;

namespace Skrypt.Parsing {
    public class ModifierChecker {

        private readonly SkryptEngine _engine;

        public ModifierChecker(SkryptEngine e) {
            _engine = e;
        }

        public void CheckModifiers (Node node) {
            if ((node.Modifiers & Modifier.Const) != 0) {
                if (!(node.Body == "assign" || node.TokenType == "MethodDeclaration" || node.TokenType == "ClassDeclaration")) {
                    _engine.ThrowError("Invalid use of const", node.Token);
                }
            }
        }
    }
}
