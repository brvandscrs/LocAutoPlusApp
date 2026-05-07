using LocAutoPlusApp.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace LocAutoPlusApp.Services
{
    //internal class ApiService
    //{
    //}

    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "http://127.0.0.1:8000/api";

        public ApiService()
        {
            _http = new HttpClient();
        }

        // Connexion employé → retourne le token et les infos
        public async Task<LoginResult?> LoginAsync(string email, string password)
        {
            var payload = new { email, password };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync($"{BaseUrl}/employes/login", content);
            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LoginResult>(json);
        }

        // Clôture d'un contrat
        public async Task<ApiResponse?> CloturerContratAsync(int contratId, int kmRetour, string dateFinReelle)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AppSession.Token);

            var payload = new { km_retour = kmRetour, date_fin_reelle = dateFinReelle };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync($"{BaseUrl}/contrats/{contratId}/cloturer", content);
            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse>(json);
        }

        // Vérification statut club d'un client
        public async Task<ClubResult?> VerifierClubAsync(int userId)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AppSession.Token);

            var response = await _http.GetAsync($"{BaseUrl}/club/verifier/{userId}");
            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ClubResult>(json);
        }
    }

    // Modèles de réponse API
    public class LoginResult
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("token")] public string? Token { get; set; }
        [JsonProperty("employe")] public EmployeInfo? Employe { get; set; }
        [JsonProperty("message")] public string? Message { get; set; }
    }

    public class EmployeInfo
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("nom")] public string? Nom { get; set; }
        [JsonProperty("prenom")] public string? Prenom { get; set; }
        [JsonProperty("email")] public string? Email { get; set; }
        [JsonProperty("role")] public string? Role { get; set; }
    }

    public class ApiResponse
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("message")] public string? Message { get; set; }
    }

    public class ClubResult
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("membre")] public bool Membre { get; set; }
        [JsonProperty("points_total")] public int PointsTotal { get; set; }
        [JsonProperty("niveau")] public string? Niveau { get; set; }
        [JsonProperty("reduction_pct")] public decimal ReductionPct { get; set; }
        [JsonProperty("date_adhesion")] public string? DateAdhesion { get; set; }
    }
}
