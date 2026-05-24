using System.Windows;

namespace LocAutoPlusApp.Views
{
    public partial class ContratCloturerWindow : Window
    {
        public int KmRetour { get; private set; }
        public DateTime DateFinReelle { get; private set; }

        public ContratCloturerWindow(int contratId)
        {
            InitializeComponent();
            DpRetour.SelectedDate = DateTime.Today;
        }

        private void BtnCloturer_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtKmRetour.Text, out int km) || km < 0)
            {
                TxtErreur.Text = "Veuillez saisir un kilométrage valide.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            if (!DpRetour.SelectedDate.HasValue)
            {
                TxtErreur.Text = "Veuillez sélectionner une date de retour.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            KmRetour = km;
            DateFinReelle = DpRetour.SelectedDate.Value;
            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
