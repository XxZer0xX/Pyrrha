#region Referenceing

using System;
using System.ComponentModel;
using System.IO;
using PyrrhaAppLoad.Imaging;

#endregion

namespace PyrrhaAppLoad
{
    public sealed class DirectoryNavigationItem
    {
        private DateTime? _created;
        private DateTime? _lastModified;
        private string _name;
        private string _path;
        internal readonly FileSystemInfo Info;

        public DirectoryNavigationItem(string path)
        {
            Info = System.IO.Path.HasExtension(path) ? (FileSystemInfo) new FileInfo(path) : new DirectoryInfo(path);
            FileIconUri = ImageUtility.Instance.GetRegisteredIcon(path);
        }

        public string Name
        {
            get { return _name ?? (_name = Info.Name); }
        }

        public DateTime? LastModified
        {
            get { return _lastModified ?? (_lastModified = Info.LastWriteTime); }
        }

        public DateTime? Created
        {
            get { return _created ?? (_created = Info.CreationTime); }
        }

        public string Path
        {
            get { return _path ?? (_path = Info.FullName); }
        }

        public string FileIconUri { get; set; }

        [DefaultValue(true)]
        public bool IsAvailable { get; private set; }
    }
}