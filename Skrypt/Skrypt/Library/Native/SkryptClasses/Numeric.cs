using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;
using Skrypt.Engine;

namespace Skrypt.Library.Native {
    partial class System {
        public class Numeric : SkryptType {
            public double value;

            public Numeric() {
                Name = "numeric";
                CreateCopyOnAssignment = true;
            }

            public Numeric(double v = 0){
                Name = "numeric";
                value = v;
                CreateCopyOnAssignment = true;
            }

            public static SkryptObject Constructor(SkryptObject Self, SkryptObject[] Input) {
                var a = TypeConverter.ToNumeric(Input,0);

                return new Numeric(a);
            }

            public new List<Operation> Operations = new List<Operation> {
                new Operation("add",typeof(Numeric),typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        return new Numeric(a + b);
                    }),
                new Operation("subtract",typeof(Numeric),typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        return new Numeric(a - b);
                    }),
                new Operation("lesser",typeof(Numeric),typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        return new Boolean(a < b);
                    }),
                new Operation("greater",typeof(Numeric),typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        return new Boolean(a > b);
                    }),
                new Operation("equal",typeof(Numeric),typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        var b = TypeConverter.ToNumeric(Input,1);

                        return new Boolean(a == b);
                    }),
                new Operation("postincrement",typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        double v = a.value;
                        a.value++;
                        return new Numeric(v);
                    }),
                new Operation("postdecrement",typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        double v = a.value;
                        a.value--;
                        return new Numeric(v);
                    }),
                new Operation("negate",typeof(Numeric),
                    (SkryptObject[] Input) => {
                        var a = TypeConverter.ToNumeric(Input,0);
                        return new Numeric(-a.value);
                    }),
            };

            public static implicit operator Numeric(double d) {
                return new Numeric(d);
            }

            public static implicit operator double(Numeric d) {
                return d.value;
            }

            public override string ToString() {
                return "" + value;
            }

            public override Boolean ToBoolean() {
                return value != 0;
            }
        }
    }
}
