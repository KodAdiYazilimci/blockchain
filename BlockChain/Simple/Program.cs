using Simple;

using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Blockchain blockchain = new Blockchain();
        blockchain.Difficulty = 3;
        blockchain.Wallets.Add("Sample Creator", 0);
        blockchain.Wallets.Add("Sample Recipent", 0);

        Block block1 = new Block();
        block1.Nonce = 1;
        block1.Index = (blockchain.Chain.LastOrDefault()?.Index ?? -1) + 1;
        block1.PreviousHash = blockchain.Chain.LastOrDefault()?.Hash ?? "0";
        block1.Transactions = new List<string>() { "Init Transaction" };
        block1.Timestamp = DateTime.UtcNow.ToString();
        block1.Hash = CalculateHash(block1.Index, block1.PreviousHash, block1.Timestamp, block1.Transactions, block1.Nonce);
        block1.SmartContracts.Add(new SmartContract()
        {
            Id = Guid.NewGuid().ToString(),
            Creator = "Sample Creator",
            Code = "if (balance >= Amount) { transfer(Recipient, Amount); }",
            Parameters = new Dictionary<string, object>()
            {
                { "Recipent", "Sample Recipent" },
                { "Amount", 50 }
            }
        });
        MineBlock(blockchain.Difficulty, block1);
        blockchain.Chain.Add(block1);

        Block block2 = new Block();
        block2.Nonce = (blockchain.Chain.LastOrDefault()?.Nonce ?? 0) + 1;
        block2.Index = (blockchain.Chain.LastOrDefault()?.Index ?? -1) + 1;
        block2.PreviousHash = blockchain.Chain.LastOrDefault()?.Hash ?? "0";
        block2.Timestamp = DateTime.UtcNow.ToString();
        block2.Transactions = new List<string>() { "Transaction 0" };
        block2.Hash = CalculateHash(block2.Index, block2.PreviousHash, block2.Timestamp, block2.Transactions, block2.Nonce);
        block2.SmartContracts.Add(new SmartContract()
        {
            Id = Guid.NewGuid().ToString(),
            Creator = "Sample Creator",
            Code = "if (balance >= Amount) { transfer(Recipient, Amount); }",
            Parameters = new Dictionary<string, object>()
            {
                { "Recipent", "Sample Recipent" },
                { "Amount", 100 }
            }
        });
        MineBlock(blockchain.Difficulty + 1, block2);
        blockchain.Chain.Add(block2);

        blockchain.PendingTransactions.Add(new Simple.Transaction()
        {
            FromAddress = "Sample Creator",
            ToAddress = "Sample Recipent",
            Amount = 50,
            TransactionHash = GenerateTransactionHash("Sample Creator", "Sample Recipent", 50)
        });
        blockchain.PendingTransactions.Add(new Simple.Transaction()
        {
            FromAddress = "Sample Creator",
            ToAddress = "Sample Recipent",
            Amount = 100,
            TransactionHash = GenerateTransactionHash("Sample Creator", "Sample Recipent", 100)
        });

        bool isChainValid = IsChainValid(blockchain);

        Console.WriteLine(isChainValid);

        ProcessPendingTransactions(blockchain, "Miner");


        isChainValid = IsChainValid(blockchain);

        Console.WriteLine(isChainValid);
    }

    static void ProcessPendingTransactions(Blockchain blockchain, string minerAddress)
    {
        foreach (var transaction in blockchain.PendingTransactions)
        {
            // Gönderenin bakiyesi güncelleniyor
            blockchain.Wallets[transaction.FromAddress] -= transaction.Amount;
            // Alıcının bakiyesi güncelleniyor
            if (!blockchain.Wallets.ContainsKey(transaction.ToAddress))
            {
                blockchain.Wallets[transaction.ToAddress] = 0;
            }
            blockchain.Wallets[transaction.ToAddress] += transaction.Amount;

            Console.WriteLine($"İşlem Başarıyla Gerçekleştirildi: {transaction.Amount} Coin {transaction.FromAddress} -> {transaction.ToAddress}");
        }

        // İşlem tamamlandıktan sonra, yeni blok yaratılır ve zincire eklenir
        Block newBlock = new Block();
        newBlock.Nonce = (blockchain.Chain.LastOrDefault()?.Nonce ?? -1) + 1;
        newBlock.Index = (blockchain.Chain.LastOrDefault()?.Index ?? 0) + 1;
        newBlock.Timestamp = DateTime.UtcNow.ToString();
        newBlock.Data = "Yeni Blok";
        newBlock.PreviousHash = blockchain.Chain.LastOrDefault()?.Hash ?? "0";
        newBlock.Hash = CalculateHash(newBlock.Index, newBlock.PreviousHash, newBlock.Timestamp, new List<string>(), newBlock.Nonce);

        MineBlock(blockchain.Difficulty, newBlock);

        blockchain.Chain.Add(newBlock);

        // Yeni blok madencisi ödülü eklenir (genellikle 0.1 BTC)
        blockchain.Wallets[minerAddress] = blockchain.Wallets.ContainsKey(minerAddress) ? blockchain.Wallets[minerAddress] + 0.1m : 0.1m;

        // İşlem listesi sıfırlanır
        blockchain.PendingTransactions.Clear();
    }

    static string GenerateTransactionHash(string FromAddress, string ToAddress, decimal Amount)
    {
        string transactionData = FromAddress + ToAddress + Amount.ToString();
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(transactionData));
            StringBuilder sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    static void MineBlock(int difficulty, Block block)
    {
        string target = new string('0', difficulty);
        while (!block.Hash.StartsWith(target))
        {
            block.Nonce++;
            block.Hash = CalculateHash(block.Index, block.PreviousHash, block.Timestamp, block.Transactions, block.Nonce);
        }
        Console.WriteLine($"Blok kazıldı! Nonce: {block.Nonce}, Hash: {block.Hash}");
    }

    static string CalculateHash(int Index, string PreviousHash, string Timestamp, List<string> Transactions, int Nonce)
    {
        string blockData = Index + PreviousHash + Timestamp + string.Join(",", Transactions) + Nonce;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    static bool IsChainValid(Blockchain blockchain)
    {
        for (int i = 1; i < blockchain.Chain.Count; i++)
        {
            Block currentBlock = blockchain.Chain[i];
            Block previousBlock = blockchain.Chain[i - 1];

            // Mevcut bloğun hash'i doğru mu?
            //if (currentBlock.Hash != currentBlock.CalculateHash())

            if (currentBlock.Hash != CalculateHash(currentBlock.Index, currentBlock.PreviousHash, currentBlock.Timestamp, currentBlock.Transactions, currentBlock.Nonce))
                return false;

            // Önceki bloğun hash'i doğru mu?
            if (currentBlock.PreviousHash != previousBlock.Hash)
                return false;
        }
        return true;
    }
}
