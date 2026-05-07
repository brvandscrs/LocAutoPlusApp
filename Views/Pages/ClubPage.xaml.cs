using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    /// <summary>
    /// Logique d'interaction pour ClubPage.xaml
    /// </summary>
    public partial class ClubPage : Page
    {
        public ClubPage()
        {
            InitializeComponent();
            Loaded += (s, e) => ChargerDonnees();
        }

        private void ChargerDonnees()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // ── Cartes niveaux ──────────────────────────────
                var cmdNiveaux = new MySqlCommand(@"
                    SELECT n.nom, n.points_min, n.reduction_pct,
                           COUNT(cm.id) AS nb_membres
                    FROM niveaux_club n
                    LEFT JOIN club_membres cm ON cm.niveau_id = n.id AND cm.actif = 1
                    GROUP BY n.id
                    ORDER BY n.points_min", conn);

                using (var reader = cmdNiveaux.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var nom = reader.GetString("nom");
                        var reduction = reader.GetDecimal("reduction_pct");
                        var nbMembres = reader.GetInt32("nb_membres");
                        var label = $"{reduction}% de réduction";
                        var nbLabel = $"{nbMembres} membre(s)";

                        switch (nom)
                        {
                            case "Bronze":
                                TxtBronzeReduction.Text = label;
                                TxtBronzeNb.Text = nbLabel;
                                break;
                            case "Silver":
                                TxtSilverReduction.Text = label;
                                TxtSilverNb.Text = nbLabel;
                                break;
                            case "Gold":
                                TxtGoldReduction.Text = label;
                                TxtGoldNb.Text = nbLabel;
                                break;
                        }
                    }
                }

                // ── Liste membres ───────────────────────────────
                var membres = new List<MembreModel>();
                var cmdMembres = new MySqlCommand(@"
                    SELECT cm.id, cm.points_total, cm.date_adhesion, cm.actif,
                           CONCAT(u.prenom, ' ', u.nom) AS nom_complet,
                           u.email,
                           n.nom AS niveau, n.reduction_pct,
                           u.id AS user_id
                    FROM club_membres cm
                    JOIN users u        ON cm.user_id  = u.id
                    JOIN niveaux_club n ON cm.niveau_id = n.id
                    ORDER BY cm.points_total DESC", conn);

                using (var reader = cmdMembres.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        membres.Add(new MembreModel
                        {
                            Id = reader.GetInt32("id"),
                            UserId = reader.GetInt32("user_id"),
                            NomComplet = reader.GetString("nom_complet"),
                            Email = reader.GetString("email"),
                            PointsTotal = reader.GetInt32("points_total"),
                            Niveau = reader.GetString("niveau"),
                            ReductionPct = reader.GetDecimal("reduction_pct"),
                            DateAdhesion = reader.GetDateTime("date_adhesion").ToString("dd/MM/yyyy"),
                            Actif = reader.GetBoolean("actif"),
                        });
                    }
                }

                DgMembres.ItemsSource = membres;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgMembres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgMembres.SelectedItem is not MembreModel m) return;

            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Visible;

            // Initiales et nom
            var parts = m.NomComplet.Split(' ');
            TxtInitiales.Text = parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : m.NomComplet[..1].ToUpper();
            TxtNomMembre.Text = m.NomComplet;

            // Badge niveau
            var (bg, fg) = m.Niveau switch
            {
                "Gold" => ("#C9A84C", "#0F0F0F"),
                "Silver" => ("#A8A9AD", "#FFFFFF"),
                _ => ("#CD7F32", "#FFFFFF")
            };
            BadgeNiveau.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg));
            TxtNiveauBadge.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg));
            TxtNiveauBadge.Text = $"⭐ {m.Niveau} — {m.ReductionPct}% de réduction";

            // Barre de progression
            int prochainSeuil = m.Niveau switch
            {
                "Bronze" => 500,
                "Silver" => 1500,
                _ => m.PointsTotal
            };

            int seuilActuel = m.Niveau switch
            {
                "Bronze" => 0,
                "Silver" => 500,
                _ => 1500
            };

            TxtPointsLabel.Text = $"{m.PointsTotal} pts";

            if (m.Niveau == "Gold")
            {
                TxtProchainNiveau.Text = "Niveau max 🏆";
                BarreProgression.Width = 200;
            }
            else
            {
                int restant = prochainSeuil - m.PointsTotal;
                TxtProchainNiveau.Text = $"{restant} pts → {(m.Niveau == "Bronze" ? "Silver" : "Gold")}";
                double pct = Math.Min(1.0, (double)(m.PointsTotal - seuilActuel) / (prochainSeuil - seuilActuel));
                BarreProgression.Width = pct * 200;
            }

            // Infos
            ListInfos.ItemsSource = new[]
            {
                new { Label = "Email",      Valeur = m.Email },
                new { Label = "Membre dep.", Valeur = m.DateAdhesion },
                new { Label = "Statut",     Valeur = m.Actif ? "✅ Actif" : "❌ Inactif" },
            };

            // Historique points
            ChargerHistorique(m.UserId);
        }

        private void ChargerHistorique(int userId)
        {
            try
            {
                var historique = new List<dynamic>();

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT points, motif,
                           DATE_FORMAT(created_at, '%d/%m/%Y') AS date
                    FROM historique_points
                    WHERE user_id = @userId
                    ORDER BY created_at DESC
                    LIMIT 10", conn);

                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    historique.Add(new
                    {
                        Motif = reader.GetString("motif"),
                        Date = reader.GetString("date"),
                        PointsLabel = $"+{reader.GetInt32("points")} pts"
                    });
                }

                if (historique.Count > 0)
                    ListHistorique.ItemsSource = historique;
                else
                    ListHistorique.ItemsSource = new[] { new { Motif = "Aucun historique", Date = "", PointsLabel = "" } };
            }
            catch { }
        }
    }

    public class MembreModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string NomComplet { get; set; } = "";
        public string Email { get; set; } = "";
        public int PointsTotal { get; set; }
        public string Niveau { get; set; } = "";
        public decimal ReductionPct { get; set; }
        public string DateAdhesion { get; set; } = "";
        public bool Actif { get; set; }

        public string ReductionLabel => $"{ReductionPct}%";
    }
}
