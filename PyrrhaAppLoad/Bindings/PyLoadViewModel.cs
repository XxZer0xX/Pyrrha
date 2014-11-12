namespace PyrrhaAppLoad.Bindings
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Imaging;

    internal partial class PyLoadViewModel : ViewModelBase
    {
        private readonly IEnumerable<string> _accessableDrives;

        internal readonly ImageUtility ImageUtility;

        public PyLoadViewModel()
        {
            _accessableDrives = DriveInfo.GetDrives()
                .Where(drive => !drive.DriveType.Equals(DriveType.CDRom) || drive.DriveType.Equals(DriveType.NoRootDirectory))
                .Select(drive => drive.Name);

            ImageUtility = ImageUtility.Instance;
        }

        internal PyListViewItem SelectedListViewItem { get; set; }

        private ObservableCollection<PyListViewItem> _currentDirctoryEntries;
        public ObservableCollection<PyListViewItem> CurrentDirctoryEntries
        {
            get
            {
                return _currentDirctoryEntries ??
                    (_currentDirctoryEntries = new ObservableCollection<PyListViewItem>());
            }
            set { SetField(ref _currentDirctoryEntries, value); }
        }

        private string _currentDirectory;

        public string CurrentDirectory
        {
            get { return _currentDirectory; }
            set
            {
                SetField(ref _currentDirectory, value);
            }
        }


    }

}