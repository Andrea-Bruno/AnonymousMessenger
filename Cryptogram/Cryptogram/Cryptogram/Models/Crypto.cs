using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Cryptogram.Models
{
	internal static class Crypto
	{
		private static readonly byte[] salt = Encoding.ASCII.GetBytes("Xamarin.iOS Version: 7.0.6.168");

		/// <summary>
		/// Takes the given text string and encrypts it using the given password.
		/// </summary>
		/// <param name="textToEncrypt">Text to encrypt.</param>
		/// <param name="encryptionPassword">Encryption password.</param>
		internal static string Encrypt(string textToEncrypt, string encryptionPassword)
		{
			var algorithm = GetAlgorithm(encryptionPassword);

			//Anything to process?
			if (textToEncrypt == null || textToEncrypt == "") return "";

			byte[] encryptedBytes;
			using (ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
			{
				var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
				encryptedBytes = InMemoryCrypt(bytesToEncrypt, encryptor);
			}
			return Convert.ToBase64String(encryptedBytes);
		}

		/// <summary>
		/// Takes the given encrypted text string and decrypts it using the given password
		/// </summary>
		/// <param name="encryptedText">Encrypted text.</param>
		/// <param name="encryptionPassword">Encryption password.</param>
		internal static string Decrypt(string encryptedText, string encryptionPassword)
		{
			var algorithm = GetAlgorithm(encryptionPassword);

			//Anything to process?
			if (encryptedText == null || encryptedText == "") return "";

			byte[] descryptedBytes;
			using (ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV))
			{
				var encryptedBytes = Convert.FromBase64String(encryptedText);
				descryptedBytes = InMemoryCrypt(encryptedBytes, decryptor);
			}
			return Encoding.UTF8.GetString(descryptedBytes);
		}

		/// <summary>
		/// Performs an in-memory encrypt/decrypt transformation on a byte array.
		/// </summary>
		/// <returns>The memory crypt.</returns>
		/// <param name="data">Data.</param>
		/// <param name="transform">Transform.</param>
		private static byte[] InMemoryCrypt(byte[] data, ICryptoTransform transform)
		{
			var memory = new MemoryStream();
			using (Stream stream = new CryptoStream(memory, transform, CryptoStreamMode.Write))
			{
				stream.Write(data, 0, data.Length);
			}
			return memory.ToArray();
		}

		/// <summary>
		/// Defines a RijndaelManaged algorithm and sets its key and Initialization Vector (IV) 
		/// values based on the encryptionPassword received.
		/// </summary>
		/// <returns>The algorithm.</returns>
		/// <param name="encryptionPassword">Encryption password.</param>
		private static RijndaelManaged GetAlgorithm(string encryptionPassword)
		{
			// Create an encryption key from the encryptionPassword and salt.
			var key = new Rfc2898DeriveBytes(encryptionPassword, salt);

			// Declare that we are going to use the Rijndael algorithm with the key that we've just got.
			var algorithm = new RijndaelManaged();
			var bytesForKey = algorithm.KeySize / 8;
			var bytesForIV = algorithm.BlockSize / 8;
			algorithm.Key = key.GetBytes(bytesForKey);
			algorithm.IV = key.GetBytes(bytesForIV);
			return algorithm;
		}

	}
}
