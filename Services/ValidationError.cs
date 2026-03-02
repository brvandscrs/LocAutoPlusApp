using System.Text.Json.Serialization;

namespace LocAutoPlusApp.Services
{

    public class ValidationError
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, List<string>>? Errors { get; set; }

        public string GetDetails()
        {
            if (Errors == null) return Message ?? "Erreur inconnue";
            return string.Join("\n", Errors.Values.SelectMany(e => e));
        }
    }
}