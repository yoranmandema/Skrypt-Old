using System.Collections.Generic;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        public class Boolean : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation("and", typeof(Boolean), typeof(Boolean),
                    Input =>
                    {
                        var a = TypeConverter.ToBoolean(Input, 0);
                        var b = TypeConverter.ToBoolean(Input, 1);

                        return new Boolean(a && b);
                    }),
                new Operation("or", typeof(Boolean), typeof(Boolean),
                    Input =>
                    {
                        var a = TypeConverter.ToBoolean(Input, 0);
                        var b = TypeConverter.ToBoolean(Input, 1);

                        return new Boolean(a || b);
                    }),
                new Operation("not", typeof(Boolean),
                    Input =>
                    {
                        var a = TypeConverter.ToBoolean(Input, 0);

                        return new Boolean(!a);
                    })
            };

            public bool value;

            public Boolean()
            {
                Name = "boolean";
                CreateCopyOnAssignment = true;
            }

            public Boolean(bool v = false)
            {
                Name = "boolean";
                value = v;
                CreateCopyOnAssignment = true;
            }

            public static SkryptObject Constructor(SkryptObject Self, SkryptObject[] Input)
            {
                var a = TypeConverter.ToBoolean(Input, 0);

                return new Boolean(a);
            }

            public static implicit operator Boolean(bool d)
            {
                return new Boolean(d);
            }

            public static implicit operator bool(Boolean d)
            {
                return d.value;
            }

            public override string ToString()
            {
                return value.ToString();
            }

            public override Boolean ToBoolean()
            {
                return value;
            }
        }
    }
}