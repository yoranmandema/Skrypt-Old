using Skrypt.Engine;
using Skrypt.Execution;
using SysMath = System.Math;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant,Static]
        public class Math : SkryptObject
        {
            [Constant]
            public static Numeric Pi = new Numeric(SysMath.PI);
            [Constant]
            public static Numeric E = new Numeric(SysMath.E);

            [Constant]
            public static SkryptObject Round(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);

                return engine.Create<Numeric>(SysMath.Round(a));
            }

            [Constant]
            public static SkryptObject Floor(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);

                return engine.Create<Numeric>(SysMath.Floor(a));
            }

            [Constant]
            public static SkryptObject Ceil(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);

                return engine.Create<Numeric>(SysMath.Ceiling(a));
            }

            [Constant]
            public static SkryptObject Abs(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);

                return engine.Create<Numeric>(SysMath.Abs(a));
            }

            [Constant]
            public static SkryptObject Sqrt(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);

                return engine.Create<Numeric>(SysMath.Sqrt(a));
            }

            [Constant]
            public static SkryptObject Min(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);
                var b = TypeConverter.ToNumeric(values, 1);

                return engine.Create<Numeric>(SysMath.Min(a, b));
            }

            [Constant]
            public static SkryptObject Max(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);
                var b = TypeConverter.ToNumeric(values, 1);

                return engine.Create<Numeric>(SysMath.Max(a, b));
            }

            [Constant]
            public static SkryptObject Clamp(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var x = TypeConverter.ToNumeric(values, 0);
                var a = TypeConverter.ToNumeric(values, 1);
                var b = TypeConverter.ToNumeric(values, 2);

                return engine.Create<Numeric>(SysMath.Min(SysMath.Max(x, a), b));
            }

            [Constant]
            public static SkryptObject Lerp(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToNumeric(values, 0);
                var b = TypeConverter.ToNumeric(values, 1);
                var t = TypeConverter.ToNumeric(values, 2);

                return engine.Create<Numeric>(a * (1-t) + b * t);
            }

            //Rebuild all from here on down when available, no idea if this works or not because VS thinks I have the wrong .NET version even though they're the same -Octo
            [Constant]
            public static SkryptObject Mod(SkryptEngine engine, SkrypObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToNumeric(values, 0);
                var b = TypeConverter.ToNumeric(values, 1);

                return engine.Create<Numeric>(a % b);
            }
        }
    }
}