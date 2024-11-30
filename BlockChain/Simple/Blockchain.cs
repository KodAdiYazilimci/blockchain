using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
    public class Blockchain
    {
        public List<Block> Chain { get; set; } = new List<Block>();
        public int Difficulty { get; set; }
        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();
        public Dictionary<string, decimal> Wallets { get; set; } = new Dictionary<string, decimal>();
    }
}
