using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

            if (typeof(SkryptType).IsAssignableFrom(type)) isType = true;

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

                if (property.IsGetter) {
                    property.Name = property.Name.TrimStart('_');
                }

                if (Attribute.GetCustomAttribute(m, typeof(InstanceAttribute)) != null) {
                    Instance?.Properties.Add(property);
                }
                else {
                    Object.Properties.Add(property);
                }
            }

            var fields = type.GetFields().Where(f =>
            {
                if (!typeof(SkryptObject).IsAssignableFrom(f.FieldType)) return false;
                if (!f.IsStatic) return false;

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

            var properties = type.GetProperties().Where(p => typeof(SkryptObject).IsAssignableFrom(p.PropertyType));

            foreach (var p in properties) {
                var getter = p.GetGetMethod();

                if (!getter.IsPublic) continue;

                DynamicMethod dm = new DynamicMethod("GetValue",typeof(SkryptObject), new Type[] { typeof(SkryptObject), typeof(SkryptObject[]) }, typeof(SkryptObject), true);
                ILGenerator lgen = dm.GetILGenerator();

                lgen.Emit(OpCodes.Ldarg_0);
                lgen.Emit(OpCodes.Call, getter);

                if (getter.ReturnType.GetTypeInfo().IsValueType) {
                    lgen.Emit(OpCodes.Box,getter.ReturnType);
                }

                lgen.Emit(OpCodes.Ret);

                var del = dm.CreateDelegate(typeof(SkryptDelegate)) as SkryptDelegate;

                var method = new SharpMethod {
                    Method = del,
                    Name = p.Name
                };

                var property = new SkryptProperty {
                    Name = p.Name,
                    Value = method,
                    Accessibility = Attribute.GetCustomAttribute(p, typeof(PrivateAttribute)) != null ? Access.Private : Access.Public,
                    IsConstant = Attribute.GetCustomAttribute(p, typeof(ConstantAttribute)) != null,
                    IsGetter = true
                };

                Instance?.Properties.Add(property);

                var setter = p.GetSetMethod(false);

                if (setter == null) continue;

                if (!setter.IsPublic) continue;

                dm = new DynamicMethod("SetValue", typeof(void), new Type[] { typeof(SkryptObject), typeof(SkryptObject) }, typeof(SkryptObject), true);
                lgen = dm.GetILGenerator();

                lgen.Emit(OpCodes.Ldarg_0);
                lgen.Emit(OpCodes.Ldarg_1);

                Type parameterType = setter.GetParameters()[0].ParameterType;

                if (parameterType.GetTypeInfo().IsValueType) {
                    lgen.Emit(OpCodes.Unbox_Any, setter.ReturnType);
                }

                lgen.Emit(OpCodes.Call, setter);
                lgen.Emit(OpCodes.Ret);

                var setdel = dm.CreateDelegate(typeof(SkryptSetDelegate)) as SkryptSetDelegate;

                var setMethod = new SetMethod {
                    Method = setdel,
                    Name = p.Name
                };

                property = new SkryptProperty {
                    Name = p.Name,
                    Value = setMethod,
                    Accessibility = Attribute.GetCustomAttribute(p, typeof(PrivateAttribute)) != null ? Access.Private : Access.Public,
                    IsConstant = Attribute.GetCustomAttribute(p, typeof(ConstantAttribute)) != null,
                    IsSetter = true
                };

                Instance?.Properties.Add(property);
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
                    Value = new Native.System.String(className),
                    IsConstant = true
                });

                Object.Properties.Add(new SkryptProperty {
                    Name = "TypeName",
                    Value = new Native.System.String(className),
                    IsConstant = true
                });

                engine.GlobalScope.AddType(className, Instance);              
            }
             
            return Object;
        }
    }
}