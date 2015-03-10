#region Referenceing

using System;
using System.IO;
using System.Windows.Input;

#endregion

namespace PyrrhaAppLoad.Bindings
{
    internal partial class ViewModel
    {
        public void DoSomethingWithFile(string path)
        {
        }

        #region ICommands

        private ICommand _backDirectoryCommand;
        private ICommand _forwardDirectoryCommand;
        private ICommand _listViewItemDoubleClickCommand;
        private ICommand _loadFileCommand;
        private ICommand _loadedCommand;
        private ICommand _searchCommand;


        public ICommand ListViewItemDoubleClickCommand
        {
            get
            {
                return _listViewItemDoubleClickCommand ??
                       (_listViewItemDoubleClickCommand = new RelayCommand(
                           obj => NavigationManager.NavigateToSelectedItem()));
            }
        }

        public ICommand BackDirectoryCommand
        {
            get
            {
                return _backDirectoryCommand ??
                       (_backDirectoryCommand = new RelayCommand(obj => NavigationManager.NavigateToParent()
                           , obj => NavigationManager.CanNavigatetoParent));
            }
        }

        public ICommand ForwardDirectoryCommand
        {
            get
            {
                return _forwardDirectoryCommand ??
                       (_forwardDirectoryCommand = new RelayCommand(obj => NavigationManager.NavigateToPrevious()
                           , obj => NavigationManager.CanNavigateToPrevious));
            }
        }

        public ICommand LoadFileCommand
        {
            get
            {
                return _loadFileCommand ??
                       (_loadFileCommand = new RelayCommand(_loadFileCommandAction, _loadFileCommandPredicate));
            }
        }

        public ICommand SearchButtonCommand
        {
            get
            {
                return _searchCommand ??
                       (_searchCommand = new RelayCommand(_searchCommandAction, _searchCommandPredicate));
            }
        }

        #endregion

        #region ICommand Actions

        private void _loadFileCommandAction(object obj)
        {
        }


        private void _searchCommandAction(object obj)
        {
            var expectedPath = string.Empty;
            var searchString = (string) obj;
            if (Path.HasExtension(searchString))
            {
                var fileInfo = new FileInfo(searchString);
                expectedPath = fileInfo.DirectoryName;
            }
            else
            {
                var dirInfo = new DirectoryInfo(searchString);
                expectedPath = dirInfo.FullName;
            }
            NavigationManager.NavigateToExplicit(new DirectoryNavigationItem(expectedPath));
        }

        #endregion

        #region ICommand Predicates

        private bool _loadFileCommandPredicate(object obj)
        {
            return false;
        }

        private bool _searchCommandPredicate(object obj)
        {
            if (obj == null)
                return false;

            var expectedString = (string) obj;
            return !string.IsNullOrEmpty(expectedString)
                   &&
                   !NavigationManager.CurrentNavigationTarget.Equals(expectedString,
                       StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}