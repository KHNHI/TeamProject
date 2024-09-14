using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quanlychitieu;

internal class DataSecurity
{
    private static readonly string key = "your-encryption-key"; // Khóa bí mật

    public static string Encrypt(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16];

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    public static string Decrypt(string encryptedText)
    {
        byte[] cipherBytes = Convert.FromBase64String(encryptedText);
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16];

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                using (var ms = new MemoryStream(cipherBytes))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
