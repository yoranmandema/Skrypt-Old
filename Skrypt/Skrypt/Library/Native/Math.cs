using System;
using SysMath = System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library.SkryptClasses;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public class Math {
            static public Numeric PI = new Numeric(SysMath.PI);
            static public Numeric E = new Numeric(SysMath.E);

            static public SkryptObject Round(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);

                return (Numeric)SysMath.Round(a);
            }

            static public SkryptObject Floor(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);

                return (Numeric)SysMath.Floor(a);
            }

            static public SkryptObject Ceil(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);

                return (Numeric)SysMath.Ceiling(a);
            }

            static public SkryptObject Abs(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);

                return (Numeric)SysMath.Abs(a);
            }

            static public SkryptObject Min(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);
                var b = TypeConverter.ToNumeric(Values, 1);

                return (Numeric)SysMath.Min(a, b);
            }

            static public SkryptObject Max(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);
                var b = TypeConverter.ToNumeric(Values, 1);

                return (Numeric)SysMath.Max(a, b);
            }

            static public SkryptObject Clamp(SkryptObject[] Values) {
                var x = TypeConverter.ToNumeric(Values, 0);
                var a = TypeConverter.ToNumeric(Values, 1);
                var b = TypeConverter.ToNumeric(Values, 2);

                return (Numeric)SysMath.Min(SysMath.Max(x, a), b);
            }
        }
    }
}
