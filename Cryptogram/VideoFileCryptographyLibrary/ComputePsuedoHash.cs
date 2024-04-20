using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VideoFileCryptographyLibrary
{
    public static class ComputePsuedoHash
    {
        public static byte[] GetHash256(MemoryStream stream)
        {
            return GetHash256(stream.ToArray());
        }

        public static byte[] GetHash256(Stream stream)
        {
            const int _bytes = 32;
            int L = (int)stream.Length;
            byte[] i = new byte[_bytes];
            byte[] l = new byte[_bytes];
            stream.ReadAsync(i, 0, _bytes).Wait();
            stream.Seek(L - _bytes, SeekOrigin.Begin);
            stream.ReadAsync(l, 0, _bytes).Wait();
            stream.Seek(0, SeekOrigin.Begin);
            return GetHash256(i, l, L);
        }

        public static byte[] GetHash256(byte[] _byteArray)
        {
            int _byteArrayLength = _byteArray.Length;
            const int _bytes = 32;
            if (_byteArrayLength >= _bytes)
            {
                byte[] first32Bytes = new byte[_bytes];
                byte[] last32Bytes = new byte[_bytes];
                byte[] lengthBytes = BitConverter.GetBytes(_byteArrayLength);
                Array.Copy(_byteArray, first32Bytes, _bytes);
                Array.Copy(_byteArray, _byteArrayLength - _bytes, last32Bytes, 0, _bytes);
                for (int i = 0; i < _bytes; i++)
                {
                    first32Bytes[i] ^= last32Bytes[i];
                }

                for (int i = 0; i < 4; i++)
                {
                    first32Bytes[i] ^= lengthBytes[i];
                }

                return first32Bytes;
            }
            return null;
        }

        public static byte[] GetHash256(byte[] _initial32Bytes, byte[] _last32Bytes, int fileLength)
        {
            int _byteArrayLength = fileLength;
            const int _bytes = 32;
            if (_byteArrayLength >= _bytes)
            {
                if (_initial32Bytes.Length == _bytes && _last32Bytes.Length == _bytes)
                {
                    byte[] first32Bytes = _initial32Bytes;
                    byte[] last32Bytes = _last32Bytes;
                    byte[] lengthBytes = BitConverter.GetBytes(_byteArrayLength);

                    for (int i = 0; i < _bytes; i++)
                    {
                        first32Bytes[i] ^= last32Bytes[i];
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        first32Bytes[i] ^= lengthBytes[i];
                    }

                    return first32Bytes;
                }
            }
            return null;
        }

        public static string ToHex(byte[] _byteArray)
        {
            var hex = new StringBuilder(_byteArray.Length * 2);
            foreach (var b in _byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public static byte[] HexToByteArray(string _hexString)
        {
            var NumberChars = _hexString.Length;
            var bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(_hexString.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
