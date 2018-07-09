using System;
using System.Linq;
using System.Reflection;
using Skrypt.Engine;

namespace Skrypt.Library.Reflection
{
    internal static class ObjectGenerator
    {
        public static SkryptObject MakeObjectFromClass(Type Class, SkryptEngine engine, SkryptObject parent = null)
        {
            var Object = new SkryptObject();
            var isType = false;
            Object.Name = Class.Name;

            if (parent != null) Object.Name = parent.Name + "." + Object.Name;

            if (Class.IsSubclassOf(typeof(SkryptType))) isType = true;

            var methods = Class.GetMethods().Where(m =>
            {
                if (m.ReturnType != typeof(SkryptObject)) return false;

                if (m.GetParameters().Count() != 2) return false;

                if (m.GetParameters()[0].ParameterType != typeof(SkryptObject)) return false;

                if (m.GetParameters()[1].ParameterType != typeof(SkryptObject[])) return false;

                return m.IsPublic;
            });

            foreach (var m in methods)
            {
                var method = new SharpMethod();

                method.Method = (SkryptDelegate) Delegate.CreateDelegate(typeof(SkryptDelegate), m);
                method.Name = m.Name;

                var property = new SkryptProperty
                {
                    Name = m.Name,
                    Value = method,
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            var fields = Class.GetFields().Where(f =>
            {
                if (!f.FieldType.IsSubclassOf(typeof(SkryptObject))) return false;

                return f.IsPublic;
            });

            foreach (var f in fields)
            {
                var property = new SkryptProperty
                {
                    Name = f.Name,
                    Value = (SkryptObject) f.GetValue(null),
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            var classes = Class.GetNestedTypes();

            foreach (TypeInfo c in classes)
            {
                SkryptObject v;

                v = MakeObjectFromClass(c, engine, Object);

                var property = new SkryptProperty
                {
                    Name = c.Name,
                    Value = v,
                    Accessibility = Access.Public
                };

                Object.Properties.Add(property);
            }

            if (isType)
            {
                var instance = Activator.CreateInstance(Class);
                engine.Types[Class.ToString()] = ((SkryptObject) instance).SetPropertiesTo(Object);
            }

            return Object;
        }
    }
}