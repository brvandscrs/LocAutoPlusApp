using LocAutoPlusApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    public partial class VehiculeEditWindow : Window
    {
        private readonly VehiculeDto? _vehicule;
        private readonly bool _isEdit;
        private readonly ApiService _api = new();

        public VehiculeEditWindow(VehiculeDto? vehicule)
        {
            InitializeComponent();
            _vehicule = vehicule;
            _isEdit = vehicule != null;

            ChargerCategories();

            if (_isEdit)
            {
                TxtTitre.Text = "Modifier le véhicule";
                TxtImmat.Text = vehicule!.Immatriculation;
                TxtMarque.Text = vehicule.Marque;
                TxtModele.Text = vehicule.Modele;
                TxtAnnee.Text = vehicule.Annee.ToString();
                TxtKm.Text = vehicule.KmActuel.ToString();
                TxtImmat.IsEnabled = false;

                foreach (ComboBoxItem item in CbStatut.Items)
                    if (item.Content.ToString() == vehicule.Statut)
                    { item.IsSelected = true; break; }
            }
        }

        private async void ChargerCategories()
        {
            try
            {
                var categories = await _api.GetCategoriesAsync();
                CbCategorie.ItemsSource = categories;
                CbCategorie.DisplayMemberPath = "Label";
                CbCategorie.SelectedValuePath = "Id";

                if (_isEdit && _vehicule != null)
                    CbCategorie.SelectedValue = _vehicule.CategorieId;
                else if (categories.Count > 0)
                    CbCategorie.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement catégories : " + ex.Message);
            }
        }

        private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtImmat.Text) ||
                string.IsNullOrWhiteSpace(TxtMarque.Text) ||
                string.IsNullOrWhiteSpace(TxtModele.Text) ||
                string.IsNullOrWhiteSpace(TxtAnnee.Text))
            {
                AfficherErreur("Les champs Immatriculation, Marque, Modèle et Année sont obligatoires.");
                return;
            }

            if (!int.TryParse(TxtAnnee.Text, out int annee) || annee < 1900 || annee > 2100)
            {
                AfficherErreur("L'année doit être un nombre valide.");
                return;
            }

            if (!int.TryParse(TxtKm.Text, out int km))
            {
                AfficherErreur("Le kilométrage doit être un nombre entier.");
                return;
            }

            if (CbCategorie.SelectedValue is not int categorieId)
            {
                AfficherErreur("Veuillez sélectionner une catégorie.");
                return;
            }

            var statut = (CbStatut.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "disponible";

            try
            {
                ApiResponse? result;

                if (_isEdit)
                {
                    result = await _api.UpdateVehiculeAsync(_vehicule!.Id, new
                    {
                        marque = TxtMarque.Text.Trim(),
                        modele = TxtModele.Text.Trim(),
                        annee,
                        km_actuel = km,
                        categorie_id = categorieId,
                        statut,
                        photo_url = string.IsNullOrWhiteSpace(TxtPhoto.Text)
                                       ? null : TxtPhoto.Text.Trim(),
                    });
                }
                else
                {
                    result = await _api.CreateVehiculeAsync(new
                    {
                        immatriculation = TxtImmat.Text.Trim().ToUpper(),
                        marque = TxtMarque.Text.Trim(),
                        modele = TxtModele.Text.Trim(),
                        annee,
                        km_actuel = km,
                        categorie_id = categorieId,
                        statut,
                        photo_url = string.IsNullOrWhiteSpace(TxtPhoto.Text)
                                          ? null : TxtPhoto.Text.Trim(),
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
