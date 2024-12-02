using System.Security.Cryptography;
using System.Text;

namespace Simple
{
    public class Wallet
    {
        public string PublicKey { get; set; }
        private string PrivateKey { get; set; }
        public decimal Amount { get; set; }

        public Wallet()
        {
            GenerateKeys();
        }

        private void GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            }
        }

        public string SignData(string data)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(PrivateKey), out _);

                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signatureBytes);
            }
        }

        public static bool VerifySignature(string data, string signature, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signatureBytes = Convert.FromBase64String(signature);
                return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
    }
}
