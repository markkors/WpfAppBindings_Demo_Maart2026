using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAppBindings.Viewmodel;

namespace WpfAppBindings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // We maken een instantie van het MainViewmodel aan,
        // zodat we deze kunnen gebruiken als DataContext voor onze MainWindow.
        private MainViewmodel _mainViewmodel = new MainViewmodel();
        public MainWindow()
        {
            InitializeComponent();
            // Koppel het viewmodel aan de DataContext van de MainWindow, zodat we de properties van het viewmodel kunnen binden aan de UI-elementen in de XAML.
            this.DataContext = _mainViewmodel;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Wanneer de button wordt geklikt, verhogen we de waarde van MyProperty in het viewmodel.
            // Door de binding in de XAML zal de UI automatisch worden bijgewerkt met de nieuwe waarde van MyProperty.
            _mainViewmodel.getPersons();
        }
    }
}