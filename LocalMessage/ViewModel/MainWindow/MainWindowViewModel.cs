using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessage.ViewModel.MainWindow
{
    /// <summary>
    /// 主页的ViewModel
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ObservableCollection<Neighbourhood> neighbourhoods;
        public ObservableCollection<Neighbourhood> Neighbourhoods
        {
            get => neighbourhoods; set
            {
                neighbourhoods = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Neighbourhoods"));
            }
        }
        public MainWindowViewModel()
        { 
            neighbourhoods = new System.Collections.ObjectModel.ObservableCollection<Neighbourhood>();
        }
    }
}
