using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Services;
using LocAutoPlusApp.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    public partial class ContratsPage : Page
    {
        private readonly ApiService _api = new();
        private List<ContratDto> _tousLesContrats = new();
        private ContratDto? _contratSelectionne;
        private readonly int? _filtreClientId;
        private bool _dataChargee = false;

        public ContratsPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ChargerContrats();
        }

        public ContratsPage(int clientId, string nomClient)
        {
            InitializeComponent();
            _filtreClientId = clientId;
            Loaded += async (s, e) =>
            {
                TxtFiltreClient.Text = $"📌 Contrats de {nomClient}";
                TxtFiltreClient.Visibility = Visibility.Visible;
                await ChargerContrats();
            };
        }

        private async Task ChargerContrats()
        {
            _dataChargee = false;
            try
            {
                _tousLesContrats = await _api.GetContratsAsync(userId: _filtreClientId);
                AppliquerFiltres();
                _dataChargee = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AppliquerFiltres()
        {
            if (TxtRecherche == null || CbStatut == null) return;

            var statut = (CbStatut.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var recherche = TxtRecherche.Text.ToLower();
            var filtres = _tousLesContrats.AsEnumerable();

            if (statut != "Tous" && !string.IsNullOrEmpty(statut))
                filtres = filtres.Where(c => c.Statut == statut);

            if (!string.IsNullOrEmpty(recherche))
                filtres = filtres.Where(c =>
                    c.Client.ToLower().Contains(recherche) ||
                    c.Vehicule.ToLower().Contains(recherche));

            DgContrats.ItemsSource = filtres.ToList();
        }

        private void CbStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => AppliquerFiltres();

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtPlaceholder.Visibility = string.IsNullOrEmpty(TxtRecherche.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            AppliquerFiltres();
        }

        private void DgContrats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgContrats.SelectedItem is not ContratDto c) return;

            _contratSelectionne = c;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Visible;
            TxtDetailId.Text = $"Contrat #{c.Id}";

            var (bg, fg, label) = c.Statut switch
            {
                "en_attente" => ("#FFF3CD", "#856404", "En attente"),
                "confirmee" => ("#CCE5FF", "#004085", "Confirmée"),
                "en_cours" => ("#D4EDDA", "#155724", "En cours"),
                "terminee" => ("#E2E3E5", "#383D41", "Terminée"),
                "annulee" => ("#FEE2E2", "#C0392B", "Annulée"),
                _ => ("#E2E3E5", "#383D41", c.Statut)
            };

            BadgeStatut.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(bg));
            TxtDetailStatut.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(fg));
            TxtDetailStatut.Text = label;

            ListDetails.ItemsSource = new[]
            {
                new { Label = "Client",     Valeur = c.Client,           Gras = "Normal" },
                new { Label = "Véhicule",   Valeur = $"{c.Vehicule} ({c.Immatriculation})", Gras = "Normal" },
                new { Label = "Réservé le", Valeur = c.DateReservation,  Gras = "Normal" },
                new { Label = "Début",      Valeur = c.DateDebut,        Gras = "SemiBold" },
                new { Label = "Fin prévue", Valeur = c.DateFin,          Gras = "SemiBold" },
                new { Label = "Fin réelle", Valeur = c.DateFinReelle,    Gras = "Normal" },
                new { Label = "Km départ",  Valeur = c.KmDepart > 0 ? $"{c.KmDepart:N0} km" : "—", Gras = "Normal" },
                new { Label = "Km retour",  Valeur = c.KmRetour > 0 ? $"{c.KmRetour:N0} km" : "—", Gras = "Normal" },
                new { Label = "Base",       Valeur = $"{c.MontantBase:F2} €",   Gras = "Normal" },
                new { Label = "Réduction",  Valeur = $"{c.Reduction} %",        Gras = "Normal" },
                new { Label = "Total",      Valeur = $"{c.MontantTotal:F2} €",  Gras = "Bold" },
                new { Label = "Employé",    Valeur = c.Employe,          Gras = "Normal" },
            };

            BtnConfirmer.Visibility = c.Statut == "en_attente" ? Visibility.Visible : Visibility.Collapsed;
            BtnDemarrer.Visibility = c.Statut == "confirmee" ? Visibility.Visible : Visibility.Collapsed;
            BtnCloturer.Visibility = c.Statut == "en_cours" ? Visibility.Visible : Visibility.Collapsed;
            BtnAnnuler.Visibility = c.Statut is "en_attente" or "confirmee"
                                      ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnConfirmer_Click(object sender, RoutedEventArgs e)
            => await ChangerStatut(_contratSelectionne!.Id, "confirmee");

        private async void BtnDemarrer_Click(object sender, RoutedEventArgs e)
        {
            if (_contratSelectionne == null) return;
            var dlg = new ContratDemarrerWindow(
                _contratSelectionne.Id, _contratSelectionne.KmDepart);
            if (dlg.ShowDialog() == true)
                await ChargerContrats();
        }

        private async void BtnCloturer_Click(object sender, RoutedEventArgs e)
        {
            if (_contratSelectionne == null) return;
            var dlg = new ContratCloturerWindow(_contratSelectionne.Id);
            if (dlg.ShowDialog() != true) return;

            try
            {
                var result = await _api.CloturerContratAsync(
                    _contratSelectionne.Id,
                    dlg.KmRetour,
                    dlg.DateFinReelle.ToString("yyyy-MM-dd"));

                if (result?.Success == true)
                {
                    MessageBox.Show($"Contrat clôturé ✅\n{result.Message}",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    await ChargerContrats();
                }
                else
                {
                    MessageBox.Show(result?.Message ?? "Erreur lors de la clôture.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur API : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAnnuler_Click(object sender, RoutedEventArgs e)
            => await ChangerStatut(_contratSelectionne!.Id, "annulee");

        private async Task ChangerStatut(int contratId, string statut)
        {
            var labels = new Dictionary<string, string>
            {
                { "confirmee", "confirmer" },
                { "annulee",   "annuler"   }
            };

            var confirm = MessageBox.Show(
                $"Voulez-vous {labels.GetValueOrDefault(statut, statut)} ce contrat ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var result = await _api.UpdateStatutContratAsync(
                    contratId, new { statut });

                if (result?.Success == true)
                    await ChargerContrats();
                else
                    MessageBox.Show(result?.Message ?? "Erreur.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContratCreerWindow();
            if (dlg.ShowDialog() == true)
                _ = ChargerContrats();
        }
    }
}
