#region Referenceing

using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

#endregion

namespace Pyrrha
{
    public class TextStyle
    {
        public TextStyleTableRecord OriginalRecord;

        #region Properties

        public Database Database
        {
            get { return OriginalRecord.Database; }
        }

        /// <summary>
        ///     Current style's name.
        /// </summary>
        public string Name
        {
            get { return OriginalRecord.Name; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (TextStyleTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Name = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current styles BigFontFileName.
        /// </summary>
        public string BigFontFileName
        {
            get { return OriginalRecord.BigFontFileName; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (TextStyleTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).BigFontFileName = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current styles FileName.
        /// </summary>
        public string FileName
        {
            get { return OriginalRecord.FileName; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (TextStyleTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).FileName = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current styles TextSize.
        /// </summary>
        public double TextSize
        {
            get { return OriginalRecord.TextSize; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (TextStyleTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).TextSize = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current styles ObjectId.
        /// </summary>
        public ObjectId ObjectId
        {
            get { return OriginalRecord.ObjectId; }
        }

        /// <summary>
        ///     Highlight the current style or
        ///     return a boolean based on the current styles highlighted property.
        /// </summary>
        public bool IsHighLighted { get; set; }

        #endregion

        #region Constructor

        public TextStyle(TextStyleTableRecord recordParameter)
        {
            OriginalRecord = recordParameter;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     This method HighLights the text in the drawing space that is of
        ///     the current style.
        /// </summary>
        public void HighLight()
        {
            IsHighLighted = HighLightSwitch();
        }

        /// <summary>
        ///     This method UnHighLights the text that is currently higlighted.
        /// </summary>
        public void UnHighLight()
        {
            IsHighLighted = HighLightSwitch();
        }

        private bool HighLightSwitch()
        {
            using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
            {
                using (var modelSpace = (BlockTableRecord)
                    trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Database), OpenMode.ForRead))
                {
                    IEnumerable<dynamic> textObjs = modelSpace.Cast<ObjectId>()
                        .Select(objId => trans.GetObject(objId, OpenMode.ForWrite))
                        .Where(dbObj => ( dbObj is DBText || dbObj is MText ) &&
                                        ( (dynamic) dbObj ).TextStyleId.Equals(ObjectId))
                        .Select(txtObj => (dynamic) txtObj);

                    foreach (var obj in textObjs)
                        if (!IsHighLighted)
                            obj.HightLight();
                        else
                            obj.UnHighLight();
                }
                trans.Commit();
            }
            return !IsHighLighted;
        }

        #endregion
    }
}