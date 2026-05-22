using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views.Pages
{
    /// <summary>
    /// Logique d'interaction pour ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        private List<ClientModel> _tousLesClients = new();
        private ClientModel? _clientSelectionne;

        public ClientsPage()
        {
            InitializeComponent();
            Loaded += (s, e) => ChargerClients();
        }

        // ── Chargement ─────────────────────────────────────────
        private void ChargerClients()
        {
            try
            {
                _tousLesClients.Clear();

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT u.id, u.nom, u.prenom, u.email,
                           u.telephone, u.adresse, u.date_naissance,
                           u.created_at,
                           COUNT(DISTINCT c.id)    AS nb_contrats,
                           cm.actif                AS club_actif,
                           n.nom                   AS niveau_club,
                           cm.points_total
                    FROM users u
                    LEFT JOIN contrats c     ON c.user_id = u.id
                    LEFT JOIN club_membres cm ON cm.user_id = u.id
                    LEFT JOIN niveaux_club n  ON cm.niveau_id = n.id
                    GROUP BY u.id
                    ORDER BY u.nom, u.prenom", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _tousLesClients.Add(new ClientModel
                    {
                        Id = reader.GetInt32("id"),
                        Nom = reader.GetString("nom"),
                        Prenom = reader.GetString("prenom"),
                        Email = reader.GetString("email"),
                        Telephone = reader.IsDBNull(reader.GetOrdinal("telephone")) ? "—" : reader.GetString("telephone"),
                        Adresse = reader.IsDBNull(reader.GetOrdinal("adresse")) ? "—" : reader.GetString("adresse"),
                        DateNaissance = reader.IsDBNull(reader.GetOrdinal("date_naissance")) ? "—" :
                                        reader.GetDateTime("date_naissance").ToString("dd/MM/yyyy"),
                        DateInscription = reader.GetDateTime("created_at").ToString("dd/MM/yyyy"),
                        NbContrats = reader.GetInt32("nb_contrats"),
                        ClubActif = !reader.IsDBNull(reader.GetOrdinal("club_actif")) && reader.GetBoolean("club_actif"),
                        NiveauClub = reader.IsDBNull(reader.GetOrdinal("niveau_club")) ? null : reader.GetString("niveau_club"),
                        PointsClub = reader.IsDBNull(reader.GetOrdinal("points_total")) ? 0 : reader.GetInt32("points_total"),
                    });
                }

                DgClients.ItemsSource = _tousLesClients;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Recherche ───────────────────────────────────────────
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

        // ── Sélection d'un client ───────────────────────────────
        private void DgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgClients.SelectedItem is not ClientModel client) return;

            _clientSelectionne = client;
            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelFiche.Visibility = Visibility.Visible;

            // Initiales
            TxtInitiales.Text = $"{client.Prenom[0]}{client.Nom[0]}".ToUpper();
            TxtFicheNom.Text = client.NomComplet;

            // Badge club
            if (client.ClubActif && client.NiveauClub != null)
            {
                TxtFicheClub.Text = $"⭐ Club {client.NiveauClub} — {client.PointsClub} pts";
                BadgeClub.Visibility = Visibility.Visible;
            }
            else
            {
                BadgeClub.Visibility = Visibility.Collapsed;
            }

            // Infos
            ListInfos.ItemsSource = new[]
            {
                new { Label = "Email",       Valeur = client.Email },
                new { Label = "Téléphone",   Valeur = client.Telephone },
                new { Label = "Adresse",     Valeur = client.Adresse },
                new { Label = "Naissance",   Valeur = client.DateNaissance },
                new { Label = "Inscrit le",  Valeur = client.DateInscription },
                new { Label = "Contrats",    Valeur = $"{client.NbContrats} contrat(s)" },
            };
        }

        // ── Boutons actions ─────────────────────────────────────
        private void BtnVoirContrats_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;
            var page = new ContratsPage(_clientSelectionne.Id, _clientSelectionne.NomComplet);
            NavigationService?.Navigate(page);
        }

        private void BtnModifierClient_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;
            var dialog = new ClientEditWindow(_clientSelectionne);
            if (dialog.ShowDialog() == true)
                ChargerClients();
        }

        private void BtnSupprimerClient_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;

            var confirm = MessageBox.Show(
                $"Supprimer le client {_clientSelectionne.NomComplet} ?\nSes contrats seront également supprimés.",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM users WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _clientSelectionne.Id);
                cmd.ExecuteNonQuery();

                PanelFiche.Visibility = Visibility.Collapsed;
                TxtSelectionner.Visibility = Visibility.Visible;
                _clientSelectionne = null;
                ChargerClients();
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
                ChargerClients();
        }
    }

    // ── Modèle ──────────────────────────────────────────────────
    public class ClientModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telephone { get; set; } = "—";
        public string Adresse { get; set; } = "—";
        public string DateNaissance { get; set; } = "—";
        public string DateInscription { get; set; } = "";
        public int NbContrats { get; set; }
        public bool ClubActif { get; set; }
        public string? NiveauClub { get; set; }
        public int PointsClub { get; set; }

        public string NomComplet => $"{Prenom} {Nom}";
        public string StatutClub => ClubActif && NiveauClub != null ? $"⭐ {NiveauClub}" : "—";
    }
}
