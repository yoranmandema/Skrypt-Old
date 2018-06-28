using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Library.SkryptClasses;
using System.Reflection;

namespace Skrypt.Library.Methods {
    public partial class StandardMethods {
        SkryptEngine engine; 

        public StandardMethods (SkryptEngine e) {
            engine = e;
        }

        public void AddMethodsToEngine () {
            var Methods = this.GetType().GetMethods().Where((m) => {
                if (!m.IsStatic) {
                    return false;
                }

                return true;
            });

            foreach (MethodInfo M in Methods) {
                SharpMethod Method = new SharpMethod();

                Method.method = (SkryptDelegate) Delegate.CreateDelegate(typeof(SkryptDelegate), M);
                Method.Name = M.Name;

                engine.Methods.Add(Method);
            }
        }
    }
}
