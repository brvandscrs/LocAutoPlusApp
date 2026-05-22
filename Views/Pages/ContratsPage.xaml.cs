using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Services;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    /// <summary>
    /// Logique d'interaction pour ContratsPage.xaml
    /// </summary>
    public partial class ContratsPage : Page
    {
        private List<ContratModel> _tousLesContrats = new();
        private ContratModel? _contratSelectionne;
        private readonly int? _filtreClientId;
        private readonly ApiService _api = new();

        public ContratsPage()
        {
            InitializeComponent();
            Loaded += (s, e) => ChargerContrats();
        }

        public ContratsPage(int clientId, string nomClient)
        {
            InitializeComponent();
            _filtreClientId = clientId;
            Loaded += (s, e) =>
            {
                TxtFiltreClient.Text = $"📌 Contrats de {nomClient}";
                TxtFiltreClient.Visibility = Visibility.Visible;
                ChargerContrats();
            };
        }

        // ── Chargement ──────────────────────────────────────────
        private void ChargerContrats()
        {
            try
            {
                _tousLesContrats.Clear();

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var sql = @"
                    SELECT c.id,
                           CONCAT(u.prenom, ' ', u.nom)   AS client,
                           CONCAT(v.marque, ' ', v.modele) AS vehicule,
                           v.immatriculation,
                           DATE_FORMAT(c.date_debut,      '%d/%m/%Y') AS date_debut,
                           DATE_FORMAT(c.date_fin_prevue, '%d/%m/%Y') AS date_fin,
                           DATE_FORMAT(c.date_fin_reelle, '%d/%m/%Y') AS date_fin_reelle,
                           c.montant_base,
                           c.reduction_appliquee,
                           c.montant_total,
                           c.statut,
                           c.notes,
                           c.km_depart, c.km_retour,
                           DATE_FORMAT(c.date_reservation, '%d/%m/%Y à %H:%i') AS date_resa,
                           CONCAT(e.prenom, ' ', e.nom) AS employe,
                           c.user_id, c.vehicule_id
                    FROM contrats c
                    JOIN users u      ON c.user_id     = u.id
                    JOIN vehicules v  ON c.vehicule_id = v.id
                    LEFT JOIN employes e ON c.employe_id = e.id";

                if (_filtreClientId.HasValue)
                    sql += " WHERE c.user_id = @clientId";

                sql += " ORDER BY c.created_at DESC";

                var cmd = new MySqlCommand(sql, conn);
                if (_filtreClientId.HasValue)
                    cmd.Parameters.AddWithValue("@clientId", _filtreClientId.Value);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _tousLesContrats.Add(new ContratModel
                    {
                        Id = reader.GetInt32("id"),
                        Client = reader.GetString("client"),
                        Vehicule = reader.GetString("vehicule"),
                        Immatriculation = reader.GetString("immatriculation"),
                        DateDebut = reader.GetString("date_debut"),
                        DateFin = reader.GetString("date_fin"),
                        DateFinReelle = reader.IsDBNull(reader.GetOrdinal("date_fin_reelle")) ? "—" : reader.GetString("date_fin_reelle"),
                        MontantBase = reader.GetDecimal("montant_base"),
                        Reduction = reader.GetDecimal("reduction_appliquee"),
                        MontantTotal = reader.GetDecimal("montant_total"),
                        Statut = reader.GetString("statut"),
                        Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? "" : reader.GetString("notes"),
                        KmDepart = reader.IsDBNull(reader.GetOrdinal("km_depart")) ? 0 : reader.GetInt32("km_depart"),
                        KmRetour = reader.IsDBNull(reader.GetOrdinal("km_retour")) ? 0 : reader.GetInt32("km_retour"),
                        DateReservation = reader.GetString("date_resa"),
                        Employe = reader.IsDBNull(reader.GetOrdinal("employe")) ? "—" : reader.GetString("employe"),
                        UserId = reader.GetInt32("user_id"),
                        VehiculeId = reader.GetInt32("vehicule_id"),
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

        // ── Sélection ───────────────────────────────────────────
        private void DgContrats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgContrats.SelectedItem is not ContratModel c) return;

            _contratSelectionne = c;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Visible;

            TxtDetailId.Text = $"Contrat #{c.Id}";

            // Badge statut
            var (bg, fg, label) = c.Statut switch
            {
                "en_attente" => ("#FFF3CD", "#856404", "En attente"),
                "confirmee" => ("#CCE5FF", "#004085", "Confirmée"),
                "en_cours" => ("#D4EDDA", "#155724", "En cours"),
                "terminee" => ("#E2E3E5", "#383D41", "Terminée"),
                "annulee" => ("#FEE2E2", "#C0392B", "Annulée"),
                _ => ("#E2E3E5", "#383D41", c.Statut)
            };

            BadgeStatut.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg));
            TxtDetailStatut.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg));
            TxtDetailStatut.Text = label;

            // Infos détail
            ListDetails.ItemsSource = new[]
            {
                new { Label = "Client",      Valeur = c.Client,          Gras = "Normal" },
                new { Label = "Véhicule",    Valeur = $"{c.Vehicule} ({c.Immatriculation})", Gras = "Normal" },
                new { Label = "Réservé le",  Valeur = c.DateReservation, Gras = "Normal" },
                new { Label = "Début",       Valeur = c.DateDebut,       Gras = "SemiBold" },
                new { Label = "Fin prévue",  Valeur = c.DateFin,         Gras = "SemiBold" },
                new { Label = "Fin réelle",  Valeur = c.DateFinReelle,   Gras = "Normal" },
                new { Label = "Km départ",   Valeur = c.KmDepart > 0 ? $"{c.KmDepart:N0} km" : "—", Gras = "Normal" },
                new { Label = "Km retour",   Valeur = c.KmRetour > 0 ? $"{c.KmRetour:N0} km" : "—", Gras = "Normal" },
                new { Label = "Base",        Valeur = $"{c.MontantBase:F2} €",   Gras = "Normal" },
                new { Label = "Réduction",   Valeur = $"{c.Reduction} %",        Gras = "Normal" },
                new { Label = "Total",       Valeur = $"{c.MontantTotal:F2} €",  Gras = "Bold" },
                new { Label = "Employé",     Valeur = c.Employe,         Gras = "Normal" },
            };

            // Boutons selon statut
            BtnConfirmer.Visibility = c.Statut == "en_attente" ? Visibility.Visible : Visibility.Collapsed;
            BtnDemarrer.Visibility = c.Statut == "confirmee" ? Visibility.Visible : Visibility.Collapsed;
            BtnCloturer.Visibility = c.Statut == "en_cours" ? Visibility.Visible : Visibility.Collapsed;
            BtnAnnuler.Visibility = c.Statut is "en_attente" or "confirmee"
                                      ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Actions ─────────────────────────────────────────────
        private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
            => ChangerStatut(_contratSelectionne!.Id, "confirmee");

        private void BtnDemarrer_Click(object sender, RoutedEventArgs e)
        {
            if (_contratSelectionne == null) return;

            var dlg = new ContratDemarrerWindow(_contratSelectionne.Id,
                                                _contratSelectionne.KmDepart);
            if (dlg.ShowDialog() == true)
                ChargerContrats();
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

                if (result == null || !result.Success)
                {
                    MessageBox.Show(result?.Message ?? "Erreur lors de la clôture.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show(
                    $"Contrat clôturé ✅\n{result.Message}\nPoints attribués : {dlg.KmRetour}",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                ChargerContrats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur API : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
            => ChangerStatut(_contratSelectionne!.Id, "annulee");

        private void ChangerStatut(int contratId, string statut)
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
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE contrats SET statut = @statut WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@statut", statut);
                cmd.Parameters.AddWithValue("@id", contratId);
                cmd.ExecuteNonQuery();

                // Si confirmée → marquer le véhicule comme loué
                if (statut == "en_cours")
                {
                    var cmdV = new MySqlCommand(
                        "UPDATE vehicules SET statut = 'loue' WHERE id = @id", conn);
                    cmdV.Parameters.AddWithValue("@id", _contratSelectionne!.VehiculeId);
                    cmdV.ExecuteNonQuery();
                }

                ChargerContrats();
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
                ChargerContrats();
        }
    }

    // ── Modèle ──────────────────────────────────────────────────
    public class ContratModel
    {
        public int     Id              { get; set; }
        public string  Client          { get; set; } = "";
        public string  Vehicule        { get; set; } = "";
        public string  Immatriculation { get; set; } = "";
        public string  DateDebut       { get; set; } = "";
        public string  DateFin         { get; set; } = "";
        public string  DateFinReelle   { get; set; } = "—";
        public string  DateReservation { get; set; } = "";
        public decimal MontantBase     { get; set; }
        public decimal Reduction       { get; set; }
        public decimal MontantTotal    { get; set; }
        public string  Statut          { get; set; } = "";
        public string  Notes           { get; set; } = "";
        public int     KmDepart        { get; set; }
        public int     KmRetour        { get; set; }
        public string  Employe         { get; set; } = "—";
        public int     UserId          { get; set; }
        public int     VehiculeId      { get; set; }

        public string Montant => $"{MontantTotal:F2} €";
    }
}
