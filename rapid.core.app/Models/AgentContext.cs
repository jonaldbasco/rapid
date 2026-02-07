using System.Collections.Generic;

namespace rapid.core.app.Models
{
    public class AgentContext
    {
        public string Goal { get; set; }
        public Dictionary<string, string> Memory { get; set; } = new();
    }
}
