using LocAutoPlusApp.Services;
using LocAutoPlusApp.Views;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views.Pages
{
    public partial class ClientsPage : Page
    {
        private readonly ApiService _api = new();
        private List<ClientDto> _tousLesClients = new();
        private ClientDto? _clientSelectionne;

        public ClientsPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ChargerClients();
        }

        private async Task ChargerClients()
        {
            try
            {
                _tousLesClients = await _api.GetClientsAsync();
                DgClients.ItemsSource = _tousLesClients;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtPlaceholder.Visibility = string.IsNullOrEmpty(TxtRecherche.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            var recherche = TxtRecherche.Text.ToLower();
            DgClients.ItemsSource = _tousLesClients
                .Where(c => c.NomComplet.ToLower().Contains(recherche) ||
                            c.Email.ToLower().Contains(recherche) ||
                            c.Telephone.ToLower().Contains(recherche))
                .ToList();
        }

        private void DgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgClients.SelectedItem is not ClientDto client) return;

            _clientSelectionne = client;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelFiche.Visibility = Visibility.Visible;

            TxtInitiales.Text = $"{client.Prenom[0]}{client.Nom[0]}".ToUpper();
            TxtFicheNom.Text = client.NomComplet;

            if (client.ClubActif && client.NiveauClub != null)
            {
                TxtFicheClub.Text = $"⭐ Club {client.NiveauClub} — {client.PointsClub} pts";
                BadgeClub.Visibility = Visibility.Visible;
            }
            else
            {
                BadgeClub.Visibility = Visibility.Collapsed;
            }

            ListInfos.ItemsSource = new[]
            {
                new { Label = "Email",      Valeur = client.Email },
                new { Label = "Téléphone",  Valeur = client.Telephone },
                new { Label = "Adresse",    Valeur = client.Adresse },
                new { Label = "Naissance",  Valeur = client.DateNaissance },
                new { Label = "Inscrit le", Valeur = client.DateInscription },
                new { Label = "Contrats",   Valeur = $"{client.NbContrats} contrat(s)" },
            };
        }

        private void BtnVoirContrats_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;
            NavigationService?.Navigate(
                new ContratsPage(_clientSelectionne.Id, _clientSelectionne.NomComplet));
        }

        private void BtnModifierClient_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;
            var dialog = new ClientEditWindow(_clientSelectionne);
            if (dialog.ShowDialog() == true)
                _ = ChargerClients();
        }

        private async void BtnSupprimerClient_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;

            if (_clientSelectionne.NbContrats > 0)
            {
                MessageBox.Show(
                    $"Impossible de supprimer {_clientSelectionne.NomComplet}.\n" +
                    $"Ce client possède {_clientSelectionne.NbContrats} contrat(s).",
                    "Suppression impossible",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Supprimer définitivement {_clientSelectionne.NomComplet} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var result = await _api.DeleteClientAsync(_clientSelectionne.Id);
                if (result?.Success == true)
                {
                    PanelFiche.Visibility = Visibility.Collapsed;
                    TxtSelectionner.Visibility = Visibility.Visible;
                    _clientSelectionne = null;
                    await ChargerClients();
                }
                else
                {
                    MessageBox.Show(result?.Message ?? "Erreur lors de la suppression.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClientEditWindow(null);
            if (dialog.ShowDialog() == true)
                _ = ChargerClients();
        }
    }
}
