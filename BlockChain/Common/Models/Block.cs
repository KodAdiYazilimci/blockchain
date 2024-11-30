namespace Common.Models
{
    public class Block
    {
        public int Index { get; set; }
        public string PreviousHash { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public List<string> Transactions { get; set; } = new List<string>();
        public string Hash { get; set; } = string.Empty;
        public int Nonce { get; set; }
        public List<SmartContract> SmartContracts { get; set; } = new List<SmartContract>();
    }
}
