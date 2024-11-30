namespace Simple
{
    public class SmartContract
    {
        public string Id { get; set; }
        public string Creator { get; set; }
        public string Code { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
