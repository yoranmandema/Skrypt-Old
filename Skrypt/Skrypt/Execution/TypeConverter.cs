using Skrypt.Library;
using Sys = Skrypt.Library.Native.System;
using Skrypt.Engine;

namespace Skrypt.Execution
{
    public static class TypeConverter
    {
        public static Sys.Numeric ToNumeric(SkryptObject[] args, int index)
        {
            if (index > args.Length - 1) return new Sys.Numeric(0);

            return (Sys.Numeric) args[index];
        }

        public static SkryptMethod ToMethod(SkryptObject[] args, int index) {
            if (index > args.Length - 1) return new SkryptMethod();

            return (SkryptMethod)args[index];
        }

        public static UserMethod ToUserMethod(SkryptObject[] args, int index) {
            if (index > args.Length - 1) return new UserMethod();

            return (UserMethod)args[index];
        }

        public static Sys.Boolean ToBoolean(SkryptObject[] args, int index)
        {
            if (index > args.Length - 1) return new Sys.Boolean(false);

            return args[index].ToBoolean();
        }

        public static Sys.String ToString(SkryptObject[] args, int index, SkryptEngine engine)
        {
            if (args == null || index > args.Length - 1) return new Sys.String("");

            return SkryptObject.GetString(args[index], engine);
        }

        public static Sys.Array ToArray(SkryptObject[] args, int index)
        {
            if (index > args.Length - 1) return new Sys.Array();

            return (Sys.Array) args[index];
        }

        public static SkryptObject ToAny(SkryptObject[] args, int index)
        {
            if (index > args.Length - 1) return new Sys.Null();

            return args[index];
        }
    }
}