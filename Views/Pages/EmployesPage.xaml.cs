using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Services;
using LocAutoPlusApp.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    public partial class EmployesPage : Page
    {
        private readonly ApiService _api = new();
        private List<EmployeDto> _tousLesEmployes = new();
        private EmployeDto? _employeSelectionne;
        private bool _dataChargee = false;

        public EmployesPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ChargerEmployes();
        }

        private async Task ChargerEmployes()
        {
            _dataChargee = false;
            try
            {
                _tousLesEmployes = await _api.GetEmployesAsync();
                AppliquerFiltre();
                _dataChargee = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AppliquerFiltre()
        {
            if (TxtRecherche == null) return;
            var recherche = TxtRecherche.Text.ToLower();
            DgEmployes.ItemsSource = string.IsNullOrEmpty(recherche)
                ? _tousLesEmployes
                : _tousLesEmployes.Where(e =>
                    e.NomComplet.ToLower().Contains(recherche) ||
                    e.Email.ToLower().Contains(recherche)).ToList();
        }

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtPlaceholder.Visibility = string.IsNullOrEmpty(TxtRecherche.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            AppliquerFiltre();
        }

        private void DgEmployes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgEmployes.SelectedItem is not EmployeDto emp) return;

            _employeSelectionne = emp;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelFiche.Visibility = Visibility.Visible;

            var avatarColor = emp.Role == "admin" ? "#C9A84C" : "#0F0F0F";
            AvatarBorder.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(avatarColor));

            TxtInitiales.Text = $"{emp.Prenom[0]}{emp.Nom[0]}".ToUpper();
            TxtFicheNom.Text = emp.NomComplet;

            if (emp.Role == "admin")
            {
                BadgeRole.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FFF3CD"));
                TxtBadgeRole.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#856404"));
                TxtBadgeRole.Text = "👑 Administrateur";
            }
            else
            {
                BadgeRole.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#E8E8E4"));
                TxtBadgeRole.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#555555"));
                TxtBadgeRole.Text = "🔧 Agent";
            }

            ListInfos.ItemsSource = new[]
            {
                new { Label = "Email",   Valeur = emp.Email },
                new { Label = "Statut",  Valeur = emp.Actif ? "✅ Actif" : "❌ Inactif" },
                new { Label = "Créé le", Valeur = emp.DateCreation },
            };

            BtnToggleActif.Content = emp.Actif
                ? "⏸️  Désactiver le compte" : "▶️  Activer le compte";
            BtnToggleActif.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                emp.Actif ? "#FFF3CD" : "#D4EDDA"));
            BtnToggleActif.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                emp.Actif ? "#856404" : "#155724"));

            BtnToggleActif.IsEnabled = emp.Email != AppSession.Email;
            BtnSupprimer.IsEnabled = emp.Email != AppSession.Email;
        }

        private async void BtnToggleActif_Click(object sender, RoutedEventArgs e)
        {
            if (_employeSelectionne == null) return;

            var action = _employeSelectionne.Actif ? "désactiver" : "activer";
            var confirm = MessageBox.Show(
                $"Voulez-vous {action} le compte de {_employeSelectionne.NomComplet} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var result = await _api.ToggleActifEmployeAsync(_employeSelectionne.Id);
                if (result?.Success == true)
                    await ChargerEmployes();
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
            if (_employeSelectionne == null) return;
            var dlg = new EmployeEditWindow(_employeSelectionne);
            if (dlg.ShowDialog() == true)
                _ = ChargerEmployes();
        }

        private async void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_employeSelectionne == null) return;

            var confirm = MessageBox.Show(
                $"Supprimer le compte de {_employeSelectionne.NomComplet} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var result = await _api.DeleteEmployeAsync(_employeSelectionne.Id);
                if (result?.Success == true)
                {
                    PanelFiche.Visibility = Visibility.Collapsed;
                    TxtSelectionner.Visibility = Visibility.Visible;
                    _employeSelectionne = null;
                    await ChargerEmployes();
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
            var dlg = new EmployeEditWindow(null);
            if (dlg.ShowDialog() == true)
                _ = ChargerEmployes();
        }
    }
}
