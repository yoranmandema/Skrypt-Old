using System.Collections.Generic;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
        public class Boolean : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation(Operators.And, typeof(Boolean), typeof(Boolean),
                    input =>
                    {
                        var a = TypeConverter.ToBoolean(input, 0);
                        var b = TypeConverter.ToBoolean(input, 1);

                        return new Boolean(a && b);
                    }),
                new Operation(Operators.Or, typeof(Boolean), typeof(Boolean),
                    input =>
                    {
                        var a = TypeConverter.ToBoolean(input, 0);
                        var b = TypeConverter.ToBoolean(input, 1);

                        return new Boolean(a || b);
                    }),
                new Operation(Operators.Not, typeof(Boolean),
                    input =>
                    {
                        var a = TypeConverter.ToBoolean(input, 0);

                        return new Boolean(!a);
                    }),
                new Operation(Operators.Equal, typeof(Boolean), typeof(Boolean),
                    input =>
                    {
                        var a = TypeConverter.ToBoolean(input, 0);
                        var b = TypeConverter.ToBoolean(input, 0);

                        return new Boolean((bool)a == (bool)b);
                    }),
                new Operation(Operators.NotEqual, typeof(Boolean), typeof(Boolean),
                    input =>
                    {
                        var a = TypeConverter.ToBoolean(input, 0);
                        var b = TypeConverter.ToBoolean(input, 0);

                        return new Boolean((bool)a != (bool)b);
                    })
            };

            public bool Value;

            public override bool CreateCopyOnAssignment => true;
            public override string Name => "boolean";

            public Boolean()
            {
            }

            public Boolean(bool v = false)
            {
                Value = v;
            }

            public static implicit operator Boolean(bool d)
            {
                return new Boolean(d);
            }

            public static implicit operator bool(Boolean d)
            {
                return d.Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }

            public override Boolean ToBoolean()
            {
                return Value;
            }
        }
    }
}