using System;
using System.Collections.Generic;
using System.Text;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        public static StringBuilder StringBuilder = new StringBuilder();

        public class String : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation("add", typeof(String), typeof(SkryptObject),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return new String(a + b);
                    }),
                new Operation("add", typeof(SkryptObject), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return new String(a + b);
                    }),
                new Operation("index", typeof(String), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return (String) a.Value[(int) b].ToString();
                    }),
                new Operation("equal", typeof(String), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return (Boolean) (a.Value == b.Value);
                    })
            };

            public string Value;

            public String()
            {
                Name = "string";
                CreateCopyOnAssignment = true;
            }

            public String(string v = "")
            {
                Name = "string";
                Value = v;
                CreateCopyOnAssignment = true;
            }

            public static SkryptObject Constructor(SkryptObject self, SkryptObject[] input)
            {
                var a = TypeConverter.ToString(input, 0);

                return new String(a);
            }

            public static implicit operator String(string d)
            {
                return new String(d);
            }

            public static implicit operator string(String d)
            {
                return d.Value;
            }

            public static SkryptObject Char(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);

                return (String) ("" + Convert.ToChar((int) a));
            }

            public static SkryptObject Length(SkryptObject self, SkryptObject[] values)
            {
                var a = (String) self;

                return (Numeric) a.Value.Length;
            }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}