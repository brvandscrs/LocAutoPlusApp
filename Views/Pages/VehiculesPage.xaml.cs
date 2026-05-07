using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    /// <summary>
    /// Logique d'interaction pour VehiculesPage.xaml
    /// </summary>
    public partial class VehiculesPage : Page
    {
        private List<VehiculeModel> _tousLesVehicules = new();
        private VehiculeModel? _vehiculeSelectionne;

        public VehiculesPage()
        {
            InitializeComponent();
            Loaded += (s, e) => ChargerVehicules();
        }

        // ── Chargement ──────────────────────────────────────────
        private void ChargerVehicules()
        {
            try
            {
                _tousLesVehicules.Clear();

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT v.id, v.immatriculation, v.marque, v.modele,
                           v.annee, v.km_actuel, v.statut, v.photo_url,
                           c.nom AS categorie, c.tarif_base_jour
                    FROM vehicules v
                    JOIN categories_vehicules c ON v.categorie_id = c.id
                    ORDER BY v.marque, v.modele", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _tousLesVehicules.Add(new VehiculeModel
                    {
                        Id = reader.GetInt32("id"),
                        Immatriculation = reader.GetString("immatriculation"),
                        Marque = reader.GetString("marque"),
                        Modele = reader.GetString("modele"),
                        Annee = reader.GetInt32("annee"),
                        KmActuel = reader.GetInt32("km_actuel"),
                        Statut = reader.GetString("statut"),
                        Categorie = reader.GetString("categorie"),
                        TarifJour = reader.GetDecimal("tarif_base_jour"),
                    });
                }

                AppliquerFiltres();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Filtres ─────────────────────────────────────────────
        private void AppliquerFiltres()
        {
            if (TxtRecherche == null || CbStatut == null) return;

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

        // ── Sélection ───────────────────────────────────────────
        private void DgVehicules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgVehicules.SelectedItem is not VehiculeModel v) return;

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

            BadgeStatut.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg));
            TxtFicheStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg));
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

        // ── Changer statut ──────────────────────────────────────
        private void BtnChangerStatut_Click(object sender, RoutedEventArgs e)
        {
            if (_vehiculeSelectionne == null) return;
            if (sender is not Button btn) return;

            var nouveauStatut = btn.Tag.ToString()!;

            if (_vehiculeSelectionne.Statut == "loue")
            {
                MessageBox.Show("Impossible de modifier le statut d'un véhicule actuellement loué.",
                    "Action impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE vehicules SET statut = @statut WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@statut", nouveauStatut);
                cmd.Parameters.AddWithValue("@id", _vehiculeSelectionne.Id);
                cmd.ExecuteNonQuery();
                ChargerVehicules();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Modifier / Supprimer ────────────────────────────────
        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            if (_vehiculeSelectionne == null) return;
            var dlg = new VehiculeEditWindow(_vehiculeSelectionne);
            if (dlg.ShowDialog() == true)
                ChargerVehicules();
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_vehiculeSelectionne == null) return;

            if (_vehiculeSelectionne.Statut == "loue")
            {
                MessageBox.Show("Impossible de supprimer un véhicule actuellement loué.",
                    "Action impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Supprimer le véhicule {_vehiculeSelectionne.Marque} {_vehiculeSelectionne.Modele} ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM vehicules WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _vehiculeSelectionne.Id);
                cmd.ExecuteNonQuery();

                PanelFiche.Visibility = Visibility.Collapsed;
                TxtSelectionner.Visibility = Visibility.Visible;
                _vehiculeSelectionne = null;
                ChargerVehicules();
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
                ChargerVehicules();
        }
    }

    // ── Modèle ──────────────────────────────────────────────────
    public class VehiculeModel
    {
        public int Id { get; set; }
        public string Immatriculation { get; set; } = "";
        public string Marque { get; set; } = "";
        public string Modele { get; set; } = "";
        public int Annee { get; set; }
        public int KmActuel { get; set; }
        public string Statut { get; set; } = "";
        public string Categorie { get; set; } = "";
        public decimal TarifJour { get; set; }

        public string StatutLabel => Statut switch
        {
            "disponible" => "✅ Disponible",
            "loue" => "🔑 Loué",
            "maintenance" => "🔧 Maintenance",
            "hors_service" => "❌ Hors service",
            _ => Statut
        };
    }
}
