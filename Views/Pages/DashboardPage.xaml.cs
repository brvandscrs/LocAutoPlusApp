using LocAutoPlusApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly ApiService _api = new();

        public DashboardPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ChargerDonnees();
        }

        private async Task ChargerDonnees()
        {
            try
            {
                var stats = await _api.GetDashboardStatsAsync();
                if (stats == null) return;

                TxtNbClients.Text = stats.NbClients.ToString();
                TxtNbContratsEnCours.Text = stats.NbContratsEnCours.ToString();
                TxtNbVehiculesDispos.Text = stats.NbVehiculesDispos.ToString();
                TxtNbMembresClub.Text = stats.NbMembresClub.ToString();

                DgDerniersContrats.ItemsSource = stats.DerniersContrats;

                if (stats.VehiculesLoues.Count == 0)
                    TxtAucunLoue.Visibility = Visibility.Visible;
                else
                    ListVehiculesLoues.ItemsSource = stats.VehiculesLoues;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur dashboard : " + ex.Message, "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
