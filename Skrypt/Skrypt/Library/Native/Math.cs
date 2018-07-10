using Skrypt.Execution;
using SysMath = System.Math;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant]
        public class Math
        {
            [Constant]
            public static Numeric Pi = new Numeric(SysMath.PI);
            [Constant]
            public static Numeric E = new Numeric(SysMath.E);

            [Constant]
            public static SkryptObject Round(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);

                return (Numeric) SysMath.Round(a);
            }

            [Constant]
            public static SkryptObject Floor(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);

                return (Numeric) SysMath.Floor(a);
            }

            [Constant]
            public static SkryptObject Ceil(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);

                return (Numeric) SysMath.Ceiling(a);
            }

            [Constant]
            public static SkryptObject Abs(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);

                return (Numeric) SysMath.Abs(a);
            }

            [Constant]
            public static SkryptObject Min(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);
                var b = TypeConverter.ToNumeric(values, 1);

                return (Numeric) SysMath.Min(a, b);
            }

            [Constant]
            public static SkryptObject Max(SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);
                var b = TypeConverter.ToNumeric(values, 1);

                return (Numeric) SysMath.Max(a, b);
            }

            [Constant]
            public static SkryptObject Clamp(SkryptObject self, SkryptObject[] values)
            {
                var x = TypeConverter.ToNumeric(values, 0);
                var a = TypeConverter.ToNumeric(values, 1);
                var b = TypeConverter.ToNumeric(values, 2);

                return (Numeric) SysMath.Min(SysMath.Max(x, a), b);
            }
        }
    }
}