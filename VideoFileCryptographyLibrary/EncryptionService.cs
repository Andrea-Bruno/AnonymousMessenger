using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NBitcoin;

namespace VideoFileCryptographyLibrary
{
    internal class EncryptionService
    {
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }

        private readonly Key privateKey;
        private readonly PubKey publicKey;

        public EncryptionService()
        {
            privateKey = new Key();
            publicKey = privateKey.PubKey;

            PublicKey = publicKey.ToString();
            PrivateKey = privateKey.GetWif(Network.Main).ToString();
        }

        public EncryptionService(string _privateKey)
        {
            var BCSecret = new BitcoinSecret(_privateKey, Network.Main);

            privateKey = BCSecret.PrivateKey;
            publicKey = BCSecret.PubKey;

            PublicKey = publicKey.ToString();
            PrivateKey = privateKey.GetWif(Network.Main).ToString();
        }

        public EncryptionService(byte[] _privateKey)
        {
            var BCSecret = new Key(_privateKey);

            privateKey = BCSecret;
            publicKey = BCSecret.PubKey;

            PublicKey = publicKey.ToString();
            PrivateKey = privateKey.GetWif(Network.Main).ToString();
        }

        public byte[] Encrypt(byte[] ToEncrypt)
        {
            try
            {
                return publicKey.Encrypt(ToEncrypt);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] Decrypt(byte[] ToDecrypt)
        {
            try
            {
                return privateKey.Decrypt(ToDecrypt);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
