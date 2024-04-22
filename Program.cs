using Newtonsoft.Json;
using System.Text;
using static Gemini_Chat_For_CSharp.GeminiModel;

internal class Program
{
    private static async Task Main()
    {
        var client = new GeminiClient(new GeminiDefaultConfig()
        {
            ApiKey = "API_KEY", //Apply for an API_KEY here: https://console.cloud.google.com/apis/api/generativelanguage.googleapis.com
            Version = "v1beta", //API version: https://ai.google.dev/gemini-api/docs/api-versions
            Model = "gemini-1.5-pro-latest", // Model list: https://ai.google.dev/gemini-api/docs/function-calling
            MaxOutputTokens = 1000
        });
        var historyChatJsonPath = "HistoryChatJson.json";
        var message = "What does the previous answer plus one equal? If there was no previous answer, start from zero.";

        var response = await client.GenerateText(message, historyChatJsonPath);

        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine(response);
    }

    public class GeminiClient(GeminiDefaultConfig config)
    {
        private readonly GeminiDefaultConfig config = config;
        private readonly HttpClient client = new();

        public async Task<string> GenerateText(string prompt, string? historyJsonPath = null)
        {
            var messageContent = HistoryChat(historyJsonPath);
            messageContent.Add(Message(MessageRole.User, prompt));

            var requestBody = new RequestBody
            {
                Contents = [.. messageContent],
                GenerationConfig = new GenerationConfig { MaxOutputTokens = config.MaxOutputTokens }
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
            string url = $"https://generativelanguage.googleapis.com/{config.Version}/models/{config.Model}:generateContent?key={config.ApiKey}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            dynamic responseJson = JsonConvert.DeserializeObject(content) ?? "";

            string modelResponse = responseJson.candidates[0].content.parts[0].text;
            messageContent.Add(Message(MessageRole.Model, modelResponse));

            File.WriteAllText(historyJsonPath ?? "HistoryChatJson.json", JsonConvert.SerializeObject(messageContent, Formatting.Indented));

            return modelResponse;
        }

        private static List<MessageContent> HistoryChat(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return [];
            try
            {
                string fileContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<MessageContent>>(fileContent) ?? [];
            }
            catch
            {
                return [];
            }
        }
        private MessageContent Message(MessageRole role, string text)
            => new() { Role = role, Parts = [new Part { Text = text }] };
    }
}