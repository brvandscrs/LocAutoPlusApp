using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocAutoPlusApp.Models
{
    public class Contrat
    {
        public int UserId { get; }
        public DateTime DateDebut { get; }
        public DateTime DateFin { get; }
        public decimal Montant { get; }
        public string EtatContrat { get; }
    }
}
