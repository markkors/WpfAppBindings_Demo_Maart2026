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

namespace WpfAppBindings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int _member;
        private int _score;

        public MainWindow()
        {
            InitializeComponent();
            //lblContent.Content = "Hello World!";
            this.DataContext = this;
            MyProperty = 100;
        }


        public int MyProperty { 
            get { return _member; } 
            set { 
                _member = value;
                doPropertyChanged(nameof(MyProperty));
            } 
        }

        public int Score { 
            get { return _score; }
            set { 
                _score = value;
                doPropertyChanged(nameof(Score));
            } 
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyProperty = MyProperty + 1;
            Score++;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyProperty)));
            
            
        }

        private void doPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}