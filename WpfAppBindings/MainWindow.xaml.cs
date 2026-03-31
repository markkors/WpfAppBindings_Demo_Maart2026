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

namespace WpfAppBindings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _member;

        public MainWindow()
        {
            InitializeComponent();
            //lblContent.Content = "Hello World!";
            this.DataContext = this;
            MyProperty = 100;
        }


        public int MyProperty { 
            get { return _member; } 
            set { _member = value; } 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _member++;
        }
    }
}