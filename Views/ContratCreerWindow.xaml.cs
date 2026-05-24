using LocAutoPlusApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    public partial class ContratCreerWindow : Window
    {
        private readonly ApiService _api = new();
        private decimal _tarifJour = 0;
        private decimal _reductionPct = 0;
        private int _clientId = 0;
        private List<ClientDto> _clients = new();
        private List<VehiculeDto> _vehicules = new();

        public ContratCreerWindow()
        {
            InitializeComponent();
            DpDebut.SelectedDate = DateTime.Today;
            DpFin.SelectedDate = DateTime.Today.AddDays(1);
            Loaded += async (s, e) => await ChargerDonnees();
        }

        private async Task ChargerDonnees()
        {
            try
            {
                // Clients
                _clients = await _api.GetClientsAsync();
                CbClient.ItemsSource = _clients;
                CbClient.DisplayMemberPath = "NomComplet";  // ← ajoute
                CbClient.SelectedValuePath = "Id";          // ← ajoute

                // Véhicules disponibles
                _vehicules = await _api.GetVehiculesAsync(statut: "disponible");
                CbVehicule.ItemsSource = _vehicules;
                CbVehicule.DisplayMemberPath = "LibelleComplet";  // ← ajoute
                CbVehicule.SelectedValuePath = "Id";              // ← ajoute
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement : " + ex.Message);
            }
        }

        private async void CbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbClient.SelectedItem is not ClientDto client) return;
            _clientId = client.Id;

            try
            {
                var club = await _api.VerifierClubAsync(_clientId);
                _reductionPct = club?.Membre == true ? club.ReductionPct : 0;
            }
            catch { _reductionPct = 0; }

            CalculerMontant();
        }

        private void CbVehicule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbVehicule.SelectedItem is not VehiculeDto v) return;
            _tarifJour = v.TarifJour;
            CalculerMontant();
        }

        private void Dates_Changed(object? sender, SelectionChangedEventArgs e)
            => CalculerMontant();

        private void CalculerMontant()
        {
            if (_tarifJour == 0 ||
                !DpDebut.SelectedDate.HasValue ||
                !DpFin.SelectedDate.HasValue) return;

            var debut = DpDebut.SelectedDate.Value;
            var fin = DpFin.SelectedDate.Value;
            var jours = (fin - debut).Days;
            if (jours <= 0) return;

            var montantBase = jours * _tarifJour;
            var montantTotal = montantBase - (montantBase * _reductionPct / 100);

            TxtRecapLabel.Text = $"{jours} jour(s) × {_tarifJour:F2} €";
            TxtRecapMontant.Text = $"{montantTotal:F2} €";
            TxtRecapMontant.Visibility = Visibility.Visible;

            if (_reductionPct > 0)
            {
                TxtRecapReduction.Text = $"⭐ Réduction Club {_reductionPct}% appliquée";
                TxtRecapReduction.Visibility = Visibility.Visible;
            }
            else
            {
                TxtRecapReduction.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            if (CbClient.SelectedItem is not ClientDto client ||
                CbVehicule.SelectedItem is not VehiculeDto ||
                !DpDebut.SelectedDate.HasValue ||
                !DpFin.SelectedDate.HasValue)
            {
                TxtErreur.Text = "Veuillez remplir tous les champs obligatoires.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            var debut = DpDebut.SelectedDate.Value;
            var fin = DpFin.SelectedDate.Value;
            if ((fin - debut).Days <= 0)
            {
                TxtErreur.Text = "La date de fin doit être après la date de début.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            var vehicule = (VehiculeDto)CbVehicule.SelectedItem;

            try
            {
                var result = await _api.CreateContratAsync(new
                {
                    user_id = _clientId,
                    vehicule_id = vehicule.Id,
                    date_debut = debut.ToString("yyyy-MM-dd"),
                    date_fin_prevue = fin.ToString("yyyy-MM-dd"),
                });

                if (result?.Success == true)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    TxtErreur.Text = result?.Message ?? "Erreur lors de la création.";
                    TxtErreur.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                TxtErreur.Text = "Erreur : " + ex.Message;
                TxtErreur.Visibility = Visibility.Visible;
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
