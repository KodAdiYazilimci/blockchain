namespace Simple
{
    public class BitcoinTransaction
    {
        public string SenderAddress { get; set; }
        public string ReceiverAddress { get; set; }
        public decimal Amount { get; set; }
        public string Signature { get; set; }

        public string Serialize()
        {
            return $"{SenderAddress}-{ReceiverAddress}-{Amount}";
        }
    }
}
