using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SmartContract
    {
        public string Id { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
