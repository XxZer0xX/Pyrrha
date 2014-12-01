#region Referenceing

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
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public string FileSelectionResult { get; private set; }
        public string ScriptPathResult { get; private set; }


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
    }
}