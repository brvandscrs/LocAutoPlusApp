using LocAutoPlusApp.ViewModels;
using System.Windows;

namespace LocAutoPlusApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContratListingViewModel vm = new ContratListingViewModel();
            DataContext = vm;
        }
    }
}