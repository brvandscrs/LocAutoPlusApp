using System.Text.Json.Serialization;

namespace LocAutoPlusApp.Models
{
    public class Contrat
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
        [JsonPropertyName("date_debut")]
        public DateTime DateDebut { get; set; }
        [JsonPropertyName("date_fin")]
        public DateTime DateFin { get; set; }
        [JsonPropertyName("montant")]
        public decimal Montant { get; set; }
        [JsonPropertyName("etat_contrat")]
        public string EtatContrat { get; set; }

        public string FullName
        {
            get {
                return
                    Id +
                    " " +
                    UserId +
                    " " +
                    DateDebut +
                    " " +
                    Montant +
                    " " +
                    EtatContrat;
            } 
        }

        public Contrat(int id, int userId, DateTime dateDebut, DateTime dateFin, decimal montant, string etatContrat)
        {
            Id = id;
            UserId = userId;
            DateDebut = dateDebut;
            DateFin = dateFin;
            Montant = montant;
            EtatContrat = etatContrat;
        }
    }
}
