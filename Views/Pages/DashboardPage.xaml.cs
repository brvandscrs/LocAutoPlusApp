using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views.Pages
{
    /// <summary>
    /// Logique d'interaction pour DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
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

                // Nombre de clients
                TxtNbClients.Text = ExecuterScalar(conn,
                    "SELECT COUNT(*) FROM users").ToString();

                // Contrats en cours
                TxtNbContratsEnCours.Text = ExecuterScalar(conn,
                    "SELECT COUNT(*) FROM contrats WHERE statut IN ('en_attente','confirmee','en_cours')").ToString();

                // Véhicules disponibles
                TxtNbVehiculesDispos.Text = ExecuterScalar(conn,
                    "SELECT COUNT(*) FROM vehicules WHERE statut = 'disponible'").ToString();

                // Membres club
                TxtNbMembresClub.Text = ExecuterScalar(conn,
                    "SELECT COUNT(*) FROM club_membres WHERE actif = 1").ToString();

                // Derniers contrats
                var contrats = new List<dynamic>();
                var cmdContrats = new MySqlCommand(@"
                    SELECT CONCAT(u.prenom, ' ', u.nom) AS client,
                           CONCAT(v.marque, ' ', v.modele) AS vehicule,
                           DATE_FORMAT(c.date_debut, '%d/%m/%Y') AS date_debut,
                           DATE_FORMAT(c.date_fin_prevue, '%d/%m/%Y') AS date_fin,
                           CONCAT(FORMAT(c.montant_total, 2), ' €') AS montant,
                           c.statut
                    FROM contrats c
                    JOIN users u ON c.user_id = u.id
                    JOIN vehicules v ON c.vehicule_id = v.id
                    ORDER BY c.created_at DESC
                    LIMIT 8", conn);

                using (var reader = cmdContrats.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contrats.Add(new
                        {
                            Client = reader.GetString("client"),
                            Vehicule = reader.GetString("vehicule"),
                            DateDebut = reader.GetString("date_debut"),
                            DateFin = reader.GetString("date_fin"),
                            Montant = reader.GetString("montant"),
                            Statut = reader.GetString("statut")
                        });
                    }
                }
                DgDerniersContrats.ItemsSource = contrats;

                // Véhicules loués
                var loues = new List<dynamic>();
                var cmdLoues = new MySqlCommand(@"
                    SELECT CONCAT(v.marque, ' ', v.modele) AS vehicule,
                           CONCAT(u.prenom, ' ', u.nom) AS client,
                           CONCAT('Retour : ', DATE_FORMAT(c.date_fin_prevue, '%d/%m/%Y')) AS date_fin
                    FROM contrats c
                    JOIN vehicules v ON c.vehicule_id = v.id
                    JOIN users u ON c.user_id = u.id
                    WHERE c.statut = 'en_cours'
                    ORDER BY c.date_fin_prevue ASC", conn);

                using (var reader = cmdLoues.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        loues.Add(new
                        {
                            Vehicule = reader.GetString("vehicule"),
                            Client = reader.GetString("client"),
                            DateFin = reader.GetString("date_fin")
                        });
                    }
                }

                if (loues.Count == 0)
                    TxtAucunLoue.Visibility = System.Windows.Visibility.Visible;
                else
                    ListVehiculesLoues.ItemsSource = loues;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Erreur de connexion à la base de données :\n" + ex.Message,
                    "Erreur", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private object ExecuterScalar(MySqlConnection conn, string sql)
        {
            using var cmd = new MySqlCommand(sql, conn);
            return cmd.ExecuteScalar() ?? 0;
        }
    }
}
