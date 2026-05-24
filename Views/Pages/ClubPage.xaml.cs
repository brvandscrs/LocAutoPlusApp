using LocAutoPlusApp.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocAutoPlusApp.Views.Pages
{
    public partial class ClubPage : Page
    {
        private readonly ApiService _api = new();

        public ClubPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ChargerDonnees();
        }

        private async Task ChargerDonnees()
        {
            try
            {
                // Niveaux
                var niveaux = await _api.GetNiveauxClubAsync();
                foreach (var n in niveaux)
                {
                    var label = $"{n.ReductionPct}% de réduction";
                    var nb = $"{n.NbMembres} membre(s)";
                    switch (n.Nom)
                    {
                        case "Bronze":
                            TxtBronzeReduction.Text = label;
                            TxtBronzeNb.Text = nb; break;
                        case "Silver":
                            TxtSilverReduction.Text = label;
                            TxtSilverNb.Text = nb; break;
                        case "Gold":
                            TxtGoldReduction.Text = label;
                            TxtGoldNb.Text = nb; break;
                    }
                }

                // Membres
                var membres = await _api.GetMembresClubAsync();
                DgMembres.ItemsSource = membres;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DgMembres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgMembres.SelectedItem is not MembreDto m) return;

            TxtSelectionner.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Visible;

            var parts = m.NomComplet.Split(' ');
            TxtInitiales.Text = parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : m.NomComplet[..1].ToUpper();
            TxtNomMembre.Text = m.NomComplet;

            var (bg, fg) = m.Niveau switch
            {
                "Gold" => ("#C9A84C", "#0F0F0F"),
                "Silver" => ("#A8A9AD", "#FFFFFF"),
                _ => ("#CD7F32", "#FFFFFF")
            };
            BadgeNiveau.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(bg));
            TxtNiveauBadge.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(fg));
            TxtNiveauBadge.Text = $"⭐ {m.Niveau} — {m.ReductionPct}% de réduction";

            // Barre progression
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
                double pct = Math.Min(1.0,
                    (double)(m.PointsTotal - seuilActuel) / (prochainSeuil - seuilActuel));
                BarreProgression.Width = pct * 200;
            }

            ListInfos.ItemsSource = new[]
            {
                new { Label = "Email",       Valeur = m.Email },
                new { Label = "Membre dep.", Valeur = m.DateAdhesion },
                new { Label = "Statut",      Valeur = m.Actif ? "✅ Actif" : "❌ Inactif" },
            };

            // Historique via API
            try
            {
                var club = await _api.VerifierClubAsync(m.UserId);
                if (club?.Historique?.Count > 0)
                    ListHistorique.ItemsSource = club.Historique;
                else
                    ListHistorique.ItemsSource = new[] {
                        new HistoriqueDto { Motif = "Aucun historique", Date = "", Points = 0 }
                    };
            }
            catch { }
        }
    }
}
