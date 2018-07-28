using System.Linq;
using System;
using Skrypt.Engine;
using Skrypt.Execution;
using System.IO;
using SysFile = System.IO.File;
using System.Text;

namespace Skrypt.Library.Native
{
    partial class System
    {
        [Constant]
        public class File : SkryptType {
            public String Path { get; set; }

            public File() {
            }

            public File (String path) {
                Path = path;
            }

            [Constant]
            public SkryptObject Write(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = (File)self;
                var str = TypeConverter.ToString(values, 0);

                using (StreamWriter sw = new StreamWriter(s.Path)) {
                    sw.Write(str);
                }

                return self;
            }

            [Constant]
            public SkryptObject Append(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = (File)self;
                var str = TypeConverter.ToString(values, 0);

                using (StreamWriter sw = SysFile.AppendText(s.Path)) {
                    sw.Write(str);
                }

                return self;
            }

            [Constant]
            public SkryptObject Read(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = (File)self;
                var str = "";

                using (StreamReader streamReader = new StreamReader(s.Path)) {
                    str = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                return engine.Create<String>(str);
            }
        }

        [Constant, Static]
        public class IO : SkryptObject
        {
            /* Template Function
            [Constant]
            public static SkryptObject Template(SkryptEngine engine, SkryptObject self, SkryptObject[] values)
            {
                var a = TypeConverter.ToArray(values, 0);
                
            }
            */

            [Constant]
            public static SkryptObject Open(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var path = TypeConverter.ToString(values,0);

                var file = engine.Create<File>(path);

                return file;
            }
        }
    }
}