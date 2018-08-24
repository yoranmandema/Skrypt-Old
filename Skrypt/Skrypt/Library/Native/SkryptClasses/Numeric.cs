using Sys = System;
using System.Collections.Generic;
using Skrypt.Execution;
using Skrypt.Engine;
using Skrypt.Parsing;
using System;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
        public class Numeric : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation(Operators.Add, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>(a + b);
                    }),
                new Operation(Operators.Subtract, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>(a - b);
                    }),
                new Operation(Operators.Divide, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>(a / b);
                    }),
                new Operation(Operators.Multiply, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>(a * b);
                    }),
                new Operation(Operators.Modulo, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>(a % b);
                    }),
                new Operation(Operators.Power, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>(Sys.Math.Pow(a.Value,b.Value));
                    }),
                new Operation(Operators.Lesser, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>(a < b);
                    }),
                new Operation(Operators.Greater, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>(a > b);
                    }),
                new Operation(Operators.EqualLesser, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>(a <= b);
                    }),
                new Operation(Operators.EqualGreater, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>(a >= b);
                    }),
                new Operation(Operators.Equal, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>(Equals(a.Value,b.Value));
                    }),
                new Operation(Operators.NotEqual, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>(!Equals(a.Value,b.Value));
                    }),
                new Operation(Operators.And, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>((a.Value != 0d) && (b.Value != 0d));
                    }),
                new Operation(Operators.Or, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Boolean>((a.Value != 0d) || (b.Value != 0d));
                    }),
                new Operation(Operators.BitShiftL, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>((int)(a.Value) << (int)(b.Value));
                    }),
                new Operation(Operators.BitShiftR, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>((int)(a.Value) >> (int)(b.Value));
                    }),
                new Operation(Operators.BitShiftRZ, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>((double)((uint)Sys.Convert.ToInt32(Sys.Convert.ToDouble(a.Value)) >> Sys.Convert.ToInt32(Sys.Convert.ToDouble(b.Value))));
                    }),              
                new Operation(Operators.BitAnd, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>((int)(a.Value) & (int)(b.Value));
                    }),
                new Operation(Operators.BitXOr, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>((int)(a.Value) ^ (int)(b.Value));
                    }),
                new Operation(Operators.BitOr, typeof(Numeric), typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var b = TypeConverter.ToNumeric(input, 1);

                        return engine.Create<Numeric>((int)(a.Value) | (int)(b.Value));
                    }),
                new Operation(Operators.BitNot, typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        return engine.Create<Numeric>(~(int)a.Value);
                    }),
                new Operation(Operators.PostIncrement, typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var v = a.Value;
                        a.Value++;
                        return engine.Create<Numeric>(v);
                    }),
                new Operation(Operators.PostDecrement, typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        var v = a.Value;
                        a.Value--;
                        return engine.Create<Numeric>(v);
                    }),
                new Operation(Operators.Negate, typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        return engine.Create<Numeric>(-a.Value);
                    }),
                new Operation(Operators.Not, typeof(Numeric),
                    (input, engine) =>
                    {
                        var a = TypeConverter.ToNumeric(input, 0);
                        return engine.Create<Boolean>(a.Value == 0);
                    })
            };

            public double Value;

            public override bool CreateCopyOnAssignment => true;
            public override string Name => "numeric";

            public Numeric()
            {
            }

            public Numeric(double v)
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

            [Constant]
            public static SkryptObject Parse(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var s = TypeConverter.ToString(input,0);

                var p = double.TryParse(s, out double r);

                if (!p) throw new SkryptException("Invalid input format!");

                return engine.Create<Numeric>(r);
            }


            [Constant]
            public SkryptObject ToString(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                return engine.Create<String>(((Numeric)self).Value.ToString(TypeConverter.ToString(input, 0)));
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