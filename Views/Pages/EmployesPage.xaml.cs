using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    /// <summary>
    /// Logique d'interaction pour EmployesPage.xaml
    /// </summary>
    public partial class EmployesPage : Page
    {
        private List<EmployeModel> _tousLesEmployes = new();
        private EmployeModel? _employeSelectionne;

        public EmployesPage()
        {
            InitializeComponent();
            Loaded += (s, e) => ChargerEmployes();
        }

        private void ChargerEmployes()
        {
            try
            {
                _tousLesEmployes.Clear();

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT id, nom, prenom, email, role, actif, created_at
                    FROM employes
                    ORDER BY nom, prenom", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _tousLesEmployes.Add(new EmployeModel
                    {
                        Id = reader.GetInt32("id"),
                        Nom = reader.GetString("nom"),
                        Prenom = reader.GetString("prenom"),
                        Email = reader.GetString("email"),
                        Role = reader.GetString("role"),
                        Actif = reader.GetBoolean("actif"),
                        DateCreation = reader.GetDateTime("created_at").ToString("dd/MM/yyyy"),
                    });
                }

                AppliquerFiltre();
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
            if (DgEmployes.SelectedItem is not EmployeModel emp) return;

            _employeSelectionne = emp;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelFiche.Visibility = Visibility.Visible;

            // Avatar couleur selon rôle
            var avatarColor = emp.Role == "admin" ? "#C9A84C" : "#0F0F0F";
            AvatarBorder.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(avatarColor));

            TxtInitiales.Text = $"{emp.Prenom[0]}{emp.Nom[0]}".ToUpper();
            TxtFicheNom.Text = emp.NomComplet;

            // Badge rôle
            if (emp.Role == "admin")
            {
                BadgeRole.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3CD"));
                TxtBadgeRole.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#856404"));
                TxtBadgeRole.Text = "👑 Administrateur";
            }
            else
            {
                BadgeRole.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8E8E4"));
                TxtBadgeRole.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
                TxtBadgeRole.Text = "🔧 Agent";
            }

            ListInfos.ItemsSource = new[]
            {
                new { Label = "Email",      Valeur = emp.Email },
                new { Label = "Statut",     Valeur = emp.Actif ? "✅ Actif" : "❌ Inactif" },
                new { Label = "Créé le",    Valeur = emp.DateCreation },
            };

            // Bouton activer/désactiver
            BtnToggleActif.Content = emp.Actif ? "⏸️  Désactiver le compte" : "▶️  Activer le compte";
            BtnToggleActif.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                emp.Actif ? "#FFF3CD" : "#D4EDDA"));
            BtnToggleActif.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                emp.Actif ? "#856404" : "#155724"));

            // Empêche l'admin de se désactiver lui-même
            BtnToggleActif.IsEnabled = emp.Email != AppSession.Email;
            BtnSupprimer.IsEnabled = emp.Email != AppSession.Email;
        }

        private void BtnToggleActif_Click(object sender, RoutedEventArgs e)
        {
            if (_employeSelectionne == null) return;

            var nouvelEtat = !_employeSelectionne.Actif;
            var action = nouvelEtat ? "activer" : "désactiver";

            var confirm = MessageBox.Show(
                $"Voulez-vous {action} le compte de {_employeSelectionne.NomComplet} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE employes SET actif = @actif WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@actif", nouvelEtat);
                cmd.Parameters.AddWithValue("@id", _employeSelectionne.Id);
                cmd.ExecuteNonQuery();
                ChargerEmployes();
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
                ChargerEmployes();
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_employeSelectionne == null) return;

            var confirm = MessageBox.Show(
                $"Supprimer le compte de {_employeSelectionne.NomComplet} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM employes WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _employeSelectionne.Id);
                cmd.ExecuteNonQuery();

                PanelFiche.Visibility = Visibility.Collapsed;
                TxtSelectionner.Visibility = Visibility.Visible;
                _employeSelectionne = null;
                ChargerEmployes();
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
                ChargerEmployes();
        }
    }

    public class EmployeModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool Actif { get; set; }
        public string DateCreation { get; set; } = "";

        public string NomComplet => $"{Prenom} {Nom}";
        public string RoleLabel => Role == "admin" ? "👑 Admin" : "🔧 Agent";
        public string StatutLabel => Actif ? "✅ Actif" : "❌ Inactif";
    }
}
