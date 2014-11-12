namespace PyrrhaAppLoad
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using Bindings;

    /// <summary>
    ///     Interaction logic for PyLoadWindow.xaml
    /// </summary>
    public partial class PyLoadWindow
    {
        public PyLoadWindow()
        {
            InitializeComponent();
            DataContext = ViewModel ?? (ViewModel = new PyLoadViewModel());
            ViewModel.InitializedCommand.Execute(null);
            //Loaded += PyLoadWindow_Loaded;
        }

        void PyLoadWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.LoadedCommand.Execute(null);
        }

        internal PyLoadViewModel ViewModel { get; set; }

        public string FileSelectionResult { get; private set; }

        private void DirectoryView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.ListViewItemDoubleClickCommand.Execute(null);
        }
    }
}