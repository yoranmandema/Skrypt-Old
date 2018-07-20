using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Skrypt.Engine;
using Skrypt.Library.Native;

namespace Skrypt.Library.Reflection
{
    internal static class ObjectGenerator
    {
        public static SkryptObject MakeObjectFromClass(Type type, SkryptEngine engine, SkryptObject parent = null) {
            var Object = new SkryptObject();
            var isType = false;
            Object.Name = type.Name;

            if (parent != null) Object.Name = parent.Name + "." + Object.Name;

            if (typeof(SkryptType).IsAssignableFrom(type)) isType = true;
            SkryptObject Instance = null;

            //if (isType) {
                Instance = (SkryptObject)Activator.CreateInstance(type);
            //}

            var methods = type.GetMethods().Where(m =>
            {
                if (m.ReturnType != typeof(SkryptObject)) return false;

                if (m.GetParameters().Count() != 3) return false;

                if (m.GetParameters()[0].ParameterType != typeof(SkryptEngine)) return false;

                if (m.GetParameters()[1].ParameterType != typeof(SkryptObject)) return false;

                if (m.GetParameters()[2].ParameterType != typeof(SkryptObject[])) return false;

                return true;
            });

            foreach (var m in methods)
            {
                SkryptDelegate del;
                var parameters = new List<string>();

                if (m.IsStatic) {
                    del = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate), m);
                } else {
                    del = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate), Instance, m);
                }

                foreach (var p in m.GetParameters()) {
                    parameters.Add(p.Name);
                }

                var method = new SharpMethod {
                    Method = del,
                    Name = m.Name,
                    Parameters = parameters
                };

                var property = new SkryptProperty {
                    Name = m.Name,
                    Value = method
                };

                if (Attribute.GetCustomAttribute(m, typeof(ConstantAttribute)) != null) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Const;
                }

                if (m.IsPublic) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Public;
                } else {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Private;
                }

                if (property.IsGetter) {
                    property.Name = property.Name.TrimStart('_');
                }

                if (!m.IsStatic) {
                    Instance?.Properties.Add(property);
                }
                else {
                    Object.Properties.Add(property);
                }
            }

            var fields = type.GetFields().Where(f => typeof(SkryptObject).IsAssignableFrom(f.FieldType));

            foreach (var f in fields)
            {
                var property = new SkryptProperty
                {
                    Name = f.Name,
                    Value = (SkryptObject) f.GetValue(Instance)
                };

                if (Attribute.GetCustomAttribute(f, typeof(ConstantAttribute)) != null) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Const;
                }

                if (f.IsPublic) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Public;
                }
                else {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Private;
                }

                if (!f.IsStatic) {
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

                DynamicMethod dm = new DynamicMethod("GetValue",typeof(SkryptObject), new Type[] { typeof(SkryptObject), typeof(SkryptObject[]) }, typeof(SkryptObject), false);
                ILGenerator lgen = dm.GetILGenerator();

                lgen.Emit(OpCodes.Ldarg_0);
                lgen.Emit(OpCodes.Call, getter);

                if (getter.ReturnType.GetTypeInfo().IsValueType) {
                    lgen.Emit(OpCodes.Box,getter.ReturnType);
                }

                lgen.Emit(OpCodes.Ret);

                var del = dm.CreateDelegate(typeof(SkryptGetDelegate)) as SkryptGetDelegate;

                var method = new GetMethod {
                    Method = del,
                    Name = p.Name
                };

                var property = new SkryptProperty {
                    Name = p.Name,
                    Value = method,
                    IsGetter = true
                };

                if (Attribute.GetCustomAttribute(p, typeof(ConstantAttribute)) != null) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Const;
                }

                if (getter.IsPublic) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Public;
                }
                else {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Private;
                }

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
                    IsSetter = true
                };

                if (Attribute.GetCustomAttribute(p, typeof(ConstantAttribute)) != null) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Const;
                }

                if (setter.IsPublic) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Public;
                }
                else {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Private;
                }

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
                };

                if (Attribute.GetCustomAttribute(c, typeof(ConstantAttribute)) != null) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Const;
                }

                if (c.IsPublic) {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Private;
                }
                else {
                    property.Modifiers = property.Modifiers | Parsing.Modifier.Public;
                }

                if (Attribute.GetCustomAttribute(c, typeof(StaticAttribute)) == null) {
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
                    Modifiers = Parsing.Modifier.Const
                });

                Object.Properties.Add(new SkryptProperty {
                    Name = "TypeName",
                    Value = new Native.System.String(className),
                    Modifiers = Parsing.Modifier.Const | Parsing.Modifier.Static        
                });

                engine.GlobalScope.AddType(className, Instance);              
            }
             
            return Object;
        }
    }
}