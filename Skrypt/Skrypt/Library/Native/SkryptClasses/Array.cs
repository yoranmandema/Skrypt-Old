using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public class Array : SkryptType {

            public List<SkryptObject> value {get; set;} = new List<SkryptObject>();
            public int lastIndex = 0;

            public Array() {
                Name = "array";
            }

            static int IndexFromObject (SkryptObject Object) {
                var index = 0;

                if (Object.GetType() == typeof(String)) {
                    index = ((String)Object).value.GetHashCode();
                }
                else {
                    index = Convert.ToInt32((Numeric)Object);
                }

                return index;
            }

            public new List<Operation> Operations = new List<Operation> {
                new Operation("add",typeof(Array),typeof(SkryptObject),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var b = TypeConverter.ToAny(Input,1);

                        var newArray = new Array();
                        newArray.value = new List<SkryptObject>(a.value);
                        newArray.value.Add(b);

                        return newArray;
                    }),
                new Operation("add",typeof(SkryptObject),typeof(Array),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToAny(Input,0);
                        var b = TypeConverter.ToArray(Input,1);

                        var newArray = new Array();
                        newArray.value = new List<SkryptObject>(b.value);
                        newArray.value.Insert(0,a);

                        return newArray;
                    }),
                new Operation("multiply",typeof(Array),typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        var newArray = new Array();
                        for (int i = 0; i < (int)b.value; i++) {
                            newArray.value.AddRange(a.value);
                        }

                        return newArray;
                    }),
                new Operation("index",typeof(Array), typeof(SkryptObject),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var b = TypeConverter.ToAny(Input,1);

                        var index = IndexFromObject(b);

                        return a.value[index];
                    }),
                new Operation("indexset",typeof(Array), typeof(SkryptObject),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var b = TypeConverter.ToAny(Input,1);
                        var c = TypeConverter.ToAny(Input,2);

                        var index = IndexFromObject(c);

                        var newValue = a.value[index] = b;

                        return newValue;
                    }),
            };

            public static SkryptObject Length(SkryptObject Self, SkryptObject[] Values) {
                var a = (Array)Self;

                return (Numeric)a.value.Count;
            }

            public override string ToString() {
                return "[" + string.Join(",",value) + "]";
            }
        }
    }
}
