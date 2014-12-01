#region Referencing

#endregion

#region Referenceing

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using PyrrhaAppLoad.FileIO;
using PyrrhaAppLoad.FileIO.Extern;
using PyrrhaAppLoad.Properties;

#endregion

namespace PyrrhaAppLoad.Imaging
{

    #region Referenceing

    #endregion

    public class ImageUtility
    {
        private static ImageUtility _instance;

        private static string[] _mappedDrives;

        private static string IconsDirectory;
        private IDictionary<string, string> _cachedIcons;

        private ImageUtility()
        {
            _mappedDrives = Directory.GetLogicalDrives();
            var installPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            IconsDirectory = Path.Combine(installPath, Settings.Default.IconDirectory);
            if (!Directory.Exists(IconsDirectory))
                Directory.CreateDirectory(IconsDirectory);

            CachedIcons = Directory.EnumerateFiles(IconsDirectory)
                                   .ToDictionary(Path.GetFileNameWithoutExtension, value => value);
        }

        public static ImageUtility Instance
        {
            get { return _instance ?? (_instance = new ImageUtility()); }
        }

        public IDictionary<string, string> CachedIcons
        {
            get { return _cachedIcons ?? (_cachedIcons = new Dictionary<string, string>()); }
            set { _cachedIcons = value; }
        }

        public string GetRegisteredIcon(string filePath)
        {
            var ext = GetFileEntryType(filePath);

            if (CachedIcons.ContainsKey(ext))
                return CachedIcons[ext];

            var shinfo = new SHfileInfo();
            Win32.SHGetFileInfo(filePath, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);

            Icon icon;
            using (var origIcon = Icon.FromHandle(shinfo.hIcon))
            {
                icon = (Icon) origIcon.Clone();
            }
            Win32.DestroyIcon(shinfo.hIcon);
            var iconPath = saveIconLocal(icon, ext);
            CachedIcons.Add(ext, iconPath);
            return iconPath;
        }

        private static string GetFileEntryType(string filePath)
        {
            string ext;
            if (!Path.HasExtension(filePath))
                ext = _mappedDrives.Contains(filePath) ? "drive" : "dir";
            else
                ext = Path.GetExtension(filePath);
            return ext;
        }

        private static string saveIconLocal(Icon icon, string entryType)
        {
            var iconPath = string.Format(@"{0}\{1}.ico", IconsDirectory, entryType);
            using (var stream = new FileStream(iconPath, FileMode.CreateNew))
            {
                icon.Save(stream);
            }
            return iconPath;
        }
    }
}