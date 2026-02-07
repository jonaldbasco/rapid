using OpenAI.Chat;

namespace rapid.core.app.Services
{
    public class OpenAiService
    {
        private readonly ChatClient _client;

        public OpenAiService(IConfiguration config)
        {
            _client = new ChatClient(
                model: config["OpenAI:Model"],
                apiKey: config["OpenAI:ApiKey"]);
        }

        public async Task<string> CompleteAsync(string prompt)
        {
            var result = await _client.CompleteChatAsync(
                new ChatMessage[]
                {
                new SystemChatMessage(
                    "You are a task planner. " +
                    "Return ONLY valid JSON: [{ agent, task }]."),
                new UserChatMessage(prompt)
                });

            return result.Value.Content[0].Text;
        }
    }
}
