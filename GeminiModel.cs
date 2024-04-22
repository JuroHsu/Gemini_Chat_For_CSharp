using Newtonsoft.Json;

namespace Gemini_Chat_For_CSharp
{
    public class GeminiModel
    {
        public class GeminiDefaultConfig
        {
            public required string ApiKey { get; set; }
            public string Version { get; set; } = "v1";
            public string Model { get; set; } = "gemini-pro:generateContent";
            public int MaxOutputTokens { get; set; } = 1000;
        }

        public class RequestBody
        {
            [JsonProperty("contents")]
            public required MessageContent[] Contents { get; set; }

            [JsonProperty("generationConfig")]
            public required GenerationConfig GenerationConfig { get; set; }
        }

        public class MessageContent
        {
            [JsonProperty("role")]
            [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
            public MessageRole Role { get; set; }

            [JsonProperty("parts")]
            public required Part[] Parts { get; set; }
        }

        public class Part
        {
            [JsonProperty("text")]
            public required string Text { get; set; }
        }

        public enum MessageRole
        {
            User,
            Model
        }

        public class GenerationConfig
        {
            [JsonProperty("maxOutputTokens")]
            public int MaxOutputTokens { get; set; }
        }
    }
}
