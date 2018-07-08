using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Skrypt.Engine;

namespace Skrypt.Library.Reflection {
    static class ObjectGenerator {
        public static SkryptObject MakeObjectFromClass(Type Class, SkryptEngine Engine, SkryptObject Parent = null) {
            SkryptObject Object = new SkryptObject();
            bool isType = false;
            Object.Name = Class.Name;

            if (Parent != null) {
                Object.Name = Parent.Name + "." + Object.Name;
            }

            if (Class.IsSubclassOf(typeof(SkryptType))) {
                isType = true;
            }

            var Methods = Class.GetMethods().Where((m) => {
                if (m.ReturnType != typeof(SkryptObject)) {
                    return false;
                }

                if (m.GetParameters().Count() != 2) {
                    return false;
                }

                if (m.GetParameters()[0].ParameterType != typeof(SkryptObject)) {
                    return false;
                }

                if (m.GetParameters()[1].ParameterType != typeof(SkryptObject[])) {
                    return false;
                }

                return m.IsPublic;
            });

            foreach (MethodInfo M in Methods) {
                SharpMethod Method = new SharpMethod();

                Method.method = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate), M);
                Method.Name = M.Name;

                SkryptProperty property = new SkryptProperty {
                    Name = M.Name,
                    Value = Method,
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            var Fields = Class.GetFields().Where((f) => {
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
                SkryptObject v;

                v = MakeObjectFromClass(C, Engine, Object);

                SkryptProperty property = new SkryptProperty {
                    Name = C.Name,
                    Value = v,
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            if (isType) {
                var Instance = Activator.CreateInstance(Class);
                Engine.Types[Class.ToString()] = ((SkryptObject)Instance).SetPropertiesTo(Object);              
            }

            return Object;
        }
    }
}
