using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

namespace Pyrrha.Scripting.AutoCad
{
    public class PyrrhaMonitor : Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        public void Initialize()
        {
            Autodesk.AutoCAD.Internal.Windows.CommandThroat.InputCharactersQueued += CommandThroat_InputCharactersQueued;
           
            Application.DocumentManager.DocumentLockModeChanged += ( sender , args ) =>
            {
                Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
                var command = args.GlobalCommandName.ToUpper();
                if (command.Contains("(PY") && !command[0].Equals('#'))
                    somethii.ProcessScriptingCall(args.GlobalCommandName);
            };
        }

        void CommandThroat_InputCharactersQueued(object sender, Autodesk.AutoCAD.Internal.Windows.InputCharactersQueuedEventArgs e)
        {
            var send = sender;
        }

        void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            var collection = (DocumentCollection)sender;
            var activedocument = collection.MdiActiveDocument;
            var eventmanager = Autodesk.AutoCAD.Internal.PreviousInput.CommandLineMonitorServices.Instance();
            var something = Autodesk.AutoCAD.Internal.
            var monitor = eventmanager.GetCommandLineMonitor(activedocument);
            monitor.
            monitor.CommandWillStart += monitor_CommandWillStart;
        }

        void monitor_CommandWillStart(object sender, Autodesk.AutoCAD.Internal.PreviousInput.InputStringEventArgs e)
        {
            var send = sender;
        }

        void activedocument_UnknownCommand(object sender, UnknownCommandEventArgs e)
        {
            var send = sender;
        }

        void DocumentManager_DocumentToBeActivated(object sender, DocumentCollectionEventArgs e)
        {
            var send = sender;
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }

    public class somethii
    {
        [CommandMethod("(")]
        public static void ProcessScriptingCall(string input)
        {
            var source = input;
        }
    }
}
