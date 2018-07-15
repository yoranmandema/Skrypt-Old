using System;
using System.Collections.Generic;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        public class Numeric : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation("add", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric(a + b);
                    }),
                new Operation("subtract", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric(a - b);
                    }),
                new Operation("divide", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric(a / b);
                    }),
                new Operation("multiply", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric(a * b);
                    }),
                new Operation("modulo", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric(a % b);
                    }),
                new Operation("lesser", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean(a < b);
                    }),
                new Operation("greater", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean(a > b);
                    }),
                new Operation("equallesser", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean(a <= b);
                    }),
                new Operation("equalgreater", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean(a >= b);
                    }),
                new Operation("equal", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean(Equals(a.Value,b.Value));
                    }),
                new Operation("postincrement", typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var v = a.Value;
                        a.Value++;
                        return new Numeric(v);
                    }),
                new Operation("postdecrement", typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var v = a.Value;
                        a.Value--;
                        return new Numeric(v);
                    }),
                new Operation("negate", typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        return new Numeric(-a.Value);
                    }),
                new Operation("not", typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        return new Boolean(a.Value == 0);
                    })
            };

            public double Value;

            public Numeric()
            {
                Name = "numeric";
                CreateCopyOnAssignment = true;
            }

            public Numeric(double v = 0)
            {
                Name = "numeric";
                Value = v;
                CreateCopyOnAssignment = true;
            }

            public static SkryptObject Constructor(SkryptObject self, SkryptObject[] input)
            {
                var a = TypeConverter.ToNumeric(input, 0);

                return new Numeric(a);
            }

            public static implicit operator Numeric(double d)
            {
                return new Numeric(d);
            }

            public static implicit operator double(Numeric d)
            {
                return d.Value;
            }

            public override string ToString()
            {
                return "" + Value;
            }

            public override Boolean ToBoolean()
            {
                return Value != 0;
            }
        }
    }
}