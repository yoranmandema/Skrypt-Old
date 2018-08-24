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
            var s = a.ToString();
            var ts = a.GetProperty("ToString");

            if (ts != null && typeof(SkryptMethod).IsAssignableFrom(ts.GetType())) {
                s = ((SkryptMethod)ts).Execute(engine, a, null, engine.CurrentScope).ReturnObject.ToString();
            }

            if ((engine.Settings & EngineSettings.NoLogs) == 0) 
                Console.WriteLine(s);

            return engine.Create<String>(s);
        }

        [Constant]
        public static SkryptObject input(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {

            engine.Stopwatch.Stop();

            var a = TypeConverter.ToAny(values, 0);

            if ((engine.Settings & EngineSettings.NoLogs) == 0)
                Console.WriteLine(a);

            var input = Console.ReadLine();
            engine.Stopwatch.Start();

            return engine.Create<String>(input);
        }
    }
}