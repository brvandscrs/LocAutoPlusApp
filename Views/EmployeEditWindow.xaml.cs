using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Views.Pages;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    public partial class EmployeEditWindow : Window
    {
        private readonly EmployeModel? _employe;
        private readonly bool _isEdit;

        public EmployeEditWindow(EmployeModel? employe)
        {
            InitializeComponent();
            _employe = employe;
            _isEdit = employe != null;

            if (_isEdit)
            {
                TxtTitre.Text = "Modifier l'employé";
                TxtNom.Text = employe!.Nom;
                TxtPrenom.Text = employe.Prenom;
                TxtEmail.Text = employe.Email;
                PanelPassword.Visibility = Visibility.Collapsed;
                PanelNewPassword.Visibility = Visibility.Visible;

                foreach (ComboBoxItem item in CbRole.Items)
                    if (item.Content.ToString() == employe.Role)
                    { item.IsSelected = true; break; }
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNom.Text) ||
                string.IsNullOrWhiteSpace(TxtPrenom.Text) ||
                string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                AfficherErreur("Les champs Nom, Prénom et Email sont obligatoires.");
                return;
            }

            if (!_isEdit && string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                AfficherErreur("Le mot de passe est obligatoire pour un nouvel employé.");
                return;
            }

            var role = (CbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "agent";

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                if (_isEdit)
                {
                    var sql = @"UPDATE employes SET
                                    nom    = @nom,
                                    prenom = @prenom,
                                    email  = @email,
                                    role   = @role";

                    // Met à jour le mot de passe seulement si renseigné
                    if (!string.IsNullOrWhiteSpace(TxtNewPassword.Password))
                    {
                        sql += ", password = @password";
                    }

                    sql += " WHERE id = @id";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@nom", TxtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@prenom", TxtPrenom.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", TxtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@id", _employe!.Id);

                    if (!string.IsNullOrWhiteSpace(TxtNewPassword.Password))
                        cmd.Parameters.AddWithValue("@password",
                            BCrypt.Net.BCrypt.HashPassword(TxtNewPassword.Password));

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var hash = BCrypt.Net.BCrypt.HashPassword(TxtPassword.Password);
                    var cmd = new MySqlCommand(@"
                        INSERT INTO employes
                            (nom, prenom, email, password, role, actif, created_at, updated_at)
                        VALUES
                            (@nom, @prenom, @email, @password, @role, 1, NOW(), NOW())", conn);

                    cmd.Parameters.AddWithValue("@nom", TxtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@prenom", TxtPrenom.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", TxtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@password", hash);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.ExecuteNonQuery();
                }

                DialogResult = true;
                Close();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AfficherErreur("Cette adresse e-mail est déjà utilisée.");
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
