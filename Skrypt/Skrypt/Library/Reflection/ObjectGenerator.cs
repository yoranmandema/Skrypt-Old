using System;
using System.Linq;
using System.Reflection;
using Skrypt.Engine;
using Skrypt.Library.Native;

namespace Skrypt.Library.Reflection
{
    internal static class ObjectGenerator
    {
        public static SkryptObject MakeObjectFromClass(Type type, SkryptEngine engine, SkryptObject parent = null)
        {
            var Object = new SkryptObject();
            var isType = false;
            Object.Name = type.Name;

            if (parent != null) Object.Name = parent.Name + "." + Object.Name;

            if (type.IsSubclassOf(typeof(SkryptType))) isType = true;

            SkryptObject Instance = null;

            if (isType) {
                Instance = (SkryptObject)Activator.CreateInstance(type);
            }

            var methods = type.GetMethods().Where(m =>
            {
                if (m.ReturnType != typeof(SkryptObject)) return false;

                if (m.GetParameters().Count() != 2) return false;

                if (m.GetParameters()[0].ParameterType != typeof(SkryptObject)) return false;

                if (m.GetParameters()[1].ParameterType != typeof(SkryptObject[])) return false;

                return m.IsPublic;
            });

            foreach (var m in methods)
            {
                var method = new SharpMethod {
                    Method = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate), m),
                    Name = m.Name
                };

                var property = new SkryptProperty {
                    Name = m.Name,
                    Value = method,
                    Accessibility = Attribute.GetCustomAttribute(m,typeof(PrivateAttribute)) != null ? Access.Private : Access.Public,
                    IsConstant = Attribute.GetCustomAttribute(m, typeof(ConstantAttribute)) != null
                };

                if (Attribute.GetCustomAttribute(m, typeof(InstanceAttribute)) != null) {
                    Instance?.Properties.Add(property);
                }
                else {
                    Object.Properties.Add(property);
                }
            }

            var fields = type.GetFields().Where(f =>
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
                    Accessibility = Attribute.GetCustomAttribute(f, typeof(PrivateAttribute)) != null ? Access.Private : Access.Public,
                    IsConstant = Attribute.GetCustomAttribute(f, typeof(ConstantAttribute)) != null
                };

                if (Attribute.GetCustomAttribute(f, typeof(InstanceAttribute)) != null) {
                    Instance?.Properties.Add(property);
                }
                else {
                    Object.Properties.Add(property);
                }
            }

            var classes = type.GetNestedTypes();

            foreach (var c in classes)
            {
                SkryptObject v;

                v = MakeObjectFromClass(c, engine, Object);

                var property = new SkryptProperty
                {
                    Name = c.Name,
                    Value = v,
                    Accessibility = Attribute.GetCustomAttribute(c, typeof(PrivateAttribute)) != null ? Access.Private : Access.Public,
                    IsConstant = Attribute.GetCustomAttribute(c, typeof(ConstantAttribute)) != null
                };

                if (Attribute.GetCustomAttribute(c, typeof(InstanceAttribute)) != null) {
                    Instance?.Properties.Add(property);
                }
                else {
                    Object.Properties.Add(property);
                }
            }
          
            if (isType) {
                var className = type.ToString();

                Instance.Properties.Add(new SkryptProperty {
                    Name = "TypeName",
                    Value = new Native.System.String(className)
                });

                engine.GlobalScope.AddType(className, Instance.SetPropertiesTo(Object));              
            }

            Console.WriteLine(Object.Name);

            return Object;
        }
    }
}