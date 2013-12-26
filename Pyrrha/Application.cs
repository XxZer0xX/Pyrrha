using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;

namespace Pyrrha
{
    public class Application
    {


        private Document _activeDocument;

        #region Properies
        [Obsolete]
        public AcApp OriginalApplication { get; set; }
        [Obsolete]
        public static DocumentCollection DocumentManager { get { return AcApp.DocumentManager; } }

        #endregion

        #region Constructors
        [Obsolete]
        public Application() { }
        [Obsolete]
        public Application(bool Visible)
        {
            
        }
        [Obsolete]
        public Application(Autodesk.AutoCAD.ApplicationServices.Application application, bool visible = false)
        {
            
        }

        [Obsolete]
        public Document ActiveDocument
        {
            get { return _activeDocument ?? ( _activeDocument = new Document(DocumentManager.MdiActiveDocument) ); }
            set
            {
               var docToBeActive =  AcApp.DocumentManager.Cast<Autodesk.AutoCAD.ApplicationServices.Document>()
                    .FirstOrDefault(doc => doc.Name.Equals(value.Name, StringComparison.CurrentCultureIgnoreCase));
                if (docToBeActive == null)
                    return;
                AcApp.DocumentManager.MdiActiveDocument = docToBeActive;
                _activeDocument = new Document(docToBeActive);
            }
        }

        #endregion

        #region Methods
        [Obsolete]
        public bool Start(string versionId)
        {
            return true;
        }
        [Obsolete]
        public bool Close()
        {
            return true;
        }
        [Obsolete]
        public bool Connect()
        {
            return true;
        }
        [Obsolete]
        public IList<string> GetApplicationData()
        {
            return new List<string>();
        }

        #endregion
    }
}
