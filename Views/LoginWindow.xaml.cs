using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Services;
using System.Windows;

namespace LocAutoPlusApp.Views
{
    /// <summary>
    /// Logique d'interaction pour LoginWindow.xaml
    /// </summary>
    
    public partial class LoginWindow : Window
    {
        //public LoginWindow()
        //{
        //    InitializeComponent();
        //}

        private readonly ApiService _api = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnConnexion_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(TxtEmail.Text) ||
                string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                AfficherErreur("Veuillez renseigner votre e-mail et mot de passe.");
                return;
            }

            // UI en chargement
            BtnConnexion.IsEnabled = false;
            TxtChargement.Visibility = Visibility.Visible;
            TxtErreur.Visibility = Visibility.Collapsed;

            try
            {
                var result = await _api.LoginAsync(TxtEmail.Text.Trim(), TxtPassword.Password);

                if (result == null || !result.Success)
                {
                    AfficherErreur(result?.Message ?? "Identifiants incorrects.");
                    return;
                }

                // Sauvegarde session
                AppSession.Token = result.Token;
                AppSession.EmployeId = result.Employe!.Id;
                AppSession.Nom = result.Employe.Nom;
                AppSession.Prenom = result.Employe.Prenom;
                AppSession.Email = result.Employe.Email;
                AppSession.Role = result.Employe.Role;

                // Ouverture fenêtre principale
                var main = new MainAppWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                AfficherErreur("Impossible de joindre le serveur. Vérifiez que Laravel est démarré.\n" + ex.Message);
            }
            finally
            {
                BtnConnexion.IsEnabled = true;
                TxtChargement.Visibility = Visibility.Collapsed;
            }
        }

        private void AfficherErreur(string message)
        {
            TxtErreur.Text = message;
            TxtErreur.Visibility = Visibility.Visible;
        }
    }
}
