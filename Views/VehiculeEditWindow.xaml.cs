using LocAutoPlusApp.Helpers;
using LocAutoPlusApp.Views.Pages;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;

namespace LocAutoPlusApp.Views
{
    /// <summary>
    /// Logique d'interaction pour VehiculeEditWindow.xaml
    /// </summary>
    public partial class VehiculeEditWindow : Window
    {
        private readonly VehiculeModel? _vehicule;
        private readonly bool _isEdit;

        public VehiculeEditWindow(VehiculeModel? vehicule)
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
                TxtImmat.IsEnabled = false; // immat non modifiable

                // Sélectionne le statut correspondant
                foreach (ComboBoxItem item in CbStatut.Items)
                    if (item.Content.ToString() == vehicule.Statut)
                    { item.IsSelected = true; break; }
            }
        }

        private void ChargerCategories()
        {
            try
            {
                var categories = new List<CategorieItem>();

                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                var cmd = new MySqlCommand(
                    "SELECT id, nom, tarif_base_jour FROM categories_vehicules ORDER BY nom", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new CategorieItem
                    {
                        Id = reader.GetInt32("id"),
                        Nom = $"{reader.GetString("nom")} — {reader.GetDecimal("tarif_base_jour"):F2} €/j"
                    });
                }

                CbCategorie.ItemsSource = categories;

                // Sélectionne la catégorie du véhicule en mode édition
                if (_isEdit && _vehicule != null)
                {
                    using var conn2 = DatabaseHelper.GetConnection();
                    conn2.Open();
                    var cmdCat = new MySqlCommand(
                        "SELECT categorie_id FROM vehicules WHERE id = @id", conn2);
                    cmdCat.Parameters.AddWithValue("@id", _vehicule.Id);
                    var catId = (int)(cmdCat.ExecuteScalar() ?? 0);

                    foreach (CategorieItem cat in categories)
                        if (cat.Id == catId)
                        { CbCategorie.SelectedItem = cat; break; }
                }
                else if (categories.Count > 0)
                {
                    CbCategorie.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement catégories : " + ex.Message);
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            // Validation
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
                AfficherErreur("L'année doit être un nombre valide (ex: 2023).");
                return;
            }

            if (!int.TryParse(TxtKm.Text, out int km))
            {
                AfficherErreur("Le kilométrage doit être un nombre entier.");
                return;
            }

            if (CbCategorie.SelectedItem is not CategorieItem categorie)
            {
                AfficherErreur("Veuillez sélectionner une catégorie.");
                return;
            }

            var statut = (CbStatut.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "disponible";

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                if (_isEdit)
                {
                    var cmd = new MySqlCommand(@"
                        UPDATE vehicules SET
                            marque       = @marque,
                            modele       = @modele,
                            annee        = @annee,
                            km_actuel    = @km,
                            categorie_id = @catId,
                            statut       = @statut,
                            photo_url    = @photo,
                            updated_at   = NOW()
                        WHERE id = @id", conn);

                    cmd.Parameters.AddWithValue("@marque", TxtMarque.Text.Trim());
                    cmd.Parameters.AddWithValue("@modele", TxtModele.Text.Trim());
                    cmd.Parameters.AddWithValue("@annee", annee);
                    cmd.Parameters.AddWithValue("@km", km);
                    cmd.Parameters.AddWithValue("@catId", categorie.Id);
                    cmd.Parameters.AddWithValue("@statut", statut);
                    cmd.Parameters.AddWithValue("@photo", string.IsNullOrWhiteSpace(TxtPhoto.Text)
                                                            ? DBNull.Value : TxtPhoto.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", _vehicule!.Id);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new MySqlCommand(@"
                        INSERT INTO vehicules
                            (immatriculation, marque, modele, annee, km_actuel,
                             categorie_id, statut, photo_url, created_at, updated_at)
                        VALUES
                            (@immat, @marque, @modele, @annee, @km,
                             @catId, @statut, @photo, NOW(), NOW())", conn);

                    cmd.Parameters.AddWithValue("@immat", TxtImmat.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@marque", TxtMarque.Text.Trim());
                    cmd.Parameters.AddWithValue("@modele", TxtModele.Text.Trim());
                    cmd.Parameters.AddWithValue("@annee", annee);
                    cmd.Parameters.AddWithValue("@km", km);
                    cmd.Parameters.AddWithValue("@catId", categorie.Id);
                    cmd.Parameters.AddWithValue("@statut", statut);
                    cmd.Parameters.AddWithValue("@photo", string.IsNullOrWhiteSpace(TxtPhoto.Text)
                                                           ? DBNull.Value : TxtPhoto.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                DialogResult = true;
                Close();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AfficherErreur("Cette immatriculation existe déjà en base de données.");
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

    // Modèle catégorie pour le ComboBox
    public class CategorieItem
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
    }
}
