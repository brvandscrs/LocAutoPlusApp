using LocAutoPlusApp.Helpers;
using MySqlConnector;
using System.Windows;

namespace LocAutoPlusApp.Views
{
    /// <summary>
    /// Logique d'interaction pour ContratDemarrerWindow.xaml
    /// </summary>
    public partial class ContratDemarrerWindow : Window
    {
        private readonly int _contratId;
        public int KmDepart { get; private set; }

        public ContratDemarrerWindow(int contratId, int kmActuel)
        {
            InitializeComponent();
            _contratId = contratId;
            TxtKmDepart.Text = kmActuel.ToString();
        }

        private void BtnDemarrer_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtKmDepart.Text, out int km) || km < 0)
            {
                TxtErreur.Text = "Veuillez saisir un kilométrage valide.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                // Met à jour le contrat
                var cmd = new MySqlCommand(@"
                    UPDATE contrats SET
                        statut     = 'en_cours',
                        km_depart  = @km,
                        employe_id = @employeId
                    WHERE id = @id", conn);

                cmd.Parameters.AddWithValue("@km", km);
                cmd.Parameters.AddWithValue("@employeId", AppSession.EmployeId);
                cmd.Parameters.AddWithValue("@id", _contratId);
                cmd.ExecuteNonQuery();

                // Récupère le véhicule_id et le marque comme loué
                var cmdV = new MySqlCommand(
                    "UPDATE vehicules SET statut = 'loue' WHERE id = (SELECT vehicule_id FROM contrats WHERE id = @id)",
                    conn);
                cmdV.Parameters.AddWithValue("@id", _contratId);
                cmdV.ExecuteNonQuery();

                KmDepart = km;
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
}
