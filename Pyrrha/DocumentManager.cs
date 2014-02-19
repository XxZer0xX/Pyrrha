using System;
using System.Collections.Generic;

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
            get { return Documents[0]; }
        }

        public static void AddDocument(PyrrhaDocument docParameter)
        {
            if (!Documents.Contains(docParameter))
                Documents.Add( docParameter );
        }

        public static void SaveAndCloseAll()
        {
            foreach (var doc in _documents)
            {
                doc.ConfirmAllChanges();
                doc.CloseAndSave( doc.Name );
            }
        }

        public void Dispose()
        {
            foreach (var doc in _documents)
                doc.Dispose();
            GC.SuppressFinalize( this );
        }
    }
}
