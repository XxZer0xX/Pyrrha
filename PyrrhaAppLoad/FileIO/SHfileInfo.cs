#region Referencing



#endregion

namespace PyrrhaAppLoad.FileIO
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout( LayoutKind.Sequential )]
    public struct SHfileInfo
    {
        public IntPtr hIcon;

        public int iIcon;

        public uint dwAttributes;

        [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 260 )]
        public string szDisplayName;

        [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 80 )]
        public string szTypeName;
    }
}