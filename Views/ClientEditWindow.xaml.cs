using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Views.Pages;
using MySqlConnector;
using System.Windows;

namespace LocAutoPlusApp.Views
{
    /// <summary>
    /// Logique d'interaction pour ClientEditWindow.xaml
    /// </summary>
    public partial class ClientEditWindow : Window
    {
        private readonly ClientModel? _client;
        private readonly bool _isEdit;

        public ClientEditWindow(ClientModel? client)
        {
            InitializeComponent();
            _client = client;
            _isEdit = client != null;

            if (_isEdit)
            {
                TxtTitre.Text = "Modifier le client";
                TxtNom.Text = client!.Nom;
                TxtPrenom.Text = client.Prenom;
                TxtEmail.Text = client.Email;
                TxtTelephone.Text = client.Telephone == "—" ? "" : client.Telephone;
                TxtAdresse.Text = client.Adresse == "—" ? "" : client.Adresse;
                PanelPassword.Visibility = Visibility.Collapsed;

                if (client.DateNaissance != "—" &&
                    DateTime.TryParse(client.DateNaissance, out var dt))
                    DpNaissance.SelectedDate = dt;
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(TxtNom.Text) ||
                string.IsNullOrWhiteSpace(TxtPrenom.Text) ||
                string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                AfficherErreur("Les champs Nom, Prénom et Email sont obligatoires.");
                return;
            }

            if (!_isEdit && string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                AfficherErreur("Le mot de passe est obligatoire pour un nouveau client.");
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                if (_isEdit)
                {
                    var cmd = new MySqlCommand(@"
                        UPDATE users SET
                            nom           = @nom,
                            prenom        = @prenom,
                            email         = @email,
                            telephone     = @telephone,
                            adresse       = @adresse,
                            date_naissance = @naissance
                        WHERE id = @id", conn);

                    cmd.Parameters.AddWithValue("@nom", TxtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@prenom", TxtPrenom.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", TxtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@telephone", TxtTelephone.Text.Trim());
                    cmd.Parameters.AddWithValue("@adresse", TxtAdresse.Text.Trim());
                    cmd.Parameters.AddWithValue("@naissance", DpNaissance.SelectedDate.HasValue
                        ? DpNaissance.SelectedDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", _client!.Id);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Hash du mot de passe bcrypt
                    var hash = BCrypt.Net.BCrypt.HashPassword(TxtPassword.Password);

                    var cmd = new MySqlCommand(@"
                        INSERT INTO users
                            (nom, prenom, email, password, telephone, adresse, date_naissance, created_at, updated_at)
                        VALUES
                            (@nom, @prenom, @email, @password, @telephone, @adresse, @naissance, NOW(), NOW())", conn);

                    cmd.Parameters.AddWithValue("@nom", TxtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@prenom", TxtPrenom.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", TxtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@password", hash);
                    cmd.Parameters.AddWithValue("@telephone", TxtTelephone.Text.Trim());
                    cmd.Parameters.AddWithValue("@adresse", TxtAdresse.Text.Trim());
                    cmd.Parameters.AddWithValue("@naissance", DpNaissance.SelectedDate.HasValue
                        ? DpNaissance.SelectedDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur : " + ex.Message);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AfficherErreur(string msg)
        {
            TxtErreur.Text = msg;
            TxtErreur.Visibility = Visibility.Visible;
        }
    }
}
