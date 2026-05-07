using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Views.Pages;
using System.Windows;

namespace LocAutoPlusApp.Views
{
    /// <summary>
    /// Logique d'interaction pour MainAppWindow.xaml
    /// </summary>
    public partial class MainAppWindow : Window
    {
        public MainAppWindow()
        {
            InitializeComponent();
            InitialiserInterface();
        }

        private void InitialiserInterface()
        {
            // Affiche le nom et rôle de l'employé connecté
            TxtNomEmploye.Text = $"{AppSession.Prenom} {AppSession.Nom}";
            TxtRoleEmploye.Text = AppSession.Role == "admin" ? "Administrateur" : "Agent";
            TxtDate.Text = DateTime.Now.ToString("dddd dd MMMM yyyy",
                                  new System.Globalization.CultureInfo("fr-FR"));

            // Affiche le bouton Employés uniquement pour les admins
            if (AppSession.EstAdmin)
                BtnEmployes.Visibility = Visibility.Visible;

            // Page par défaut
            NaviguerVers("dashboard");
        }

        // ── Navigation ──────────────────────────────────────
        private void NaviguerVers(string page)
        {
            // Réinitialise tous les boutons
            BtnDashboard.Style = (Style)FindResource("SidebarBtn");
            BtnClients.Style = (Style)FindResource("SidebarBtn");
            BtnContrats.Style = (Style)FindResource("SidebarBtn");
            BtnVehicules.Style = (Style)FindResource("SidebarBtn");
            BtnClub.Style = (Style)FindResource("SidebarBtn");
            BtnEmployes.Style = (Style)FindResource("SidebarBtn");

            switch (page)
            {
                case "dashboard":
                    BtnDashboard.Style = (Style)FindResource("SidebarBtnActive");
                    TxtPageTitre.Text = "Tableau de bord";
                    MainFrame.Navigate(new DashboardPage());
                    break;
                case "clients":
                    BtnClients.Style = (Style)FindResource("SidebarBtnActive");
                    TxtPageTitre.Text = "Gestion des clients";
                    MainFrame.Navigate(new ClientsPage());
                    break;
                case "contrats":
                    BtnContrats.Style = (Style)FindResource("SidebarBtnActive");
                    TxtPageTitre.Text = "Gestion des contrats";
                    MainFrame.Navigate(new ContratsPage());
                    break;
                case "vehicules":
                    BtnVehicules.Style = (Style)FindResource("SidebarBtnActive");
                    TxtPageTitre.Text = "Gestion des véhicules";
                    MainFrame.Navigate(new VehiculesPage());
                    break;
                case "club":
                    BtnClub.Style = (Style)FindResource("SidebarBtnActive");
                    TxtPageTitre.Text = "Club LocAutoPlus";
                    MainFrame.Navigate(new ClubPage());
                    break;
                case "employes":
                    BtnEmployes.Style = (Style)FindResource("SidebarBtnActive");
                    TxtPageTitre.Text = "Gestion des employés";
                    MainFrame.Navigate(new EmployesPage());
                    break;
            }
        }

        // ── Événements boutons ───────────────────────────────
        private void BtnDashboard_Click(object sender, RoutedEventArgs e) => NaviguerVers("dashboard");
        private void BtnClients_Click(object sender, RoutedEventArgs e) => NaviguerVers("clients");
        private void BtnContrats_Click(object sender, RoutedEventArgs e) => NaviguerVers("contrats");
        private void BtnVehicules_Click(object sender, RoutedEventArgs e) => NaviguerVers("vehicules");
        private void BtnClub_Click(object sender, RoutedEventArgs e) => NaviguerVers("club");
        private void BtnEmployes_Click(object sender, RoutedEventArgs e) => NaviguerVers("employes");

        private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Voulez-vous vraiment vous déconnecter ?",
                "Déconnexion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                AppSession.Clear();
                var login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }
    }
}
