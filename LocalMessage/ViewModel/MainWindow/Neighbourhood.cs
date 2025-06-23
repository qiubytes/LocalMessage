using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessage.ViewModel.MainWindow
{
    /// <summary>
    /// 网上邻居对象
    /// </summary>
    public class Neighbourhood : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _name;
        /// <summary>
        /// 网上邻居名字
        /// </summary>
        public string Name { get => _name; set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }
    }
}
