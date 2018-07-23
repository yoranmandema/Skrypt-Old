using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Skrypt.Engine;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant, Static]
        public class Statistics : SkryptObject
        {
            [Constant]
            public static SkryptObject Mode(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var v = a.Value;

                var query = v.GroupBy(x => ((Numeric)x).Value)
                    .Select(group => new { Value = group.Key, Count = group.Count() })
                    .OrderByDescending(x => x.Count);

                var item = query.First();

                return engine.Create<Numeric>(item.Value);
            }

            [Constant]
            public static SkryptObject Mean(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var total = 0d;

                for (int i = 0; i < a.Value.Count; i++)
                {
                    total += (Numeric)a.Value[i];
                }

                return engine.Create<Numeric>(total / a.Value.Count);
            }

            [Constant]
            public static SkryptObject Range(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);

                var sorted = a.Sort(engine, a, null);

                double high = (Numeric)((Array)sorted).Value.Last();
                double low = (Numeric)((Array)sorted).Value.First();

                return engine.Create<Numeric>(high - low);
            }

            [Constant]
            public static SkryptObject Sort(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);

                return a.Sort(engine, a, null);
            }

            //Rebuild all from here on down when available, no idea if this works or not because VS thinks I have the wrong .NET version even though they're the same -Octo
            [Constant]
            public static SkryptObject Count(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                return engine.Create<Numeric>(a.Value.Count);
            }

            [Constant]
            public static SkryptObject CountNotEmpty(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var notEmpty = 0;
                for (int i = 0; i < a.Value.Count; i++)
                {
                    if (a.Value[i] == null || a.Value[i] == "")
                    {
                        notEmpty--;
                    }
                    else
                    {
                        notEmpty++;
                    }
                }

                return engine.Create<Numeric>(notEmpty);
            }

            [Constant]
            public static SkryptObject CountEmpty(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var empty = 0;

                for (int i = 0; i < a.Value.Count; i++)
                {
                    if (a.Value[i] == null || a.Value[i] == "")
                    {
                        empty++;
                    }
                    else
                    {
                        empty--;
                    }
                }

                return engine.Create<Numeric>(empty);
            }

            [Constant]
            public static SkryptObject Large(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToArray(values, 0);
                var k = TypeConverter.ToNumeric(input, 1);

                a = Sort(a);

                return engine.Create<Numeric>(a.Value[k]);
            }

            [Constant]
            public static SkryptObject Small(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var k = TypeConverter.ToNumeric(input, 1);

                a = Sort(a);

                return engine.Create<Numeric>(a.Value[Count(a)-k-1]);
            }

            [Constant]
            public static SkryptObject Min(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var b = 9223372036854775807;

                for (int i = 0; i < a.Value.Count; i++) {
                    if (a.Value[i] < b) {
                        b = a.Value[i];
                    }
                }

                return engine.Create<Numeric>(b);
            }

            [Constant]
            public static SkryptObject Max(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var b = -9223372036854775807;
                for (int i = 0; i < a.Value.Count; i++) {
                    if (a.Value[i] > b) {
                        b = a.Value[i];
                    }
                }

                return engine.Create<Numeric>(b);
            }

            [Constant]
            public static SkryptObject MinIndex(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var b = 9223372036854775807;
                var c = 0;

                for (int i = 0; i < a.Value.Count; i++)
                {
                    if (a.Value[i] < b)
                    {
                        b = a.Value[i];
                        c = i;
                    }
                }

                return engine.Create<Numeric>(c);
            }

            [Constant]
            public static SkryptObject MaxIndex(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var b = -9223372036854775807;
                var c = 0;

                for (int i = 0; i < a.Value.Count; i++)
                {
                    if (a.Value[i] > b)
                    {
                        b = a.Value[i];
                        c = i;
                    }
                }

                return engine.Create<Numeric>(c);
            }


            [Constant]
            public SkryptObject Sum(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var b = 0;
                for (int i = 0; i < a.Value.Count; i++) {
                    b += a.Value[i];
                }

                return engine.Create<Numeric>(b);
            }

            //Whole array, no params
            [Constant]
            public SkryptObject Concat(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                var b = "";
                for (int i=0; i<a.Value.Count; i++) {
                    b += a;
                }

                return engine.Create<String>(b);
            }
        }
    }
}