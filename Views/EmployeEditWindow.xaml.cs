using LocAutoPlusApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    public partial class EmployeEditWindow : Window
    {
        private readonly EmployeDto? _employe;
        private readonly bool _isEdit;
        private readonly ApiService _api = new();

        public EmployeEditWindow(EmployeDto? employe)
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
                AfficherErreur("Le mot de passe est obligatoire pour un nouvel employé.");
                return;
            }

            var role = (CbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "agent";

            try
            {
                ApiResponse? result;

                if (_isEdit)
                {
                    var payload = new
                    {
                        nom = TxtNom.Text.Trim(),
                        prenom = TxtPrenom.Text.Trim(),
                        email = TxtEmail.Text.Trim(),
                        role,
                        password = string.IsNullOrWhiteSpace(TxtNewPassword.Password)
                                   ? null : TxtNewPassword.Password,
                    };
                    result = await _api.UpdateEmployeAsync(_employe!.Id, payload);
                }
                else
                {
                    var payload = new
                    {
                        nom = TxtNom.Text.Trim(),
                        prenom = TxtPrenom.Text.Trim(),
                        email = TxtEmail.Text.Trim(),
                        role,
                        password = TxtPassword.Password,
                    };
                    result = await _api.CreateEmployeAsync(payload);
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
