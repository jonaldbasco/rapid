using rapid.core.app.Models;
using System.Text.Json;

namespace rapid.core.app.Services
{
    public class LLMParser
    {
        public List<ParsedCommand> Parse(string llmOutput)
        {
            return JsonSerializer.Deserialize<List<ParsedCommand>>(
                llmOutput,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
