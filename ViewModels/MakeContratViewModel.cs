using System.Windows.Input;

namespace LocAutoPlusApp.ViewModels
{
    public class MakeContratViewModel : BaseViewModel
    {
        public ICommand EnregistrerCommand { get; }
        public ICommand AnnulerCommand { get; }

        private int _userId;
        private decimal _montant;
        private string _etatContrat;
        private DateTime _dateDebut;
        private DateTime _dateFin;

        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        public decimal Montant
        {
            get => _montant;
            set
            {
                _montant = value;
                OnPropertyChanged(nameof(Montant));
            }
        }

        public string EtatContrat
        {
            get => _etatContrat;
            set
            {
                _etatContrat = value;
                OnPropertyChanged(nameof(EtatContrat));
            }
        }

        public DateTime DateDebut
        {
            get => _dateDebut;
            set
            {
                _dateDebut = value;
                OnPropertyChanged(nameof(DateDebut));
            }
        }

        public DateTime DateFin
        {
            get => _dateFin;
            set
            {
                _dateFin = value;
                OnPropertyChanged(nameof(_dateFin));
            }
        }
    }
}
