using System;
using Sys = System;
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
                new Operation(Operators.Add, typeof(String), typeof(SkryptObject),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return a.Engine.Create<String>(a + b);
                    }),
                new Operation(Operators.Add, typeof(SkryptObject), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return a.Engine.Create<String>(a + b);
                    }),
                new Operation(Operators.Index, typeof(String), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return b.Engine.Create<String>(a.Value[(int) b].ToString());
                    }),
                new Operation(Operators.Equal, typeof(String), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return  new Boolean(a.Value == b.Value);
                    }),
                new Operation(Operators.NotEqual, typeof(String), typeof(String),
                    input =>
                    {
                        var a = TypeConverter.ToString(input, 0);
                        var b = TypeConverter.ToString(input, 1);

                        return  new Boolean(a.Value != b.Value);
                    })
            };

            public string Value = "";

            public Numeric Length => Engine.Create<Numeric>(Value.Length);

            public override bool CreateCopyOnAssignment => true;
            public override string Name => "string";

            public String(){}

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
                return engine.Create<String>("" + Sys.Convert.ToChar((int)TypeConverter.ToNumeric(input, 0)));
            }

            [Constant]
            public static SkryptObject Byte(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<Numeric>(Sys.Convert.ToByte(TypeConverter.ToString(input, 0).Value[0]));
            }

            [Constant]
            public SkryptObject PadLeft(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var a = TypeConverter.ToNumeric(input, 0);
                var s = TypeConverter.ToString(input, 1);
                var newString = engine.Create<String>(((String)self).Value);

                while (newString.Value.Length < a) {
                    newString.Value = s + newString.Value;
                }

                return newString;
            }

            [Constant]
            public SkryptObject PadRight(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var a = TypeConverter.ToNumeric(input, 0);
                var s = TypeConverter.ToString(input, 1);
                var newString = engine.Create<String>(((String)self).Value);

                while (newString.Value.Length < a) {
                    newString.Value += s;
                }

                return newString;
            }

            [Constant]
            public SkryptObject EndsWith(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<Boolean>(((String)self).Value.EndsWith(TypeConverter.ToString(input,0)));
            }

            [Constant]
            public SkryptObject StartsWith(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<Boolean>(((String)self).Value.StartsWith(TypeConverter.ToString(input, 0)));
            }

            [Constant]
            public SkryptObject ToUpper(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(((String)self).Value.ToUpper());
            }

            [Constant]
            public SkryptObject ToLower(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(((String)self).Value.ToLower());
            }

            [Constant]
            public SkryptObject Trim(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(((String)self).Value.Trim());
            }

            [Constant]
            public SkryptObject TrimEnd(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(((String)self).Value.TrimEnd());
            }

            [Constant]
            public SkryptObject TrimStart(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(((String)self).Value.TrimStart());
            }

            [Constant]
            public SkryptObject Reverse(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var s = (String)self;

                char[] charArray = s.Value.ToCharArray();
                Sys.Array.Reverse(charArray);

                return engine.Create<String>(new string(charArray));
            }

            [Constant]
            public SkryptObject Find(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<Numeric>(
                    ((String)self).Value.IndexOf(
                        TypeConverter.ToString(input, 0),
                        (int)TypeConverter.ToNumeric(input, 1)
                        )
                    );
            }

            [Constant]
            public SkryptObject Replace(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(
                        ((String)self).Value.Replace(
                            TypeConverter.ToString(input, 0),
                            TypeConverter.ToString(input, 1)
                        )
                    );
            }

            [Constant]
            public SkryptObject GetBytes(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var byteArray = Sys.Text.Encoding.ASCII.GetBytes(((String)self).Value);
                var array = engine.Create<Array>();
                 
                for (int i = 0; i < byteArray.Length - 1; i++) {
                    array.List.Add((Numeric)byteArray[i]);
                }

                return array;
            }

            [Constant]
            public SkryptObject Explode(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var delimiter = TypeConverter.ToString(input, 0);
                var selfString = ((String)self).Value;

                var exploded = !string.IsNullOrEmpty(delimiter) ? 
                    selfString.Split(new string[] { delimiter }, Sys.StringSplitOptions.None) : 
                    new string[] { selfString };

                var array = engine.Create<Array>();

                for (int i = 0; i < exploded.Length; i++) {
                    array.List.Add(engine.Create<String>(exploded[i]));
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