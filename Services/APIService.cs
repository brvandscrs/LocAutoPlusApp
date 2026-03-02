using LocAutoPlusApp.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace LocAutoPlusApp.Services
{
    internal class ContratsService
    {
        private readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:8000/api/") };
        public async Task<List<Contrat>> GetContrats()
            => await _http.GetFromJsonAsync<List<Contrat>>("contrats") ?? new List<Contrat>();

        /* public async Task<List<Contrat>> GetContratsAsync()
        {
            HttpResponseMessage response = await _http.GetAsync("contrats");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Contrat>>(json) ?? new List<Contrat>();
            }
            else
            {
                // Handle error (e.g., log it, throw an exception, etc.)
                return new List<Contrat>();
            }
        } */
    }

    internal class UsersService
    {
    }
}
