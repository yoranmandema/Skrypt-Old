using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public class Boolean : SkryptType {
            public bool value;

            public Boolean() {
                Name = "boolean";
            }

            public Boolean(bool v = false) {
                Name = "boolean";
                value = v;
            }

            public static SkryptObject Constructor(SkryptObject Self, SkryptObject[] Input) {
                var a = TypeConverter.ToBoolean(Input, 0);

                return new Boolean(a);
            }

            public static implicit operator Boolean(bool d) {
                return new Boolean(d);
            }

            public static implicit operator bool(Boolean d) {
                return d.value;
            }

            public new List<Operation> Operations = new List<Operation> {
                new Operation("and",typeof(Boolean),typeof(Boolean),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToBoolean(Input,0);
                        var b = TypeConverter.ToBoolean(Input,1);

                        return new Boolean(a && b);
                    }),
                new Operation("or",typeof(Boolean),typeof(Boolean),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToBoolean(Input,0);
                        var b = TypeConverter.ToBoolean(Input,1);

                        return new Boolean(a || b);
                    }),
                new Operation("not",typeof(Boolean),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToBoolean(Input,0);

                        return new Boolean(!a);
                    }),
            };

            public override string ToString() {
                return value.ToString();
            }

            public override Boolean ToBoolean() {
                return value;
            }
        }
    }
}
