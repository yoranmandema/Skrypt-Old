using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public static StringBuilder stringBuilder = new StringBuilder();

        public class String : SkryptObject {
            public string value;

            public String() {
                Name = "string";
            }

            public String(string v) {
                Name = "string";
                value = v;
            }

            public static implicit operator String(string d) {
                return new String(d);
            }

            public static implicit operator string(String d) {
                return d.value;
            }

            public new List<Operation> Operations = new List<Operation> {
                new Operation("add",typeof(String),typeof(SkryptObject),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToString(Input,0);
                        var b = TypeConverter.ToString(Input,1);

                        return new String(a + b);
                    }),
                new Operation("add",typeof(SkryptObject),typeof(String),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToString(Input,0);
                        var b = TypeConverter.ToString(Input,1);

                        return new String(a + b);
                    }),
            };

            static public SkryptObject Length(SkryptObject[] Values) {
                var a = TypeConverter.ToString(Values, 0);

                return (Numeric)a.value.Length;
            }

            public override string ToString() {
                return value;
            }
        }
    }
}
