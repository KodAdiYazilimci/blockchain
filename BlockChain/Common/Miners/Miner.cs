using Common.Cryptography;

using System.Security.Cryptography;
using System.Text;

namespace Common.Miners
{
    public static class Miner
    {

        public static string MineBlock(string data, int difficulty, out int nonce)
        {
            nonce = 0;
            string hash = string.Empty;
            string target = new string('0', difficulty); // Zorluk için hedef (ör. "0000")

            using (SHA256 sha256 = SHA256.Create())
            {
                while (true)
                {
                    // Blok verisini ve nonce'u birleştir
                    string input = data + nonce;

                    // Hash'i hesapla
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                    // Hash, hedef zorluk seviyesine uyuyor mu?
                    if (hash.StartsWith(target))
                    {
                        break; // Zorluk sağlanırsa döngüden çık
                    }

                    nonce++; // Zorluk sağlanmadıysa nonce'u artır ve devam et
                }
            }

            return hash;
        }
    }
}
