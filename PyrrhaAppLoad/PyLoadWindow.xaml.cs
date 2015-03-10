#region Referenceing

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PyrrhaAppLoad.Bindings;

#endregion

namespace PyrrhaAppLoad
{
    /// <summary>
    ///     Interaction logic for PyLoadWindow.xaml
    /// </summary>
    public partial class PyLoadWindow
    {
        public PyLoadWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_Move(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void DirectoryView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var context = (ViewModel) DataContext;
            switch (e.Key)
            {
                case Key.Enter:
                    HandleEnter_KeyPress(context);
                    break;
                case Key.Left:
                case Key.Right:
                    HandleLeftRightArrow_KeyPress(context, e.Key);
                    break;
            }
        }

        private void HandleLeftRightArrow_KeyPress(ViewModel context, Key key)
        {
            var manager = context.NavigationManager;
            if (key == Key.Left)
            {
                if (manager.CanNavigatetoParent)
                    manager.NavigateToParent();
            }
            else if (manager.CanNavigateToPrevious)
                manager.NavigateToPrevious();
        }

        private void HandleEnter_KeyPress(ViewModel context)
        {
            var item = context.NavigationManager.SelectedDirectoryNavigationItem;

            if (File.Exists(item.Path))
                context.LoadFileCommand.Execute(null);
            else
                context.NavigationManager.NavigateTo(item);
        }

        private void SearchBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return && e.Key != Key.Tab)
                return;

            var textBox = (TextBox) sender;
            var viewModel = (ViewModel) DataContext;
            viewModel.SearchButtonCommand.Execute(textBox.Text);
        }
    }
}