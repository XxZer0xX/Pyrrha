using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PyrrhaAppLoad.Annotations;
using PyrrhaAppLoad.Bindings;

namespace PyrrhaAppLoad
{
    /// <summary>
    /// Interaction logic for Waiting.xaml
    /// </summary>
    public partial class WaitingDialog : INotifyPropertyChanged
    {
        public WaitingDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private string _displayText;

        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                _displayText = value;
                OnPropertyChanged(); 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
