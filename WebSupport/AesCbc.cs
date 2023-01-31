
namespace WebSupport
{
    using System.IO;
    using System.Security.Cryptography;
    static class AesCbc
    {

        public static Aes CurrentAes;

        internal static Aes GenerateKey()
        {
            CurrentAes = Aes.Create();
            return CurrentAes;
        }

        static public byte[] Encrypt(byte[] data, Aes aes = null)
        {
            if (aes == null)
                aes = CurrentAes;
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, encryptor);
            }
        }

        static public byte[] Decrypt(byte[] data, Aes aes = null)
        {
            if (aes == null)
                aes = CurrentAes;
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                return PerformCryptography(data, decryptor);
            }
        }

        static private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }
}