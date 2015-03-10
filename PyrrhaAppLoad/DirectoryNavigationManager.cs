#region Referenceing

using PyrrhaAppLoad.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

#endregion

namespace PyrrhaAppLoad
{
    public sealed class DirectoryNavigationManager : List<DirectoryNavigationItem>, INotifyPropertyChanged
    {
        #region ctor

        public DirectoryNavigationManager()
        {
            setCurrentDirectoryContent();
        }

        #endregion

        #region Shadowed

        new private void Insert(int index, DirectoryNavigationItem item) => base.Insert(index, item);
        #endregion

        #region Properties

        private static IEnumerable<string> _accessableDrives;
        private IEnumerable<DirectoryNavigationItem> _currentDirectoryContent;
        private string _currentNavigationTarget;
        private DirectoryNavigationItem _selectedDirectoryNavigationItem;
        private DirectoryInfo _currentDirectory;
        private int currentIndex = -1;

        public string CurrentNavigationTarget
        {
            get { return _currentNavigationTarget; }
            set
            {
                _currentNavigationTarget = value;
                OnPropertyChanged();
            }
        }


        public IEnumerable<DirectoryNavigationItem> CurrentDirectoryContent
        {
            get { return _currentDirectoryContent; }

            set
            {
                _currentDirectoryContent = value;
                OnPropertyChanged();
            }
        }

        public DirectoryNavigationItem SelectedDirectoryNavigationItem
        {
            get { return _selectedDirectoryNavigationItem; }
            set
            {
                if (value == null)
                    return;
                _selectedDirectoryNavigationItem = value;
                CurrentNavigationTarget = value.Path;
            }
        }

        public static IEnumerable<string> AccessableDrives { get; } = loadAccessableDrives();

        public bool CanNavigateToPrevious
        {
            get { return currentIndex < Count - 1 && currentIndex > -1; }
        }

        public bool CanNavigatetoParent
        {
            get { return _currentDirectory != null; }
        }

        #endregion

        #region Public Methods

        public void NavigateToPrevious()
        {
            currentIndex++;
            var item = this[currentIndex];

            var entryInfo = item.Info as DirectoryInfo;

            if (entryInfo == null)
                return; // Todo Handle File

            _currentDirectory = entryInfo;
            setCurrentDirectoryContent();
        }

        public void NavigateToParent()
        {
            currentIndex--;

            _currentDirectory = _currentDirectory.Parent;

            if (_currentDirectory == null)
                currentIndex = -1;

            setCurrentDirectoryContent();
        }

        public void NavigateTo(DirectoryNavigationItem item)
        {
            if (Path.HasExtension(item.Path))
                return;

            currentIndex++;
            _currentDirectory = (DirectoryInfo) item.Info;
            EvaluateSubDirectories(item);

            if (currentIndex < Count && this[currentIndex].Path == item.Path)
            {
                currentIndex--;
                NavigateToPrevious();
                return;
            }

            Insert(currentIndex, item);
            setCurrentDirectoryContent();
        }

        public void NavigateToSelectedItem()
        {
            if (SelectedDirectoryNavigationItem == null)
                return;
            NavigateTo(SelectedDirectoryNavigationItem);
        }

        public void NavigateToExplicit(DirectoryNavigationItem item)
        {
            if (Count <= 0)
            {
                var stack = new Stack<DirectoryNavigationItem>();
                var info = item.Info as DirectoryInfo;

                if (info == null)
                    return;

                _currentDirectory = info;
                while (info.Parent != null)
                {
                    stack.Push(new DirectoryNavigationItem(info.FullName));
                    info = info.Parent;
                }

                foreach (var navigationItem in stack)
                {
                    currentIndex++;
                    Insert(currentIndex, navigationItem);
                }

                setCurrentDirectoryContent();
            }
        }

        #endregion

        #region Private Methods

        private void EvaluateSubDirectories(DirectoryNavigationItem item)
        {
            if (currentIndex == Count)
                return;

            if (currentIndex < Count && _currentDirectory.FullName != this[currentIndex].Path)
                RemoveRange(currentIndex, Math.Abs(currentIndex - Count));
        }

        private static IEnumerable<string> loadAccessableDrives()
        {
            return DriveInfo.GetDrives()
                            .Where(drive =>
                                (!drive.DriveType.Equals(DriveType.CDRom) ||
                                 drive.DriveType.Equals(DriveType.NoRootDirectory)) &&
                                Directory.Exists(drive.Name))
                            .Select(drive => drive.Name);
        }


        private void setCurrentDirectoryContent()
        {
            IEnumerable<string> iterablePaths;
            if (currentIndex == -1 && _currentDirectory == null)
            {
                iterablePaths = AccessableDrives;
                CurrentNavigationTarget = "Computer";
            }
            else
            {
                if (_currentDirectory == null)
                {
                    // TODO Find Bug
                    MessageBox.Show("Current Directory is null");
                    Thread.CurrentThread.Abort(this);
                }

                CurrentNavigationTarget = _currentDirectory.FullName;

                iterablePaths = Directory.EnumerateFileSystemEntries(_currentDirectory.FullName, "*",
                    SearchOption.TopDirectoryOnly).Where(
                        subpath =>
                        {
                            var dirInfo = new DirectoryInfo(subpath);
                            var attrs = dirInfo.Attributes;

                            if (UserHasAccess(subpath) && (attrs ^ FileAttributes.Directory) == 0 || (attrs ^ FileAttributes.Archive) == 0)
                                return true;
                            return (attrs & FileAttributes.Hidden) == 0
                                   &&
                                   Path.GetExtension(subpath).Equals(".py", StringComparison.CurrentCultureIgnoreCase);
                        });
            }

            CurrentDirectoryContent =
                iterablePaths.Select(
                    obj => new DirectoryNavigationItem(obj));
        }

        private bool UserHasAccess(string subpath)
        {
            if (Path.HasExtension(subpath))
                return true;

            DirectorySecurity acl = null;
            
            try
            {
                acl = Directory.GetAccessControl(subpath);
            }
            catch (Exception ex) if (ex is DirectoryNotFoundException)
            {
                return false;
            }

            var arc = acl?.GetAccessRules(true, true, typeof(SecurityIdentifier));

            if (arc == null)
                return false;

            foreach (FileSystemAccessRule rule in arc)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    return true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    return false;
            }
            return false;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        #endregion

  
    }
}