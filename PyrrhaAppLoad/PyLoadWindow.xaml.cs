#region Referenceing

using System.Windows;
using System.Windows.Input;
using PyrrhaAppLoad.Bindings;

#endregion

namespace PyrrhaAppLoad
{
    #region Referenceing

    

    #endregion

    /// <summary>
    ///     Interaction logic for PyLoadWindow.xaml
    /// </summary>
    public partial class PyLoadWindow
    {
        public PyLoadWindow()
        {
            InitializeComponent();
            //ViewModel.InitializedCommand.Execute(null);
            //Loaded += PyLoadWindow_Loaded;
        }

        public string FileSelectionResult { get; private set; }
    }
}