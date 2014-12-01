#region Referenceing

using PyrrhaAppLoad.Imaging;
using PyrrhaAppLoad.Properties;

#endregion

namespace PyrrhaAppLoad.Bindings
{
    internal partial class ViewModel : ViewModelBase
    {
        internal readonly ImageUtility ImageUtility;

        public ViewModel()
        {
            NavigationManager = new DirectoryNavigationManager();
            ImageUtility = ImageUtility.Instance;
        }

        public DirectoryNavigationManager NavigationManager { get; set; }

        public string WindowTitle
        {
            get { return Settings.Default.WindowTitle; }
        }
    }
}