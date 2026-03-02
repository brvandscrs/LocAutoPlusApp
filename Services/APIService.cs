using LocAutoPlusApp.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;

namespace LocAutoPlusApp.Services
{
    internal class ContratsService
    {
        private readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:8000/api/") };
        public async Task<List<Contrat>> GetContrats()
            => await _http.GetFromJsonAsync<List<Contrat>>("contrats") ?? new List<Contrat>();

        public async Task UpdateContrat(Contrat contrat)
        {
            var json = JsonSerializer.Serialize(contrat);
            var bytes = Encoding.UTF8.GetBytes(json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentLength = bytes.Length;

            var response = await _http.PutAsync($"contrats/{contrat.Id}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var body = await response.Content.ReadAsStringAsync();
                var error = JsonSerializer.Deserialize<ValidationError>(body);
                throw new Exception(error?.GetDetails() ?? "Erreur de validation");
            }

            response.EnsureSuccessStatusCode();
        }
    }

    internal class UsersService
    {
    }
}
