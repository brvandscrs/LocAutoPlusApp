using LocAutoPlusApp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace LocAutoPlusApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "https://locautoplus.fr/api";

        public ApiService()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // ── Auth ─────────────────────────────────────────────────
        public async Task<LoginResult?> LoginAsync(string email, string password)
        {
            var response = await PostAsync("/employes/login",
                new { email, password }, withToken: false);
            return Deserialize<LoginResult>(response);
        }

        public async Task LogoutAsync()
        {
            await PostAsync("/employes/logout", null);
        }

        // ── Dashboard ────────────────────────────────────────────
        public async Task<DashboardStats?> GetDashboardStatsAsync()
        {
            var json = await GetAsync("/dashboard/stats");
            return Deserialize<DashboardStats>(json);
        }

        // ── Clients ──────────────────────────────────────────────
        public async Task<List<ClientDto>> GetClientsAsync()
        {
            var json = await GetAsync("/clients");
            return DeserializeList<ClientDto>(json, "data");
        }

        public async Task<ApiResponse?> CreateClientAsync(object payload)
        {
            var json = await PostAsync("/clients", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> UpdateClientAsync(int id, object payload)
        {
            var json = await PutAsync($"/clients/{id}", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> DeleteClientAsync(int id)
        {
            var json = await DeleteAsync($"/clients/{id}");
            return Deserialize<ApiResponse>(json);
        }

        // ── Contrats ─────────────────────────────────────────────
        public async Task<List<ContratDto>> GetContratsAsync(
            int? userId = null, string? statut = null)
        {
            var query = "";
            if (userId.HasValue) query += $"?user_id={userId}";
            if (!string.IsNullOrEmpty(statut) && statut != "Tous")
                query += (query == "" ? "?" : "&") + $"statut={statut}";

            var json = await GetAsync($"/contrats{query}");
            return DeserializeList<ContratDto>(json, "data");
        }

        public async Task<ApiResponse?> CreateContratAsync(object payload)
        {
            var json = await PostAsync("/contrats", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> UpdateStatutContratAsync(
            int id, object payload)
        {
            var json = await PutAsync($"/contrats/{id}/statut", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> CloturerContratAsync(
            int contratId, int kmRetour, string dateFinReelle)
        {
            var json = await PostAsync($"/contrats/{contratId}/cloturer",
                new { km_retour = kmRetour, date_fin_reelle = dateFinReelle });
            return Deserialize<ApiResponse>(json);
        }

        // ── Véhicules ────────────────────────────────────────────
        public async Task<List<VehiculeDto>> GetVehiculesAsync(
            string? statut = null, string? search = null)
        {
            var query = "";
            if (!string.IsNullOrEmpty(statut) && statut != "Tous")
                query += $"?statut={statut}";
            if (!string.IsNullOrEmpty(search))
                query += (query == "" ? "?" : "&") + $"search={Uri.EscapeDataString(search)}";

            var json = await GetAsync($"/vehicules{query}");
            return DeserializeList<VehiculeDto>(json, "data");
        }

        public async Task<ApiResponse?> CreateVehiculeAsync(object payload)
        {
            var json = await PostAsync("/vehicules", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> UpdateVehiculeAsync(int id, object payload)
        {
            var json = await PutAsync($"/vehicules/{id}", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> DeleteVehiculeAsync(int id)
        {
            var json = await DeleteAsync($"/vehicules/{id}");
            return Deserialize<ApiResponse>(json);
        }

        // ── Catégories ───────────────────────────────────────────
        public async Task<List<CategorieDto>> GetCategoriesAsync()
        {
            var json = await GetAsync("/categories");
            return DeserializeList<CategorieDto>(json, "data");
        }

        // ── Club ─────────────────────────────────────────────────
        public async Task<List<MembreDto>> GetMembresClubAsync()
        {
            var json = await GetAsync("/club/membres");
            return DeserializeList<MembreDto>(json, "data");
        }

        public async Task<List<NiveauDto>> GetNiveauxClubAsync()
        {
            var json = await GetAsync("/club/niveaux");
            return DeserializeList<NiveauDto>(json, "data");
        }

        public async Task<ClubResult?> VerifierClubAsync(int userId)
        {
            var json = await PostAsync($"/club/verifier/{userId}", null);
            return Deserialize<ClubResult>(json);
        }

        // ── Employés ─────────────────────────────────────────────
        public async Task<List<EmployeDto>> GetEmployesAsync()
        {
            var json = await GetAsync("/employes");
            return DeserializeList<EmployeDto>(json, "data");
        }

        public async Task<ApiResponse?> CreateEmployeAsync(object payload)
        {
            var json = await PostAsync("/employes", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> UpdateEmployeAsync(int id, object payload)
        {
            var json = await PutAsync($"/employes/{id}", payload);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> ToggleActifEmployeAsync(int id)
        {
            var json = await PatchAsync($"/employes/{id}/actif", null);
            return Deserialize<ApiResponse>(json);
        }

        public async Task<ApiResponse?> DeleteEmployeAsync(int id)
        {
            var json = await DeleteAsync($"/employes/{id}");
            return Deserialize<ApiResponse>(json);
        }

        // ── HTTP helpers ─────────────────────────────────────────
        private void SetAuthHeader()
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AppSession.Token);
        }

        private async Task<string> GetAsync(string endpoint)
        {
            SetAuthHeader();
            var response = await _http.GetAsync(BaseUrl + endpoint);
            return await response.Content.ReadAsStringAsync();
        }

        //private async Task<string> GetAsync(string endpoint)
        //{
        //    SetAuthHeader();
        //    var response = await _http.GetAsync(BaseUrl + endpoint);
        //    var json = await response.Content.ReadAsStringAsync();

        //    // Debug temporaire
        //    if (!response.IsSuccessStatusCode)
        //        System.Windows.MessageBox.Show(
        //            $"Erreur {(int)response.StatusCode}\nEndpoint : {endpoint}\nRéponse : {json}",
        //            "API Debug", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

        //    return json;
        //}

        private async Task<string> PostAsync(
            string endpoint, object? payload, bool withToken = true)
        {
            if (withToken) SetAuthHeader();
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(BaseUrl + endpoint, content);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PutAsync(string endpoint, object? payload)
        {
            SetAuthHeader();
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(BaseUrl + endpoint, content);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PatchAsync(string endpoint, object? payload)
        {
            SetAuthHeader();
            var content = new StringContent(
                JsonConvert.SerializeObject(payload ?? new { }),
                Encoding.UTF8, "application/json");
            var response = await _http.PatchAsync(BaseUrl + endpoint, content);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> DeleteAsync(string endpoint)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync(BaseUrl + endpoint);
            return await response.Content.ReadAsStringAsync();
        }

        private static T? Deserialize<T>(string json)
        {
            try { return JsonConvert.DeserializeObject<T>(json); }
            catch { return default; }
        }

        private static List<T> DeserializeList<T>(string json, string key)
        {
            try
            {
                var obj = JObject.Parse(json);
                return obj[key]?.ToObject<List<T>>() ?? new List<T>();
            }
            catch { return new List<T>(); }
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────

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

    public class DashboardStats
    {
        [JsonProperty("nb_clients")] public int NbClients { get; set; }
        [JsonProperty("nb_contrats_en_cours")] public int NbContratsEnCours { get; set; }
        [JsonProperty("nb_vehicules_dispos")] public int NbVehiculesDispos { get; set; }
        [JsonProperty("nb_membres_club")] public int NbMembresClub { get; set; }
        [JsonProperty("derniers_contrats")] public List<ContratDto> DerniersContrats { get; set; } = new();
        [JsonProperty("vehicules_loues")] public List<ContratDto> VehiculesLoues { get; set; } = new();
    }

    public class ClientDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("nom")] public string Nom { get; set; } = "";
        [JsonProperty("prenom")] public string Prenom { get; set; } = "";
        [JsonProperty("email")] public string Email { get; set; } = "";
        [JsonProperty("telephone")] public string Telephone { get; set; } = "—";
        [JsonProperty("adresse")] public string Adresse { get; set; } = "—";
        [JsonProperty("code_postal")] public string CodePostal { get; set; } = "—";
        [JsonProperty("ville")] public string Ville { get; set; } = "—";
        [JsonProperty("date_naissance")] public string DateNaissance { get; set; } = "—";
        [JsonProperty("date_inscription")] public string DateInscription { get; set; } = "";
        [JsonProperty("nb_contrats")] public int NbContrats { get; set; }
        [JsonProperty("club_actif")] public bool ClubActif { get; set; }
        [JsonProperty("niveau_club")] public string? NiveauClub { get; set; }
        [JsonProperty("points_club")] public int PointsClub { get; set; }

        public string NomComplet => $"{Prenom} {Nom}";
        public string StatutClub => ClubActif && NiveauClub != null
                                     ? $"⭐ {NiveauClub}" : "—";
    }

    public class ContratDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("client")] public string Client { get; set; } = "";
        [JsonProperty("vehicule")] public string Vehicule { get; set; } = "";
        [JsonProperty("immatriculation")] public string Immatriculation { get; set; } = "";
        [JsonProperty("date_debut")] public string DateDebut { get; set; } = "";
        [JsonProperty("date_fin")] public string DateFin { get; set; } = "";
        [JsonProperty("date_fin_reelle")] public string DateFinReelle { get; set; } = "—";
        [JsonProperty("date_reservation")] public string DateReservation { get; set; } = "";
        [JsonProperty("montant_base")] public decimal MontantBase { get; set; }
        [JsonProperty("reduction")] public decimal Reduction { get; set; }
        [JsonProperty("montant_total")] public decimal MontantTotal { get; set; }
        [JsonProperty("montant")] public string Montant { get; set; } = "";
        [JsonProperty("statut")] public string Statut { get; set; } = "";
        [JsonProperty("km_depart")] public int KmDepart { get; set; }
        [JsonProperty("km_retour")] public int KmRetour { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; } = "";
        [JsonProperty("employe")] public string Employe { get; set; } = "—";
        [JsonProperty("user_id")] public int UserId { get; set; }
        [JsonProperty("vehicule_id")] public int VehiculeId { get; set; }

        public string MontantLabel => !string.IsNullOrEmpty(Montant)
            ? Montant : $"{MontantTotal:F2} €";
    }

    public class VehiculeDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("immatriculation")] public string Immatriculation { get; set; } = "";
        [JsonProperty("marque")] public string Marque { get; set; } = "";
        [JsonProperty("modele")] public string Modele { get; set; } = "";
        [JsonProperty("annee")] public int Annee { get; set; }
        [JsonProperty("km_actuel")] public int KmActuel { get; set; }
        [JsonProperty("statut")] public string Statut { get; set; } = "";
        [JsonProperty("photo_url")] public string? PhotoUrl { get; set; }
        [JsonProperty("categorie")] public string Categorie { get; set; } = "";
        [JsonProperty("categorie_id")] public int CategorieId { get; set; }
        [JsonProperty("tarif_jour")] public decimal TarifJour { get; set; }

        public string StatutLabel => Statut switch
        {
            "disponible" => "✅ Disponible",
            "loue" => "🔑 Loué",
            "maintenance" => "🔧 Maintenance",
            "hors_service" => "❌ Hors service",
            _ => Statut
        };
        public string LibelleComplet => $"{Marque} {Modele} — {Immatriculation} ({Annee})";
    }

    public class CategorieDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("nom")] public string Nom { get; set; } = "";
        [JsonProperty("tarif_base_jour")] public decimal TarifBaseJour { get; set; }
        [JsonProperty("label")] public string Label { get; set; } = "";
    }

    public class MembreDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("user_id")] public int UserId { get; set; }
        [JsonProperty("nom_complet")] public string NomComplet { get; set; } = "";
        [JsonProperty("email")] public string Email { get; set; } = "";
        [JsonProperty("points_total")] public int PointsTotal { get; set; }
        [JsonProperty("niveau")] public string Niveau { get; set; } = "";
        [JsonProperty("reduction_pct")] public decimal ReductionPct { get; set; }
        [JsonProperty("date_adhesion")] public string DateAdhesion { get; set; } = "";
        [JsonProperty("actif")] public bool Actif { get; set; }

        public string ReductionLabel => $"{ReductionPct}%";
    }

    public class NiveauDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("nom")] public string Nom { get; set; } = "";
        [JsonProperty("points_min")] public int PointsMin { get; set; }
        [JsonProperty("reduction_pct")] public decimal ReductionPct { get; set; }
        [JsonProperty("nb_membres")] public int NbMembres { get; set; }
    }

    public class EmployeDto
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("nom")] public string Nom { get; set; } = "";
        [JsonProperty("prenom")] public string Prenom { get; set; } = "";
        [JsonProperty("email")] public string Email { get; set; } = "";
        [JsonProperty("role")] public string Role { get; set; } = "";
        [JsonProperty("actif")] public bool Actif { get; set; }
        [JsonProperty("date_creation")] public string DateCreation { get; set; } = "";

        public string NomComplet => $"{Prenom} {Nom}";
        public string RoleLabel => Role == "admin" ? "👑 Admin" : "🔧 Agent";
        public string StatutLabel => Actif ? "✅ Actif" : "❌ Inactif";
    }

    public class ClubResult
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("membre")] public bool Membre { get; set; }
        [JsonProperty("points_total")] public int PointsTotal { get; set; }
        [JsonProperty("niveau")] public string? Niveau { get; set; }
        [JsonProperty("reduction_pct")] public decimal ReductionPct { get; set; }
        [JsonProperty("date_adhesion")] public string? DateAdhesion { get; set; }
        [JsonProperty("historique")] public List<HistoriqueDto> Historique { get; set; } = new();
    }

    public class HistoriqueDto
    {
        [JsonProperty("points")] public int Points { get; set; }
        [JsonProperty("motif")] public string Motif { get; set; } = "";
        [JsonProperty("date")] public string Date { get; set; } = "";

        public string PointsLabel => $"+{Points} pts";
    }
}
