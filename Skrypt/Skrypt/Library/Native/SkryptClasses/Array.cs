using System;
using System.Linq;
using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
        public class Array : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation("add", typeof(Array), typeof(SkryptObject),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToAny(input, 1);

                        var newArray = (Array) ObjectExtensions.Copy(a);
                        newArray.Value = new List<SkryptObject>(a.Value);
                        newArray.Value.Add(b);

                        return newArray;
                    }),
                new Operation("add", typeof(SkryptObject), typeof(Array),
                    input =>
                    {
                        var a = TypeConverter.ToAny(input, 0);
                        var b = TypeConverter.ToArray(input, 1);

                        var newArray = new Array();
                        newArray.Value = new List<SkryptObject>(b.Value);
                        newArray.Value.Insert(0, a);

                        return newArray;
                    }),
                new Operation("multiply", typeof(Array), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        var mul = (int) b.Value - 1;

                        var newArray = new Array();
                        newArray.Value = new List<SkryptObject>(a.Value);
                        for (var i = 0; i < mul; i++)
                            foreach (var obj in a.Value)
                                newArray.Value.Add(ObjectExtensions.Copy(obj));

                        return newArray;
                    }),
                new Operation("index", typeof(Array), typeof(SkryptObject),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToAny(input, 1);

                        var index = IndexFromObject(b);

                        return a.Value[index];
                    }),
                new Operation("indexset", typeof(Array), typeof(SkryptObject),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToAny(input, 1);
                        var c = TypeConverter.ToAny(input, 2);

                        var index = IndexFromObject(c);

                        var newValue = a.Value[index] = b;

                        return newValue;
                    })
            };

            public List<SkryptObject> Value = new List<SkryptObject>();

            public Numeric Length => Value.Count;

            public override string Name => "array";

            public Array()
            {

            }

            public Array(List<SkryptObject> v) {
                Value = v;
            }

            private static int IndexFromObject(SkryptObject Object)
            {
                var index = 0;

                if (Object.GetType() == typeof(String))
                    index = ((String) Object).Value.GetHashCode();
                else
                    index = Convert.ToInt32((Numeric) Object);

                return index;
            }

            [Constant]
            public SkryptObject Push(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToAny(values,0);

                ((Array)self).Value.Add(a);

                return new Null();
            }

            [Constant]
            public SkryptObject Concat(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                return engine.Create<String>(string.Join(string.Empty,((Array)self).Value));
            }

            [Constant]
            public SkryptObject Sort(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var m = TypeConverter.ToMethod(values,0);

                if (m.Parameters.Count < 2) engine.ThrowError("Input function must have 2 parameters!"); 

                ((Array)self).Value.Sort((x, y) => {
                    var scope = SkryptMethod.GetPopulatedScope(m, new[] { x, y });
                    scope.ParentScope = ScopeContext;

                    var r = m.Execute(engine, self, new[] {x,y}, scope);

                    if (r.SubContext.ReturnObject.ToBoolean()) {
                        return 1;
                    } else {
                        return -1;
                    }
                });

                return self;
            }

            [Constant]
            public SkryptObject ForEach(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var m = TypeConverter.ToMethod(values, 0);

                if (m.Parameters.Count != 1) engine.ThrowError("Input function must have 1 parameter!");

                var scope = SkryptMethod.GetPopulatedScope(m);
                scope.ParentScope = engine.CurrentScope;          
                var name = scope.Variables.Keys.First();

                for (int i = 0; i < ((Array)self).Value.Count; i++) {
                    var x = ((Array)self).Value[i];

                    scope.Variables[name].Value = x;

                    var r = m.Execute(engine, self, new[] { x }, scope);

                    x = scope.Variables[name].Value;
                }

                return self;
            }

            [Constant]
            public SkryptObject Map(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var m = TypeConverter.ToMethod(values, 0);

                if (m.Parameters.Count != 1) engine.ThrowError("Input function must have 1 parameter!");

                var newArray = engine.Create<Array>(((Array)self).Value);

                var scope = SkryptMethod.GetPopulatedScope(m);
                scope.ParentScope = engine.CurrentScope;
                var name = scope.Variables.Keys.First();

                for (int i = 0; i < newArray.Value.Count; i++) {
                    var x = newArray.Value[i];

                    scope.Variables[name].Value = x;

                    var r = m.Execute(engine, self, new[] { x }, scope);

                    x = scope.Variables[name].Value;
                }

                return newArray;
            }

            public override string ToString()
            {
                return "[" + string.Join(",", Value) + "]";
            }
        }
    }
}