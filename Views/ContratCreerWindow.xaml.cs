using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    /// <summary>
    /// Logique d'interaction pour ContratCreerWindow.xaml
    /// </summary>
    public partial class ContratCreerWindow : Window
    {
        private decimal _tarifJour = 0;
        private decimal _reductionPct = 0;
        private int _clientId = 0;

        public ContratCreerWindow()
        {
            InitializeComponent();
            DpDebut.SelectedDate = DateTime.Today;
            DpFin.SelectedDate = DateTime.Today.AddDays(1);
            ChargerDonnees();
        }

        private void ChargerDonnees()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Clients
                var clients = new List<ClientItem>();
                var cmdC = new MySqlCommand(
                    "SELECT id, CONCAT(prenom, ' ', nom) AS nom_complet FROM users ORDER BY nom", conn);
                using (var r = cmdC.ExecuteReader())
                    while (r.Read())
                        clients.Add(new ClientItem
                        {
                            Id = r.GetInt32("id"),
                            NomComplet = r.GetString("nom_complet")
                        });
                CbClient.ItemsSource = clients;

                // Véhicules disponibles
                var vehicules = new List<VehiculeItem>();
                var cmdV = new MySqlCommand(@"
                    SELECT v.id,
                    CONCAT(v.marque, ' ', v.modele, ' (', v.immatriculation, ') — ', c.tarif_base_jour, ' €/j') AS label,
                    c.tarif_base_jour
                    FROM vehicules v
                    JOIN categories_vehicules c ON v.categorie_id = c.id
                    WHERE v.statut = 'disponible'
                    ORDER BY v.marque", conn);
                using (var r = cmdV.ExecuteReader())
                    while (r.Read())
                        vehicules.Add(new VehiculeItem
                        {
                            Id = r.GetInt32("id"),
                            Label = r.GetString("label"),
                            TarifJour = r.GetDecimal("tarif_base_jour")
                        });
                CbVehicule.ItemsSource = vehicules;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement : " + ex.Message);
            }
        }

        private void CbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbClient.SelectedItem is not ClientItem client) return;
            _clientId = client.Id;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(@"
                    SELECT n.reduction_pct
                    FROM club_membres cm
                    JOIN niveaux_club n ON cm.niveau_id = n.id
                    WHERE cm.user_id = @id AND cm.actif = 1", conn);
                cmd.Parameters.AddWithValue("@id", _clientId);
                var result = cmd.ExecuteScalar();
                _reductionPct = result != null ? Convert.ToDecimal(result) : 0;
            }
            catch { _reductionPct = 0; }

            CalculerMontant();
        }

        private void CbVehicule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbVehicule.SelectedItem is not VehiculeItem v) return;
            _tarifJour = v.TarifJour;
            CalculerMontant();
        }

        private void Dates_Changed(object? sender, SelectionChangedEventArgs e)
            => CalculerMontant();

        private void CalculerMontant()
        {
            if (_tarifJour == 0 || !DpDebut.SelectedDate.HasValue || !DpFin.SelectedDate.HasValue)
                return;

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

        private void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            if (CbClient.SelectedItem == null || CbVehicule.SelectedItem == null ||
                !DpDebut.SelectedDate.HasValue || !DpFin.SelectedDate.HasValue)
            {
                TxtErreur.Text = "Veuillez remplir tous les champs obligatoires.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            if (CbVehicule.SelectedItem is not VehiculeItem vehicule) return;

            var debut = DpDebut.SelectedDate.Value;
            var fin = DpFin.SelectedDate.Value;
            var jours = (fin - debut).Days;

            if (jours <= 0)
            {
                TxtErreur.Text = "La date de fin doit être après la date de début.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            var montantBase = jours * _tarifJour;
            var montantTotal = montantBase - (montantBase * _reductionPct / 100);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(@"
                    INSERT INTO contrats
                        (user_id, vehicule_id, employe_id, date_reservation,
                         date_debut, date_fin_prevue, montant_base,
                         reduction_appliquee, montant_total, statut,
                         created_at, updated_at)
                    VALUES
                        (@userId, @vehiculeId, @employeId, NOW(),
                         @debut, @fin, @base,
                         @reduction, @total, 'en_attente',
                         NOW(), NOW())", conn);

                cmd.Parameters.AddWithValue("@userId", _clientId);
                cmd.Parameters.AddWithValue("@vehiculeId", vehicule.Id);
                cmd.Parameters.AddWithValue("@employeId", AppSession.EmployeId);
                cmd.Parameters.AddWithValue("@debut", debut.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fin", fin.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@base", montantBase);
                cmd.Parameters.AddWithValue("@reduction", _reductionPct);
                cmd.Parameters.AddWithValue("@total", montantTotal);
                cmd.ExecuteNonQuery();

                DialogResult = true;
                Close();
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

    public class ClientItem
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = "";
    }

    public class VehiculeItem
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public decimal TarifJour { get; set; }
    }
}
