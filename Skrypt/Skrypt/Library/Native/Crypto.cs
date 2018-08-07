using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Execution;
using System.Security.Cryptography;
using System.IO;

namespace Skrypt.Library.Native {
    partial class System {
        [Constant, Static]
        public class Crypto : SkryptObject {
            [Constant]
            public static SkryptObject SHA256(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = TypeConverter.ToString(values, 0);

                var byteArray = Sys.Text.Encoding.ASCII.GetBytes(s.Value);
                var hashValue = new SHA256Managed().ComputeHash(byteArray);
                var array = engine.Create<Array>();

                for (int i = 0; i < hashValue.Length; i++) {
                    array.List.Add((Numeric)hashValue[i]);
                }

                return array;
            }

            [Constant]
            public static SkryptObject SHA1(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = TypeConverter.ToString(values, 0);

                var byteArray = Sys.Text.Encoding.ASCII.GetBytes(s.Value);
                var hashValue = new SHA1Managed().ComputeHash(byteArray);
                var array = engine.Create<Array>();

                for (int i = 0; i < hashValue.Length; i++) {
                    array.List.Add((Numeric)hashValue[i]);
                }

                return array;
            }

            public static SkryptObject AESEncrypt(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var toEncrypt = TypeConverter.ToString(values, 0);
                var password = TypeConverter.ToString(values, 1);

                var bytesToBeEncrypted = Sys.Text.Encoding.UTF8.GetBytes(toEncrypt);
                var passwordBytes = Sys.Text.Encoding.UTF8.GetBytes(password);

                // Hash the password with SHA256
                passwordBytes = Sys.Security.Cryptography.SHA256.Create().ComputeHash(passwordBytes);

                byte[] encryptedBytes = null;

                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.
                byte[] saltBytes = new byte[] { 2, 64, 56, 76, 12, 10, 23, 5 };

                using (MemoryStream ms = new MemoryStream()) {
                    using (RijndaelManaged AES = new RijndaelManaged()) {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write)) {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }

                var result = Convert.ToBase64String(encryptedBytes);

                return engine.Create<String>(result);
            }

            public static SkryptObject AESDecrypt(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var toEncrypt = TypeConverter.ToString(values, 0);
                var password = TypeConverter.ToString(values, 1);

                var bytesToBeDecrypted = Convert.FromBase64String(toEncrypt);
                var passwordBytes = Sys.Text.Encoding.UTF8.GetBytes(password);
                // Hash the password with SHA256
                passwordBytes = Sys.Security.Cryptography.SHA256.Create().ComputeHash(passwordBytes);

                byte[] decryptedBytes = null;

                byte[] saltBytes = new byte[] {2,64,56,76,12,10,23,5};

                using (MemoryStream ms = new MemoryStream()) {
                    using (RijndaelManaged AES = new RijndaelManaged()) {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write)) {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }

                var result = Sys.Text.Encoding.UTF8.GetString(decryptedBytes);

                return engine.Create<String>(result);
            }
        }
    }
}
