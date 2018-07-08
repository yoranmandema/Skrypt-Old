﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public static StringBuilder stringBuilder = new StringBuilder();

        public class String : SkryptType {
            public string value;

            public String() {
                Name = "string";
                CreateCopyOnAssignment = true;
            }

            public String(string v = "") {
                Name = "string";
                value = v;
                CreateCopyOnAssignment = true;
            }

            public static SkryptObject Constructor(SkryptObject Self, SkryptObject[] Input) {
                var a = TypeConverter.ToString(Input, 0);

                return new String(a);
            }

            public static implicit operator String(string d) {
                return new String(d);
            }

            public static implicit operator string(String d) {
                return d.value;
            }

            public new List<Operation> Operations = new List<Operation> {
                new Operation("add",typeof(String),typeof(SkryptObject),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToString(Input,0);
                        var b = TypeConverter.ToString(Input,1);

                        return new String(a + b);
                    }),
                new Operation("add",typeof(SkryptObject),typeof(String),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToString(Input,0);
                        var b = TypeConverter.ToString(Input,1);

                        return new String(a + b);
                    }),
                new Operation("index",typeof(String), typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToString(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        return (String)a.value[(int)b].ToString();
                    }),
                new Operation("equal",typeof(String), typeof(String),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToString(Input,0);
                        var b = TypeConverter.ToString(Input,1);

                        return (Boolean)(a.value == b.value);
                    }),
            };

            public static SkryptObject Char (SkryptObject Self, SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values,0);

                return (String)("" + Convert.ToChar((int)a));
            }

            public static SkryptObject Length(SkryptObject Self, SkryptObject[] Values) {
                var a = (String)Self;

                return (Numeric)a.value.Length;
            }

            public override string ToString() {
                return value;
            }
        }
    }
}