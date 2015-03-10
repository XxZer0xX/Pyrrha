#region Referenceing

using PyrrhaAppLoad.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace PyrrhaAppLoad
{
    /// <summary>
    ///     Interaction logic for Waiting.xaml
    /// </summary>
    public partial class WaitingDialog : INotifyPropertyChanged
    {
        private string _displayText;

        public WaitingDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string DisplayText
        {
            get { return _displayText; }
            set
            {
                _displayText = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}