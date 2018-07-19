using System;
using Skrypt.Execution;
using Skrypt.Engine;

namespace Skrypt.Library.Native
{
    [Constant, Static]
    public partial class System : SkryptObject
    {
        [Constant]
        public static SkryptObject print(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
        {
            var a = TypeConverter.ToAny(values, 0);

            Console.WriteLine(a);

            return new Null();
        }

        [Constant]
        public static SkryptObject input(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {

            engine.Stopwatch.Stop();
            var input = Console.ReadLine();
            engine.Stopwatch.Start();

            return engine.Create<String>(input);
        }
    }
}