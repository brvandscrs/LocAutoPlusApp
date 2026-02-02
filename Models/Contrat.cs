namespace LocAutoPlusApp.Models
{
    public class Contrat
    {
        public int UserId { get; }
        public DateTime DateDebut { get; }
        public DateTime DateFin { get; }
        public decimal Montant { get; }
        public string EtatContrat { get; set; }

        public Contrat(int userId, DateTime dateDebut, DateTime dateFin, decimal montant, string etatContrat)
        {
            UserId = userId;
            DateDebut = dateDebut;
            DateFin = dateFin;
            Montant = montant;
            EtatContrat = etatContrat;
        }
    }
}
