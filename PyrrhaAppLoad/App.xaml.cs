#region Referenceing

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PyrrhaAppLoad.Properties;

#endregion

namespace PyrrhaAppLoad
{

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public readonly new Window MainWindow;

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
        }

        public string ScriptPathResult { get; private set; }

        internal static IEnumerable<string> AccessableDrives { get; set; }

        private void App_DispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            // intercept unhandled exceptions
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //todo
            //Delegate startLoadTask = 

            //base.OnStartup(e);
            loadAccessableDrives();
        }

        private void loadAccessableDrives()
        {
            
            AccessableDrives = DriveInfo.GetDrives()
                                            .Where(drive =>
                                                (!drive.DriveType.Equals(DriveType.CDRom) ||
                                                 drive.DriveType.Equals(DriveType.NoRootDirectory)) && Directory.Exists(drive.Name))
                                            .Select(drive => drive.Name);
        }
    }
}