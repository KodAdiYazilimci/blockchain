using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace Common.Cryptography
{
    public class Sha256Hasher
    {
        public static string ComputeSha256Hash(string rawData)
        {
            // SHA-256 algoritması için bir nesne oluştur
            using (SHA256 sha256 = SHA256.Create())
            {
                // Girdiyi byte dizisine dönüştür
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Byte dizisini hexadecimal string'e dönüştür
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public string CalculateHash(int Index, string PreviousHash, string Timestamp, List<string> Transactions, int Nonce)
        {
            string blockData = Index + PreviousHash + Timestamp + string.Join(",", Transactions) + Nonce;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
