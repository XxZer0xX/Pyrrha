﻿namespace PyrrhaAppLoad.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using Properties;

    internal partial class PyLoadViewModel
    {

        #region ICommands
        private ICommand _initCommand;
        private ICommand _loadedCommand;
        private ICommand _backDirectoryCommand;
        private ICommand _forwardDirectoryCommand;
        private ICommand _loadFileCommand;
        private ICommand _closeCommand;
        private ICommand _searchCommand;
        private ICommand _listViewItemDoubleClickCommand;


        public ICommand ListViewItemDoubleClickCommand
        {
            get
            {
                return _listViewItemDoubleClickCommand ??
                       (_listViewItemDoubleClickCommand = new RelayCommand(_listViewItemDoubleClickCommandAction));
            }
        }


        public ICommand InitializedCommand
        {
            get
            {
                return _initCommand ??
                       (_initCommand = new RelayCommand(_initCommandAction));
            }
        }


        public ICommand LoadedCommand
        {
            get
            {
                return _loadedCommand ??
                       (_loadedCommand = new RelayCommand(_loadedCommandAction));
            }
        }



        #endregion

        #region ICommand Actions

        private void _listViewItemDoubleClickCommandAction(object obj)
        {
            // not loading the selected item
            var fileEntries = GetFileSystemEntries(SelectedListViewItem.Path);
            LoadListViewItems(fileEntries); 
        }

        private void _initCommandAction(object obj)
        {
            if (!string.IsNullOrEmpty(Settings.Default.LastLocation))
                return;
            CurrentDirectory = "Computer";
            _loadedCommandAction(_accessableDrives);
        }

        private void _loadedCommandAction(object obj)
        {
            if (CurrentDirctoryEntries != null)
                CurrentDirctoryEntries.Clear();

            var fileSystemEntries = obj == null 
                ? GetFileSystemEntries(Settings.Default.LastLocation) 
                : _accessableDrives;

            LoadListViewItems(fileSystemEntries);
        }


        private void _backDirectoryCommandAction(object obj)
        {

        }

        private void _forwardDirectoryCommandAction(object obj)
        {

        }

        private void _loadFileCommandAction(object obj)
        {

        }

        private void _closeCommandAction(object obj)
        {

        }

        private void _searchCommandAction(object obj)
        {

        }

        #endregion

        #region ICommand Predicates

        private bool _loadCommandPredicate(object obj)
        {
            return false;
        }

        private bool _backDirectoryCommandPredicate(object obj)
        {
            return false;
        }

        private bool _forwardDirectoryCommandPredicate(object obj)
        {
            return false;
        }

        private bool _loadFileCommandPredicate(object obj)
        {
            return false;
        }

        private bool _closeCommandPredicate(object obj)
        {
            return false;
        }

        private bool _searchCommandPredicate(object obj)
        {
            return false;
        }

        #endregion

        #region Functional

        internal IEnumerable<string> GetFileSystemEntries(string path)
        {
            return Directory.EnumerateFileSystemEntries(path).Where(
                    subpath =>
                        !Path.HasExtension(path) ||
                        Path.GetExtension(subpath).Equals(".py", StringComparison.CurrentCultureIgnoreCase));
        }

        internal void LoadListViewItems(IEnumerable<string> paths)
        {
            var items = paths.Select(path => new PyListViewItem(path, ImageUtility.GetRegisteredIcon(path)));
            foreach (var item in items)
                CurrentDirctoryEntries.Add(item);
        }

        #endregion
    }
}