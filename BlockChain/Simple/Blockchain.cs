namespace Simple
{
    public class Blockchain
    {
        public List<Block> Blocks { get; set; } = new List<Block>();
        public int Difficulty { get; set; }
        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();
        public List<Wallet> Wallets { get; set; } = new List<Wallet>();
    }
}
