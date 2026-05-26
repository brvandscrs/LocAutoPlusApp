using LocAutoPlusApp.Services;
using LocAutoPlusApp.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    public partial class VehiculesPage : Page
    {
        private readonly ApiService _api = new();
        private List<VehiculeDto> _tousLesVehicules = new();
        private VehiculeDto? _vehiculeSelectionne;
        private bool _dataChargee = false;

        public VehiculesPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ChargerVehicules();
        }

        private async Task ChargerVehicules()
        {
            _dataChargee = false;
            try
            {
                _tousLesVehicules = await _api.GetVehiculesAsync();
                DgVehicules.ItemsSource = _tousLesVehicules;
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
            if (!_dataChargee || TxtRecherche == null || CbStatut == null) return;

            var statut = (CbStatut.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var recherche = TxtRecherche.Text.ToLower();
            var filtres = _tousLesVehicules.AsEnumerable();

            if (statut != "Tous" && !string.IsNullOrEmpty(statut))
                filtres = filtres.Where(v => v.Statut == statut);

            if (!string.IsNullOrEmpty(recherche))
                filtres = filtres.Where(v =>
                    v.Marque.ToLower().Contains(recherche) ||
                    v.Modele.ToLower().Contains(recherche) ||
                    v.Immatriculation.ToLower().Contains(recherche));

            DgVehicules.ItemsSource = filtres.ToList();
        }

        private void CbStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => AppliquerFiltres();

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtPlaceholder.Visibility = string.IsNullOrEmpty(TxtRecherche.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            AppliquerFiltres();
        }

        private void DgVehicules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgVehicules.SelectedItem is not VehiculeDto v) return;

            _vehiculeSelectionne = v;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelFiche.Visibility = Visibility.Visible;
            TxtFicheNom.Text = $"{v.Marque} {v.Modele}";

            var (bg, fg, label) = v.Statut switch
            {
                "disponible" => ("#D4EDDA", "#155724", "✅ Disponible"),
                "loue" => ("#CCE5FF", "#004085", "🔑 Loué"),
                "maintenance" => ("#FFF3CD", "#856404", "🔧 Maintenance"),
                "hors_service" => ("#FEE2E2", "#C0392B", "❌ Hors service"),
                _ => ("#E2E3E5", "#383D41", v.Statut)
            };

            BadgeStatut.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(bg));
            TxtFicheStatut.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(fg));
            TxtFicheStatut.Text = label;

            ListInfos.ItemsSource = new[]
            {
                new { Label = "Immat.",    Valeur = v.Immatriculation },
                new { Label = "Catégorie", Valeur = v.Categorie },
                new { Label = "Année",     Valeur = v.Annee.ToString() },
                new { Label = "Km actuel", Valeur = $"{v.KmActuel:N0} km" },
                new { Label = "Tarif",     Valeur = $"{v.TarifJour:F2} € / jour" },
            };
        }

        private async void BtnChangerStatut_Click(object sender, RoutedEventArgs e)
        {
            if (_vehiculeSelectionne == null) return;
            if (sender is not Button btn) return;

            var nouveauStatut = btn.Tag.ToString()!;

            if (_vehiculeSelectionne.Statut == "loue")
            {
                MessageBox.Show("Impossible de modifier le statut d'un véhicule loué.",
                    "Action impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = await _api.UpdateVehiculeAsync(
                    _vehiculeSelectionne.Id,
                    new { statut = nouveauStatut });

                if (result?.Success == true)
                    await ChargerVehicules();
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

        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            if (_vehiculeSelectionne == null) return;
            var dlg = new VehiculeEditWindow(_vehiculeSelectionne);
            if (dlg.ShowDialog() == true)
                _ = ChargerVehicules();
        }

        private async void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_vehiculeSelectionne == null) return;

            if (_vehiculeSelectionne.Statut == "loue")
            {
                MessageBox.Show("Impossible de supprimer un véhicule loué.",
                    "Action impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Supprimer {_vehiculeSelectionne.Marque} {_vehiculeSelectionne.Modele} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var result = await _api.DeleteVehiculeAsync(_vehiculeSelectionne.Id);
                if (result?.Success == true)
                {
                    PanelFiche.Visibility = Visibility.Collapsed;
                    TxtSelectionner.Visibility = Visibility.Visible;
                    _vehiculeSelectionne = null;
                    await ChargerVehicules();
                }
                else
                {
                    MessageBox.Show(result?.Message ?? "Erreur.",
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
            var dlg = new VehiculeEditWindow(null);
            if (dlg.ShowDialog() == true)
                _ = ChargerVehicules();
        }
    }
}
