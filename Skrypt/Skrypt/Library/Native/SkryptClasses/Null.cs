using System.Collections.Generic;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
        public class Null : SkryptType
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation(Operators.Not, typeof(Null),
                    input =>
                    {
                        return new Boolean(true);
                    }),
                new Operation(Operators.Equal, typeof(Null), typeof(SkryptObject),
                    input =>
                    {
                        return new Boolean(input[1].GetType() == typeof(Null));
                    }),
                new Operation(Operators.Equal, typeof(SkryptObject), typeof(Null),
                    input =>
                    {
                        return new Boolean(input[0].GetType() == typeof(Null));
                    }),
                new Operation(Operators.NotEqual, typeof(Null), typeof(SkryptObject),
                    input =>
                    {
                        return new Boolean(input[1].GetType() != typeof(Null));
                    }),
                new Operation(Operators.NotEqual, typeof(SkryptObject), typeof(Null),
                    input =>
                    {
                        return new Boolean(input[0].GetType() != typeof(Null));
                    }),
            };

            public Null()
            {
                Name = "null";
            }

            public override Boolean ToBoolean()
            {
                return false;
            }

            public override string ToString()
            {
                return "null";
            }
        }
    }
}