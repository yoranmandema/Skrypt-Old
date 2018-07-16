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

                if (m.GetParameters().Count() != 2) return false;

                if (m.GetParameters()[0].ParameterType != typeof(SkryptObject)) return false;

                if (m.GetParameters()[1].ParameterType != typeof(SkryptObject[])) return false;

                return true;
            });

            foreach (var m in methods)
            {
                SkryptDelegate del;

                if (m.IsStatic) {
                    del = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate), m);
                } else {
                    del = (SkryptDelegate)Delegate.CreateDelegate(typeof(SkryptDelegate), Instance, m);
                }

                var method = new SharpMethod {
                    Method = del,
                    Name = m.Name
                };

                var property = new SkryptProperty {
                    Name = m.Name,
                    Value = method,
                    Accessibility = m.IsPublic ? Access.Public : Access.Private,
                    IsConstant = Attribute.GetCustomAttribute(m, typeof(ConstantAttribute)) != null
                };

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
                    Value = (SkryptObject) f.GetValue(Instance),
                    Accessibility = f.IsPublic ? Access.Public : Access.Private,
                    IsConstant = Attribute.GetCustomAttribute(f, typeof(ConstantAttribute)) != null
                };

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
                    Accessibility = getter.IsPublic ? Access.Public : Access.Private,
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
                    Accessibility = setter.IsPublic ? Access.Public : Access.Private,
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
                    Accessibility = Access.Public,
                    IsConstant = Attribute.GetCustomAttribute(c, typeof(ConstantAttribute)) != null
                };

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
                    IsConstant = true
                });

                Object.Properties.Add(new SkryptProperty {
                    Name = "TypeName",
                    Value = new Native.System.String(className),
                    IsConstant = true,
                    IsStatic = true                    
                });

                engine.GlobalScope.AddType(className, Instance);              
            }
             
            return Object;
        }
    }
}