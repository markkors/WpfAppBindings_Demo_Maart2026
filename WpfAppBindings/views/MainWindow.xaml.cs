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


        private void Getpersons_Click(object sender, RoutedEventArgs e)
        {
            // Wanneer de button wordt geklikt, halen we de personen op via de getPersons() methode van het MainViewmodel. 
            // Door de binding in de XAML zal de UI automatisch worden bijgewerkt.
            // Er vinden diverse automatische updates plaats dankzij de data binding en de implementatie van INotifyPropertyChanged in het viewmodel.

            // Let hierbij vooral op de Combobox, deze is gebonden aan de Persons property van het viewmodel, en zal automatisch de lijst van personen tonen zodra deze is opgehaald.
            // In het viewmodel wordt de PersonsLoaded property direct bijgewerkt, wat ook automatisch wordt weergegeven in de UI dankzij de binding.
            // De selectedperson property in het viewmodel wordt ook automatisch bijgewerkt wanneer de gebruiker een persoon selecteert in de combo box, dankzij de TwoWay binding.
            _mainViewmodel.getPersons();
        }

        private void Addpersons_Click(object sender, RoutedEventArgs e)
        {
            _mainViewmodel.addPerson(new models.Person { name = "New Person Test", age = 30 });
        }
    }
}