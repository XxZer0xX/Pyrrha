using System;


namespace PyrrhaAppLoad
{
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;

    internal class PyListViewItem
    {
        private readonly FileSystemInfo _info;

        private string _name;
        public string Name
        {
            get { return _name ?? (_name = _info.Name); }
        }

        private DateTime? _lastModified;
        public DateTime? LastModified
        {
            get { return _lastModified ?? (_lastModified = _info.LastWriteTime); }
        }

        private DateTime? _created;
        public DateTime? Created
        {
            get { return _created ?? (_created = _info.CreationTime); }
        }

        private string _path;
        public string Path
        {
            get { return _path ?? (_path = _info.FullName); }
        }

        public string FileIconUri { get; set; }

        [DefaultValue(true)]
        public bool IsAvailable { get; private set; }

        public PyListViewItem(string path, string iconUri)
        {
            _info = System.IO.Path.HasExtension(path) ? (FileSystemInfo)new FileInfo(path) : new DirectoryInfo(path);
            FileIconUri = iconUri;
        }
    }
}
