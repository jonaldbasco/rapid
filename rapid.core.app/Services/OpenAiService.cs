using OpenAI.Chat;
using OpenAI.Responses;

namespace rapid.core.app.Services
{
    public class OpenAIService
    {
        private readonly ChatClient _client;

        public OpenAIService(IConfiguration config)
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

        //Text Request
        public OpenAIResponse Send(string text)
        {
            OpenAIResponse response = _client.CreateResponse(text);
            string result = response.GetOutputText();
            return response;
        }

    }
}
