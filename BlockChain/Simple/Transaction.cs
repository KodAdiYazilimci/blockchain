namespace Simple
{
    public class Transaction
    {
        public decimal Amount { get; set; }
        public string TransactionHash { get; set; }

        public string SenderPublicKey { get; set; }
        public string ReceiverPublicKey { get; set; }
        public string Signature { get; set; }

        public Transaction()
        {
            
        }

        public Transaction(string senderPublicKey, string receiverPublicKey, decimal amount, string signature)
        {
            SenderPublicKey = senderPublicKey;
            ReceiverPublicKey = receiverPublicKey;
            Amount = amount;
            Signature = signature;
        }
    }
}
