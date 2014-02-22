using System;
using System.Collections.Generic;
using System.Linq;
using Pyrrha.Runtime;
using Pyrrha.Runtime.Exception;

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
            get { return Documents.Values.First(); }
        }

        public static void AddDocument(PyrrhaDocument docParameter)
        {
            if (!Documents.ContainsKey(docParameter.GetHashCode()))
                Documents.Add(docParameter.GetHashCode(), docParameter);
        }

        public static void SaveAndCloseAll()
        {
            foreach (var doc in _documents.Values)
            {
                doc.ConfirmAllChanges();
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
    }
}
