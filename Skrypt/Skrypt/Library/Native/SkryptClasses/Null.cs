using System.Collections.Generic;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        public class Null : SkryptObject
        {
            public new List<Operation> Operations = new List<Operation>
            {
                new Operation("not", typeof(Null),
                    input =>
                    {
                        return new Boolean(true);
                    })
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