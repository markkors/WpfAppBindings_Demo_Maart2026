using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBindings.Viewmodel
{
    public class MainViewmodel : INotifyPropertyChanged
    {
        // Het viewmodel gebruiken we als een soort van "brug" tussen de view (UI) en de data.
        // Een viewmodel bevat de logica en de data die de view nodig heeft om correct te functioneren.
        // In dit geval hebben we een eenvoudige property genaamd MyProperty die we willen 'binden' aan een element in de UI, zoals een TextBlock of een Label.

        private int _member;

        public int MyProperty
        {
            get { return _member; }
            set
            {
                _member = value;
                OnPropertyChanged(nameof(MyProperty));
            }
        }


        #region INotifyPropertyChanged Implementation

        // De INotifyPropertyChanged interface is essentieel voor data binding in WPF.
        // Zonder deze interface zou de UI niet weten wanneer een property in het viewmodel is gewijzigd, en zou dus niet automatisch worden bijgewerkt.
        // Wanneer een property in het viewmodel wordt gewijzigd, roept de setter van die property de OnPropertyChanged methode aan, die op zijn beurt het PropertyChanged event triggert.
        
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }


}
