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
        public Contrat SelectedContrat
        {
            get => _selectedContrat; 
            set
            {
                _selectedContrat = value;
                OnPropertyChanged(); // Uncomment if BaseViewModel implements INotifyPropertyChanged
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
