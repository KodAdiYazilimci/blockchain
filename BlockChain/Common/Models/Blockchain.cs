using System.Transactions;

namespace Common.Models
{
    public class Blockchain
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();
        public int Difficulty { get; set; }
        public Dictionary<string, decimal> Wallets { get; set; } = new Dictionary<string, decimal>();
    }
}
