using System;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    public partial class System
    {
        public static SkryptObject print(SkryptObject Self, SkryptObject[] Values)
        {
            var a = TypeConverter.ToAny(Values, 0);

            Console.WriteLine(a);

            return new Void();
        }
    }
}