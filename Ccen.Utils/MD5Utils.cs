using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Amazon.Utils
{
    public class MD5Utils
    {

        public static byte[] GetMD5Hash(string text)
        {
            byte[] bytePassword = Encoding.UTF8.GetBytes(text);

            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(bytePassword);
            }
        }

        public static string GetMD5HashAsString(string text)
        {
            var hash = GetMD5Hash(text);
            var result = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
                result.Append(hash[i].ToString("x2"));

            return result.ToString();
        }
    }
}
