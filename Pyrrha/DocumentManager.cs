using System;
using System.Collections.Generic;
using System.Linq;
<<<<<<< HEAD
using Autodesk.AutoCAD.ApplicationServices;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
=======
using Pyrrha.Runtime;
using Pyrrha.Runtime.Exception;
>>>>>>> origin/master

namespace Pyrrha
{
    public class DocumentManager : IDisposable, IEnumerable<PyrrhaDocument>
    {
        private static IDictionary<int, PyrrhaDocument> _documents;
        public static IDictionary<int, PyrrhaDocument> Documents
        {
            get { return _documents ?? (_documents = new Dictionary<int, PyrrhaDocument>()); }
        }

        public PyrrhaDocument this[int hash]
        {
            get
            {
                if (Documents.ContainsKey(hash))
                    return Documents[hash];

                var exception = new InvalidAccessException("Document Not available");
                exception.ThrowException();
                return null;

            }
            set
            {
                if (Documents.ContainsKey(hash))
                {
                    Documents[hash] = value;
                    return;
                }

                var exception = new InvalidAccessException("Document Not available");
                exception.ThrowException();
            }
        }

        public static PyrrhaDocument ActiveDocument
        {
<<<<<<< HEAD
            get { return GetActiveDocument(); }
=======
            get { return Documents.Values.First(); }
>>>>>>> origin/master
        }

        public static void AddDocument(PyrrhaDocument document)
        {
<<<<<<< HEAD
            if (!Documents.Contains(document))
                Documents.Add(document);
=======
            if (!Documents.ContainsKey(docParameter.GetHashCode()))
                Documents.Add(docParameter.GetHashCode(), docParameter);
>>>>>>> origin/master
        }

        public static void SaveAndCloseAll()
        {
            foreach (var doc in _documents.Values)
            {
                doc.ConfirmAllChanges();
<<<<<<< HEAD

                // This might not save to the right location.
                // I think we should add extensions for 
                // Save() and Close() from the AcadDocument.
=======
>>>>>>> origin/master
                doc.CloseAndSave(doc.Name);
            }
        }

        public void Dispose()
        {
            foreach (var doc in _documents.Values)
                doc.Dispose();
            GC.SuppressFinalize(this);
        }

        public IEnumerator<PyrrhaDocument> GetEnumerator()
        {
            return Documents.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static PyrrhaDocument GetActiveDocument()
        {
            return Documents.FirstOrDefault(doc => doc.BaseDocument
                            .Equals(AcApp.DocumentManager.MdiActiveDocument));
        }
    }
}
