using Simple;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    static void Main(string[] args)
    {
        Blockchain blockchain = new Blockchain();
        blockchain.Difficulty = 3;
        blockchain.Wallets.Add("Sample Creator", 0);
        blockchain.Wallets.Add("Sample Recipent", 0);

        Block block1 = GenerateBlock(blockchain);
        block1.Data = "{}";
        block1.Transactions = new List<string>() { "Init Transaction" };
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
        block1 = MineBlock(blockchain.Difficulty, block1);
        blockchain.Chain.Add(block1);

        blockchain.Difficulty++;

        Block block2 = GenerateBlock(blockchain);
        block2.Data = "{}";
        block2.Transactions = new List<string>() { "Transaction 0" };
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
        block2 = MineBlock(blockchain.Difficulty, block2);
        blockchain.Chain.Add(block2);

        blockchain.Difficulty++;

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

        var hashedTransactions = blockchain.PendingTransactions.Select(x => $"{x.TransactionHash}").ToList();

        string merkleRoot = ComputeMerkleRoot(hashedTransactions);

        var firstTransaction = blockchain.PendingTransactions.FirstOrDefault();

        string targetTransaction = $"{firstTransaction.TransactionHash}";
        List<string> merklePath = GenerateMerklePath(blockchain.PendingTransactions.Select(x => $"{x.TransactionHash}").ToList(), targetTransaction);

        bool isValid = VerifyMerkleProof(targetTransaction, merklePath, merkleRoot);

        blockchain.PendingTransactions.Clear();

        isChainValid = IsChainValid(blockchain);
        Console.WriteLine(isChainValid);
    }

    public static bool VerifyMerkleProof(string transactionHash, List<string> merklePath, string merkleRoot)
    {
        string computedHash = transactionHash;

        foreach (var hash in merklePath)
        {
            computedHash = computedHash.CompareTo(hash) < 0
                ? ComputeSha256Hash(computedHash + hash)
                : ComputeSha256Hash(hash + computedHash);
        }

        return computedHash == merkleRoot;
    }

    public static string ComputeMerkleRoot(List<string> transactionHashes)
    {
        if (transactionHashes == null || transactionHashes.Count == 0)
            throw new ArgumentException("İşlem listesi boş olamaz!");

        // Eğer tek bir işlem varsa, hash'i doğrudan döndür
        if (transactionHashes.Count == 1)
            return transactionHashes[0];

        // Her seviyede çift hash yapmak için işlem sayısını eşit hale getir
        while (transactionHashes.Count % 2 != 0)
        {
            transactionHashes.Add(transactionHashes.Last());
        }

        List<string> newLevel = new List<string>();

        // Mevcut seviyedeki hash'leri birleştirerek yeni bir üst seviye oluştur
        for (int i = 0; i < transactionHashes.Count; i += 2)
        {
            string combinedHash = ComputeSha256Hash(transactionHashes[i] + transactionHashes[i + 1]);
            newLevel.Add(combinedHash);
        }

        // Yeniden hesaplama için üst seviyeyi recursive olarak gönder
        return ComputeMerkleRoot(newLevel);
    }

    public static List<string> GenerateMerklePath(List<string> transactionHashes, string targetHash)
    {
        if (!transactionHashes.Contains(targetHash))
            throw new ArgumentException("Hedef hash işlem listesinde bulunamadı!");

        List<string> merklePath = new List<string>();

        while (transactionHashes.Count > 1)
        {
            while (transactionHashes.Count % 2 != 0)
            {
                transactionHashes.Add(transactionHashes[^1]); // Son hash'i çift yapmak için ekle
            }

            List<string> newLevel = new List<string>();

            for (int i = 0; i < transactionHashes.Count; i += 2)
            {
                if (transactionHashes[i] == targetHash || transactionHashes[i + 1] == targetHash)
                {
                    merklePath.Add(transactionHashes[i] == targetHash ? transactionHashes[i + 1] : transactionHashes[i]);
                    targetHash = ComputeSha256Hash(transactionHashes[i] + transactionHashes[i + 1]);
                }

                string combinedHash = ComputeSha256Hash(transactionHashes[i] + transactionHashes[i + 1]);
                newLevel.Add(combinedHash);
            }

            transactionHashes = newLevel;
        }

        return merklePath;
    }

    static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    private static Block GenerateBlock(Blockchain blockchain)
    {
        Block block1 = new Block();
        block1.Nonce = (blockchain.Chain.LastOrDefault()?.Nonce ?? 0) + 1;
        block1.Index = (blockchain.Chain.LastOrDefault()?.Index ?? -1) + 1;
        block1.PreviousHash = blockchain.Chain.LastOrDefault()?.Hash ?? "0";
        block1.Timestamp = DateTime.UtcNow.ToString();
        block1.Hash = CalculateHash(block1.Index, block1.PreviousHash, block1.Timestamp, block1.Transactions, block1.Nonce);
        return block1;
    }

    static void ProcessPendingTransactions(Blockchain blockchain, string minerAddress)
    {
        foreach (var transaction in blockchain.PendingTransactions)
        {
            blockchain.Wallets[transaction.FromAddress] -= transaction.Amount;

            if (!blockchain.Wallets.ContainsKey(transaction.ToAddress))
            {
                blockchain.Wallets[transaction.ToAddress] = 0;
            }

            blockchain.Wallets[transaction.ToAddress] += transaction.Amount;

            Block newBlock = GenerateBlock(blockchain);
            newBlock.Data = JsonSerializer.Serialize(blockchain.Wallets);
            newBlock = MineBlock(blockchain.Difficulty, newBlock);
            blockchain.Chain.Add(newBlock);

            Console.WriteLine($"İşlem Başarıyla Gerçekleştirildi: {transaction.Amount} Coin {transaction.FromAddress} -> {transaction.ToAddress}");
        }

        blockchain.Wallets[minerAddress] = blockchain.Wallets.ContainsKey(minerAddress) ? blockchain.Wallets[minerAddress] + 0.1m : 0.1m;

        Block minerBlock = GenerateBlock(blockchain);
        minerBlock.Data = JsonSerializer.Serialize(blockchain.Wallets);
        minerBlock = MineBlock(blockchain.Difficulty, minerBlock);
        blockchain.Chain.Add(minerBlock);
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

    static Block MineBlock(int difficulty, Block block)
    {
        string target = new string('0', difficulty);
        while (!block.Hash.StartsWith(target))
        {
            block.Nonce++;
            block.Hash = CalculateHash(block.Index, block.PreviousHash, block.Timestamp, block.Transactions, block.Nonce);
        }
        Console.WriteLine($"Blok kazıldı! Nonce: {block.Nonce}, Hash: {block.Hash}");

        return block;
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

            if (currentBlock.Hash != CalculateHash(currentBlock.Index, currentBlock.PreviousHash, currentBlock.Timestamp, currentBlock.Transactions, currentBlock.Nonce))
                return false;
            if (currentBlock.PreviousHash != previousBlock.Hash)
                return false;
        }
        return true;
    }
}
