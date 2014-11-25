#region Referenceing

using System.Collections.Generic;
using PyrrhaAppLoad.Imaging;
using PyrrhaAppLoad.Properties;

#endregion

namespace PyrrhaAppLoad.Bindings
{
    internal partial class ViewModel : ViewModelBase
    {
        internal readonly ImageUtility ImageUtility;
        private IList<PyListViewItem> _currentDirctoryEntries;
        private string _currentDirectory;
        private string _lastLocation;

        public ViewModel()
        {
            ImageUtility = ImageUtility.Instance;
        }

        public PyListViewItem SelectedListViewItem { get; set; }

        public IList<PyListViewItem> CurrentDirctoryEntries
        {
            get
            {
                return _currentDirctoryEntries ??
                       (_currentDirctoryEntries = new List<PyListViewItem>());
            }
            set { SetField(ref _currentDirctoryEntries, value); }
        }

        public string CurrentDirectory
        {
            get { return _currentDirectory; }
            set { SetField(ref _currentDirectory, value); }
        }

        public string LastLocation
        {
            get { return _lastLocation ?? (_lastLocation = Settings.Default.LastLocation); }
            set { Settings.Default.LastLocation = value; }
        }
    }
}