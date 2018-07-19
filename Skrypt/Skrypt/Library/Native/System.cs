using System;
using Skrypt.Execution;
using Skrypt.Engine;

namespace Skrypt.Library.Native
{
    [Constant, Static]
    public partial class System : SkryptObject
    {
        [Constant]
        public static SkryptObject Print(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
        {
            var a = TypeConverter.ToAny(values, 0);

            Console.WriteLine(a);

            return new Null();
        }

        [Constant]
        public static SkryptObject Input(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
            var input = Console.ReadLine();

            return engine.Create<String>(input);
        }
    }
}