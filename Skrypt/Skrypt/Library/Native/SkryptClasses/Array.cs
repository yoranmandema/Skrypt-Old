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
                new Operation(Operators.Add, typeof(Array), typeof(SkryptObject),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToAny(input, 1);

                        var newArray = a.Engine.Create<Array>();
                        newArray.List = new List<SkryptObject>(a.List);
                        newArray.List.Add(b);

                        return newArray;
                    }),
                new Operation(Operators.Add, typeof(SkryptObject), typeof(Array),
                    input =>
                    {
                        var a = TypeConverter.ToAny(input, 0);
                        var b = TypeConverter.ToArray(input, 1);

                        var newArray = a.Engine.Create<Array>();
                        newArray.List = new List<SkryptObject>(b.List);
                        newArray.List.Insert(0, a);

                        return newArray;
                    }),
                new Operation(Operators.Multiply, typeof(Array), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        var mul = (int) b.Value - 1;

                        var newArray = a.Engine.Create<Array>();
                        newArray.List = new List<SkryptObject>(a.List);
                        for (var i = 0; i < mul; i++) {
                            foreach (var obj in a.List) {
                                var copy = ObjectExtensions.Copy(obj);
                                newArray.List.Add(copy);
                            }
                        }

                        return newArray;
                    }),
                new Operation(Operators.Index, typeof(Array), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        if (b < 0 || b > a.List.Count) {
                            return a.Engine.Create<Null>();
                        }
                         
                        return a.List[(int)b];
                    }),
                new Operation(Operators.Index, typeof(Array), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        if (!a.Table.ContainsKey(b)) {
                            return a.Engine.Create<Null>();
                        }

                        return a.Table[b];
                    }),
                new Operation(Operators.IndexSet, typeof(Array), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToAny(input, 1);
                        var c = TypeConverter.ToNumeric(input, 2);

                        if (c > a.List.Count) {
                            var collection = new SkryptObject[(int)c - a.List.Count + 1];

                            for (int i = 0; i < collection.Length; i++) {
                                collection[i] = a.Engine.Create<Null>();
                            }

                            a.List.AddRange(collection);
                        }

                        var newValue = a.List[(int)c] = b;

                        return newValue;
                    }),
                new Operation(Operators.IndexSet, typeof(Array), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToArray(input, 0);
                        var b = TypeConverter.ToAny(input, 1);
                        var c = TypeConverter.ToString(input, 2);

                        var newValue = a.Table[c] = b;

                        return newValue;
                    })
            };

            public List<SkryptObject> List = new List<SkryptObject>();
            public Dictionary<string,SkryptObject> Table = new Dictionary<string, SkryptObject>();

            public Numeric Length => List.Count;

            public override string Name => "array";

            public Array()
            {

            }

            public Array(List<SkryptObject> v) {
                List = v;
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

                ((Array)self).List.Add(a);

                return (Array)self;
            }

            [Constant]
            public SkryptObject Insert(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToAny(values, 0);
                var i = TypeConverter.ToNumeric(values, 1);

                ((Array)self).List.Insert((int)i,a);

                return (Array)self;
            }

            [Constant]
            public SkryptObject Remove(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var i = TypeConverter.ToAny(values, 0);

                if (i.GetType() == typeof(Numeric)) {
                    if ((int)(Numeric)i > 0 && (int)(Numeric)i < ((Array)self).List.Count - 1)
                        ((Array)self).List.RemoveAt((int)(Numeric)i);
                } else if (i.GetType() == typeof(String)) {
                    ((Array)self).Table.Remove((String)i);
                } else {
                    engine.ThrowError("Array index can only be string or numeric.");
                }

                return (Array)self;
            }

            [Constant]
            public SkryptObject Concat(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                return engine.Create<String>(string.Join(string.Empty,((Array)self).List));
            }

            [Constant]
            public SkryptObject Sort(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var m = TypeConverter.ToMethod(values,0);

                if (m.Parameters.Count < 2) engine.ThrowError("Input function must have 2 parameters!"); 

                ((Array)self).List.Sort((x, y) => {
                    var scope = SkryptMethod.GetPopulatedScope(m, new[] { x, y });
                    scope.ParentScope = ScopeContext;

                    var r = m.Execute(engine, self, new[] {x,y}, scope);

                    if (r.ReturnObject.ToBoolean()) {
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

                if (m.GetType() == typeof(SkryptMethod) && m.Parameters.Count != 1) engine.ThrowError("Input function must have 1 parameter!");

                var scope = SkryptMethod.GetPopulatedScope(m, new[] {new Null()});
                scope.ParentScope = engine.CurrentScope;          
                var name = scope.Variables.Keys.First();

                for (int i = 0; i < ((Array)self).List.Count; i++) {
                    var x = ((Array)self).List[i];

                    scope.Variables[name].Value = x;

                    var r = m.Execute(engine, self, new[] { x }, scope);

                    ((Array)self).List[i] = scope.Variables[name].Value;
                }

                var iterator = new Dictionary<string, SkryptObject>(((Array)self).Table);

                foreach (var p in iterator) {
                    var x = p.Value;

                    scope.Variables[name].Value = x;

                    var r = m.Execute(engine, self, new[] { x }, scope);

                    ((Array)self).Table[p.Key] = scope.Variables[name].Value;
                }

                return self;
            }

            [Constant]
            public SkryptObject Map(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var m = TypeConverter.ToMethod(values, 0);

                var newArray = engine.Create<Array>(((Array)self).List);

                if (m.GetType() == typeof(SkryptMethod) && m.Parameters.Count != 1) engine.ThrowError("Input function must have 1 parameter!");

                var scope = SkryptMethod.GetPopulatedScope(m, new[] { new Null() });
                scope.ParentScope = engine.CurrentScope;
                var name = scope.Variables.Keys.First();

                for (int i = 0; i < newArray.List.Count; i++) {
                    var x = newArray.List[i];

                    scope.Variables[name].Value = x;

                    var r = m.Execute(engine, self, new[] { x }, scope);

                    ((Array)self).List[i] = scope.Variables[name].Value;
                }

                var iterator = new Dictionary<string, SkryptObject>(((Array)self).Table);

                foreach (var p in iterator) {
                    var x = p.Value;

                    scope.Variables[name].Value = x;

                    var r = m.Execute(engine, self, new[] { x }, scope);

                    newArray.Table[p.Key] = scope.Variables[name].Value;
                }

                return newArray;
            }

            public override string ToString()
            {
                return "[" + string.Join(",", List) + "]";
            }
        }
    }
}