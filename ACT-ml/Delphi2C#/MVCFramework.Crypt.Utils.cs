
using System;
using System.Security.Cryptography;
using System.Text;

namespace MVCFramework.Crypt.Utils
{
    public static class CryptUtils
    {
        
        public static byte[] PBKDF2(byte[] password, byte[] salt, int iterationsCount, int keyLengthInBytes, HashAlgorithmName? prfc = null)
        {
            var algorithm = prfc ?? HashAlgorithmName.SHA1;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterationsCount, algorithm))
            {
                return deriveBytes.GetBytes(keyLengthInBytes);
            }
        }

        public static string BytesToHexString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        public static void MVCCryptInit()
        {
            
            // If you need to enforce any checks (such as FIPS compliance), you could do so here.
        }
    }
}
