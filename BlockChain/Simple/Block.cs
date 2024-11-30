namespace Simple
{
    public class Block
    {
        public int Index { get; set; }
        public string PreviousHash { get; set; }
        public string Timestamp { get; set; } 
        public List<string> Transactions { get; set; }  = new List<string>();
        public string Hash { get; set; }
        public int Nonce { get; set; }
        public List<SmartContract> SmartContracts { get; set; } = new List<SmartContract>();
        public string Data { get; set; }
    }
}
