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

                Method.method = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate),M);

                Method.Name = M.Name;

                SkryptProperty property = new SkryptProperty {
                    Name = M.Name,
                    Value = Method,
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            var Fields = Class.GetFields().Where((f) => {
                Console.WriteLine(f.FieldType.IsSubclassOf(typeof(SkryptObject)));

                if (!f.FieldType.IsSubclassOf(typeof(SkryptObject))) {
                    return false;
                }

                return f.IsPublic;
            });

            foreach (FieldInfo F in Fields) {
                SkryptProperty property = new SkryptProperty {
                    Name = F.Name,
                    Value = (SkryptObject)F.GetValue(null),
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            var Classes = Class.GetNestedTypes();

            foreach (TypeInfo C in Classes) {
                Console.WriteLine(C);

                SkryptProperty property = new SkryptProperty {
                    Name = C.Name,
                    Value = MakeObjectFromClass(C),
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            return Object;
        }
    }
}
