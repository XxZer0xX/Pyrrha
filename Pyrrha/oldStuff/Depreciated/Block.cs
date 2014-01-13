#region Referenceing

using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Pyrrha.Collections;

#endregion

namespace Pyrrha
{
    [Obsolete("Depreciated",true)]
    public class Block 
    {
        public readonly BlockReference OriginalBlockReference;
        private AttributeDictionary _attributes;

        #region Properties

        public Database Database
        {
            get { return OriginalBlockReference.Database; }
        }

        /// <summary>
        ///     Checks if the block has been erased.
        /// </summary>
        public bool IsErased
        {
            get { return OriginalBlockReference.IsErased; }
        }

        /// <summary>
        ///     The current blocks Name.
        /// </summary>
        public string Name
        {
            get { return OriginalBlockReference.Name; }
        }

        /// <summary>
        ///     The current blocks Handle.
        /// </summary>
        public Handle Handle
        {
            get { return OriginalBlockReference.Handle; }
        }

        /// <summary>
        ///     A collection of attributes associated with the current block.
        /// </summary>
        public AttributeDictionary Attributes
        {
            get
            {
                var rtn = _attributes ??
                       ( _attributes = new AttributeDictionary(OriginalBlockReference.AttributeCollection) );
                return rtn;
            }
        }

        /// <summary>
        ///     the current blocks ObjectId.
        /// </summary>
        public ObjectId ObjectId
        {
            get { return OriginalBlockReference.ObjectId; }
        }

        /// <summary>
        ///     The current blocks BlockName
        /// </summary>
        public string BlockName
        {
            get { return OriginalBlockReference.BlockName; }
        }

        /// <summary>
        ///     The current blocks LayerRecord.
        ///     *** Read Only ***
        /// </summary>
        public LayerTableRecord LayerRecord
        {
            get
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                    return (LayerTableRecord) trans.GetObject(OriginalBlockReference.LayerId, OpenMode.ForRead);
            }
        }

        /// <summary>
        ///     The current blocks color by integer.
        /// </summary>
        public short ColorIndex
        {
            get { return (short) OriginalBlockReference.ColorIndex; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (BlockReference) trans.GetObject(OriginalBlockReference.ObjectId, OpenMode.ForWrite) ).Color = Color
                        .FromColorIndex(
                            ColorMethod.ByAci, value);
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     The current blocks color by string.
        /// </summary>
        public string ColorString
        {
            get { return OriginalBlockReference.Color.ColorNameForDisplay; }
        }

        /// <summary>
        ///     The current blocks Linetype.
        /// </summary>
        public string Linetype
        {
            get { return OriginalBlockReference.Linetype; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var lineTable = (LinetypeTable) trans.GetObject(Database.LinetypeTableId, OpenMode.ForRead);
                    if (!lineTable.Has(value))
                        Pyrrha.LoadLinetype(value);

                    ( (BlockReference) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Linetype = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     The current blocks X,Y,Z insertion point.
        /// </summary>
        public Point3d Position
        {
            get { return OriginalBlockReference.Position; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (BlockReference) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Position = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     The current blocks degree of rotation
        /// </summary>
        public double Rotation
        {
            get { return OriginalBlockReference.Rotation; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (BlockReference) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Rotation = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     The current blocks ScaleFactor.
        /// </summary>
        public Scale3d ScaleFactors
        {
            get { return OriginalBlockReference.ScaleFactors; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (BlockReference) trans.GetObject(ObjectId, OpenMode.ForWrite) ).ScaleFactors = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     If the current block is visible.
        /// </summary>
        public bool Visible
        {
            get { return OriginalBlockReference.Visible; }
            set
            {
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (BlockReference) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Visible = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     The count of how many attributes the current block contains.
        /// </summary>
        public int AttributeCount
        {
            get { return OriginalBlockReference.AttributeCollection.Count; }
        }

        /// <summary>
        ///     If the current block has attributes.
        /// </summary>
        public bool HasAttributes
        {
            get { return ( OriginalBlockReference.AttributeCollection.Count > 0 ); }
        }

        #endregion

        #region Constructor

        public Block(BlockReference blockRefParameter)
        {
            OriginalBlockReference = blockRefParameter;
        }

        #endregion

        #region Methods

        public void Erase()
        {
            using (var blk = ObjectId.Open(OpenMode.ForWrite))
            blk.Erase();
        }

        #endregion
    }
}