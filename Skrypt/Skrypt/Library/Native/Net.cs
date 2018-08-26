using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Execution;
using System.Net;
using System.IO;
using System.Threading;

namespace Skrypt.Library.Native {
    partial class System {
        [Constant, Static]
        public class Net : SkryptObject {
            [Constant, Static]
            public class HTTP : SkryptObject {
                public static SkryptObject Request (SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                    var address = TypeConverter.ToString(values,0,engine);

                    var request = (HttpWebRequest)WebRequest.Create(address);
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.ContentType = "text/plain";
                    request.Timeout = Timeout.Infinite;
                    request.Method = "GET";
                    request.Accept = "text/plain";
                    request.KeepAlive = false;

                    HttpWebResponse response = null;

                    try {
                        response = (HttpWebResponse)request.GetResponse();
                    } catch (WebException e) {
                        return engine.Create<String>(e.Status.ToString());
                    }
                                        
                    // Get the stream containing content returned by the server.  
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.  
                    var reader = new StreamReader(dataStream);
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    Console.WriteLine(responseFromServer);
                    // Clean up the streams and the response.  
                    reader.Close();
                    response.Close();

                    return engine.Create<String>(responseFromServer);
                }
            }
        }
    }
}
