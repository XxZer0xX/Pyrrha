#region Referenceing

using System;
using System.ComponentModel;
using System.IO;

#endregion

namespace PyrrhaAppLoad
{
    #region Referenceing

    

    #endregion

    internal class PyListViewItem
    {
        private readonly FileSystemInfo _info;
        private DateTime? _created;
        private DateTime? _lastModified;

        private string _name;
        private string _path;

        public PyListViewItem(string path, string iconUri)
        {
            _info = System.IO.Path.HasExtension(path) ? (FileSystemInfo) new FileInfo(path) : new DirectoryInfo(path);
            FileIconUri = iconUri;
        }

        public string Name
        {
            get { return _name ?? (_name = _info.Name); }
        }

        public DateTime? LastModified
        {
            get { return _lastModified ?? (_lastModified = _info.LastWriteTime); }
        }

        public DateTime? Created
        {
            get { return _created ?? (_created = _info.CreationTime); }
        }

        public string Path
        {
            get { return _path ?? (_path = _info.FullName); }
        }

        public string FileIconUri { get; set; }

        [DefaultValue(true)]
        public bool IsAvailable { get; private set; }
    }
}