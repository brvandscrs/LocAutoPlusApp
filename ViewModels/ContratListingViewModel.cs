using LocAutoPlusApp.Commands;
using LocAutoPlusApp.Models;
using LocAutoPlusApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace LocAutoPlusApp.ViewModels
{
    internal class ContratListingViewModel : INotifyPropertyChanged
    {
        private readonly ContratsService _contratsService = new ContratsService();

        private Contrat? _selectedContrat;
        public ObservableCollection<Contrat> Contrats { get; set; }

        public ICommand SaveUserCommand { get; }

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
            SaveUserCommand = new RelayCommand(async () => await SaveContratAsync(),
                                               () => SelectedContrat != null);
            _ = LoadContrats(); // Load contrats from API or database
        }

        private async Task SaveContratAsync()
        {
            try
            {
                await _contratsService.UpdateContrat(SelectedContrat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private async Task LoadContrats()
        {
            // List<Contrat> data = await _contratsService.GetContrats();
            try
            {
                var contrats = await _contratsService.GetContrats();
                Contrats.Clear();
                foreach (var contrat in contrats)
                    Contrats.Add(contrat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}");
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
