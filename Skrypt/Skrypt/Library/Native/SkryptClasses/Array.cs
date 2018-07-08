using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public class Array : SkryptType {

            public Dictionary<string, SkryptObject> value {get; set;} = new Dictionary<string,SkryptObject>();

            public Array() {
                Name = "array";
            }

            public new List<Operation> Operations = new List<Operation> {
                //new Operation("add",typeof(Array),typeof(SkryptObject),
                //    (SkryptObject[] Input) => {
                //        var a = TypeConverter.ToArray(Input,0);
                //        var b = TypeConverter.ToAny(Input,1);

                //        var newArray = new Array();
                //        newArray.value = new List<SkryptObject>(a.value);
                //        newArray.value.Add(b);

                //        return newArray;
                //    }),
                //new Operation("add",typeof(SkryptObject),typeof(Array),
                //    (SkryptObject[] Input) => {
                //        var a = TypeConverter.ToAny(Input,0);
                //        var b = TypeConverter.ToArray(Input,1);

                //        var newArray = new Array();
                //        newArray.value = new List<SkryptObject>(b.value);
                //        newArray.value.Insert(0,a);

                //        return newArray;
                //    }),
                new Operation("index",typeof(Array), typeof(String),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var index = TypeConverter.ToString(Input,1);

                        if (!a.value.ContainsKey(index)) {
                            return new Null();
                        }

                        return a.value[index];
                    }),
                new Operation("index",typeof(Array), typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var index = TypeConverter.ToString(Input,1);

                        if (!a.value.ContainsKey(index)) {
                            return new Null();
                        }

                        return a.value[index];
                    }),
                new Operation("indexset",typeof(Array), typeof(String),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var value = TypeConverter.ToAny(Input,1);
                        var index = TypeConverter.ToString(Input,2);

                        var newValue = a.value[index] = value;

                        return newValue;
                    }),
                new Operation("indexset",typeof(Array), typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToArray(Input,0);
                        var value = TypeConverter.ToAny(Input,1);
                        var index = TypeConverter.ToString(Input,2);

                        var newValue = a.value[index] = value;

                        return newValue;
                    }),
            };

            public override string ToString() {
                stringBuilder.Clear();
                stringBuilder.Append("{\n");

                foreach (KeyValuePair<string,SkryptObject> v in value) {
                    stringBuilder.Append("\t" + v.Key + ": " + v.Value + ",\n");
                }

                stringBuilder.Append("}");

                return stringBuilder.ToString();
            }
        }
    }
}
