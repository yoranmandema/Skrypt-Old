using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Skrypt.Library.Reflection {
    static class ObjectGenerator {
        public static SkryptObject MakeObjectFromClass(Type Class) {
            SkryptObject Object = new SkryptObject();

            var Methods = Class.GetMethods().Where((m) => {
                if (m.ReturnType != typeof(SkryptObject)) {
                    return false;
                }

                return m.IsPublic;
            });

            foreach (MethodInfo M in Methods) {
                SharpMethod Method = new SharpMethod();

                Console.WriteLine(M);

                Method.method = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate),M);

                Method.Name = M.Name;

                SkryptProperty property = new SkryptProperty {
                    Name = M.Name,
                    Value = Method,
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            return Object;
        }
    }
}
