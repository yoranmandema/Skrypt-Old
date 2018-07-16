using Sys = System;
using System.Collections.Generic;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
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
                new Operation("power", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric(Sys.Math.Pow(a.Value,b.Value));
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
                new Operation("and", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean((a.Value != 0d) && (b.Value != 0d));
                    }),
                new Operation("or", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Boolean((a.Value != 0d) || (b.Value != 0d));
                    }),
                new Operation("bitshiftl", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric((int)(a.Value) << (int)(b.Value));
                    }),
                new Operation("bitshiftr", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric((int)(a.Value) >> (int)(b.Value));
                    }),
                new Operation("bitshiftrz", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric((double)((uint)Sys.Convert.ToInt32(Sys.Convert.ToDouble(a.Value)) >> Sys.Convert.ToInt32(Sys.Convert.ToDouble(b.Value))));
                    }),              
                new Operation("bitand", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric((int)(a.Value) & (int)(b.Value));
                    }),
                new Operation("bitxor", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric((int)(a.Value) ^ (int)(b.Value));
                    }),
                new Operation("bitor", typeof(Numeric), typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return new Numeric((int)(a.Value) | (int)(b.Value));
                    }),
                new Operation("bitnot", typeof(Numeric),
                    input =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        return new Numeric(~(int)a.Value);
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

            public override bool CreateCopyOnAssignment => true;
            public override string Name => "numeric";

            public Numeric()
            {
            }

            public Numeric(double v = 0)
            {
                Value = v;
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