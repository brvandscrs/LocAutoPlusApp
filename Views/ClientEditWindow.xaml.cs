using LocAutoPlusApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    public partial class ClientEditWindow : Window
    {
        private readonly ClientDto? _client;
        private readonly bool _isEdit;
        private readonly ApiService _api = new();

        public ClientEditWindow(ClientDto? client)
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
                TxtCodePostal.Text = client.CodePostal == "—" ? "" : client.CodePostal;
                TxtVille.Text = client.Ville == "—" ? "" : client.Ville;
                PanelPassword.Visibility = Visibility.Collapsed;

                if (client.DateNaissance != "—" &&
                    DateTime.TryParse(client.DateNaissance, out var dt))
                    DpNaissance.SelectedDate = dt;
            }
        }

        private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
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
                AfficherErreur("Le mot de passe est obligatoire pour un nouveau client.");
                return;
            }

            try
            {
                ApiResponse? result;

                if (_isEdit)
                {
                    result = await _api.UpdateClientAsync(_client!.Id, new
                    {
                        nom = TxtNom.Text.Trim(),
                        prenom = TxtPrenom.Text.Trim(),
                        email = TxtEmail.Text.Trim(),
                        telephone = TxtTelephone.Text.Trim(),
                        adresse = TxtAdresse.Text.Trim(),
                        code_postal = TxtCodePostal.Text.Trim(),
                        ville = TxtVille.Text.Trim(),
                        date_naissance = DpNaissance.SelectedDate.HasValue
                            ? DpNaissance.SelectedDate.Value.ToString("yyyy-MM-dd")
                            : (string?)null,
                    });
                }
                else
                {
                    result = await _api.CreateClientAsync(new
                    {
                        nom = TxtNom.Text.Trim(),
                        prenom = TxtPrenom.Text.Trim(),
                        email = TxtEmail.Text.Trim(),
                        password = TxtPassword.Password,
                        telephone = TxtTelephone.Text.Trim(),
                        adresse = TxtAdresse.Text.Trim(),
                        code_postal = TxtCodePostal.Text.Trim(),
                        ville = TxtVille.Text.Trim(),
                        date_naissance = DpNaissance.SelectedDate.HasValue
                            ? DpNaissance.SelectedDate.Value.ToString("yyyy-MM-dd")
                            : (string?)null,
                    });
                }

                if (result?.Success == true)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    AfficherErreur(result?.Message ?? "Erreur lors de l'enregistrement.");
                }
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
