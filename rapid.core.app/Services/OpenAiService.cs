using OpenAI.Chat;
using OpenAI.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace rapid.core.app.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _http;

        public OpenAIService(IConfiguration config)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config["OpenAI:ApiKey"]);
        }

        public async Task<string> ChatAsync(string system, string user)
        {
            var payload = new
            {
                model = "gpt-4.1-mini",
                messages = new[]
                {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
            };

            var json = JsonSerializer.Serialize(payload);
            var res = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var body = await res.Content.ReadAsStringAsync();
            return JsonDocument.Parse(body)
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
    }
}
