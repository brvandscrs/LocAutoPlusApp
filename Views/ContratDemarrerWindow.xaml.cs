using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Services;
using System.Windows;

namespace LocAutoPlusApp.Views
{
    public partial class ContratDemarrerWindow : Window
    {
        private readonly int _contratId;
        private readonly ApiService _api = new();
        public int KmDepart { get; private set; }

        public ContratDemarrerWindow(int contratId, int kmActuel)
        {
            InitializeComponent();
            _contratId = contratId;
            TxtKmDepart.Text = kmActuel.ToString();
        }

        private async void BtnDemarrer_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtKmDepart.Text, out int km) || km < 0)
            {
                TxtErreur.Text = "Veuillez saisir un kilométrage valide.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                var result = await _api.UpdateStatutContratAsync(_contratId, new
                {
                    statut = "en_cours",
                    km_depart = km,
                    employe_id = AppSession.EmployeId,
                });

                if (result?.Success == true)
                {
                    KmDepart = km;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    TxtErreur.Text = result?.Message ?? "Erreur lors du démarrage.";
                    TxtErreur.Visibility = Visibility.Visible;
                }
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
