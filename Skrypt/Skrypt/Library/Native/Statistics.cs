using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        [Constant, Static]
        public class Statistics : SkryptObject {
            [Constant]
            public static SkryptObject Mode (SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToArray(values,0);
                var v = a.Value;

                var query = v.GroupBy(x => ((Numeric)x).Value)
                    .Select(group => new { Value = group.Key, Count = group.Count() })
                    .OrderByDescending(x => x.Count);

                var item = query.First();

                return engine.Create<Numeric>(item.Value);
            }

            [Constant]
            public static SkryptObject Mean(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToArray(values, 0);
                var total = 0d;

                for (int i = 0; i < a.Value.Count; i++) {
                    total += (Numeric)a.Value[i];
                }

                return engine.Create<Numeric>(total/a.Value.Count);
            }

            [Constant]
            public static SkryptObject Range(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToArray(values, 0);

                var sorted = a.Sort(engine, a, null);

                double high = (Numeric)((Array)sorted).Value.Last();
                double low = (Numeric)((Array)sorted).Value.First();

                return engine.Create<Numeric>(high - low);
            }

            [Constant]
            public static SkryptObject Sort(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var a = TypeConverter.ToArray(values, 0);

                return a.Sort(engine,a,null);
            }

            //[Constant]
            //public static SkryptObject Count(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
            //    var a = TypeConverter.ToArray(values, 0);



            //    return Array.Length(engine, a, null);
            //}
        }
    }
}
