using LocAutoPlusApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocAutoPlusApp.ViewModels
{
    internal class ContratListingViewModel : INotifyPropertyChanged
    {
        private Contrat? _selectedContrat;
        public ObservableCollection<Contrat> Contrats { get; set; }
        public Contrat? SelectedContrat
        {
            get => _selectedContrat; 
            set
            {
                _selectedContrat = value;
                OnPropertyChanged(); // Uncomment if BaseViewModel implements INotifyPropertyChanged
            }
        }

        public ContratListingViewModel()
        {
            Contrats = new ObservableCollection<Contrat>();

            Contrat contrat = new Contrat(1, 1, DateTime.Now, DateTime.Now.AddDays(30), 500.00m, "Actif");
            Contrats.Add(contrat);

            contrat = new Contrat(1, 1, DateTime.Now, DateTime.Now.AddDays(60), 500.00m, "En cours");
            Contrats.Add(contrat);

            contrat = new Contrat(1, 1, DateTime.Now, DateTime.Now, 500.00m, "Terminé");
            Contrats.Add(contrat);
            // LoadContrats(); // Load contrats from API or database
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
