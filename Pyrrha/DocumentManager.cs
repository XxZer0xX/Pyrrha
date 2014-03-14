using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Pyrrha
{
    public class DocumentManager : IDisposable
    {
        private static IList<PyrrhaDocument> _documents;
        public static IList<PyrrhaDocument> Documents
        {
            get { return _documents ?? ( _documents = new List<PyrrhaDocument>() ); }
        }

        public static PyrrhaDocument ActiveDocument
        {
            get { return GetActiveDocument(); }
        }

        public static void AddDocument(PyrrhaDocument document)
        {
            if (!Documents.Contains(document))
                Documents.Add(document);
        }

        public static void SaveAndCloseAll()
        {
            foreach (var doc in _documents)
            {
                doc.ConfirmAllChanges();

                // This might not save to the right location.
                // I think we should add extensions for 
                // Save() and Close() from the AcadDocument.
                doc.CloseAndSave();
            }
        }

        public void Dispose()
        {
            foreach (var doc in _documents)
                doc.Dispose();
            GC.SuppressFinalize( this );
        }

        private static PyrrhaDocument GetActiveDocument()
        {
            return Documents.FirstOrDefault(doc => doc.BaseDocument
                            .Equals(AcApp.DocumentManager.MdiActiveDocument));
        }
    }
}
