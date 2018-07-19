using System;
using System.Collections.Generic;
using System.Text;
using Skrypt.Engine;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant,Static]
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

            public Numeric Length => this.Value.Length;

            public override bool CreateCopyOnAssignment => true;
            public override string Name => "string";

            public String()
            {
            }

            public String(string v = "")
            {
                Value = v;
            }

            public static implicit operator String(string d)
            {
                return new String(d);
            }

            public static implicit operator string(String d)
            {
                return d.Value;
            }

            [Constant]
            public static SkryptObject Char(SkryptEngine engine, SkryptObject self, SkryptObject[] input)
            {
                return engine.Create<String>("" + Convert.ToChar((int)TypeConverter.ToNumeric(input, 0)));
            }

            [Constant]
            public static SkryptObject Byte(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<Numeric>(Convert.ToByte(TypeConverter.ToString(input, 0).Value[0]));
            }

            [Constant]
            public SkryptObject Explode(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var delimiter = TypeConverter.ToString(input, 0);
                var selfString = ((String)self).Value;

                var exploded = !string.IsNullOrEmpty(delimiter) ? 
                    selfString.Split(new string[] { delimiter }, StringSplitOptions.None) : 
                    new string[] { selfString };

                var array = engine.Create<Array>();

                for (int i = 0; i < exploded.Length; i++) {
                    array.Value.Add(engine.Create<String>(exploded[i]));
                }

                return array;
            }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}