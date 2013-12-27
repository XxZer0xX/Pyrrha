
#region Referenceing

using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

#endregion

namespace Pyrrha
{
    public class Layer
    {
        public LayerTableRecord OriginalRecord;
        private string _linetypeName;

        #region Properties

        private Database Database
        {
            get { return OriginalRecord.Database; }
        }

        /// <summary>
        ///     Current layers name.
        /// </summary>
        public string Name
        {
            get { return OriginalRecord.Name; }
            set
            {
                Modifying(new ModifiedEventArgs<string> {ValueBefore = Name, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Name = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current layers color.
        ///     ****** READONLY ******
        /// </summary>
        public string ColorName
        {
            get { return OriginalRecord.Color.ColorNameForDisplay; }
        }

        /// <summary>
        ///     Layers XData
        /// </summary>
        public ResultBuffer XData
        {
            get { return OriginalRecord.XData; }
            set
            {
                Modifying(new ModifiedEventArgs<ResultBuffer> {ValueBefore = XData, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( trans.GetObject(ObjectId, OpenMode.ForWrite) ).XData = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current Layers LineWeight.
        /// </summary>
        public LineWeight LineWeight
        {
            get { return OriginalRecord.LineWeight; }
            set
            {
                Modifying(new ModifiedEventArgs<LineWeight> {ValueBefore = LineWeight, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).LineWeight = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current layers linetype.
        /// </summary>
        public string LineType
        {
            get
            {
                using (DBObject lineTypeRecord = OriginalRecord.LinetypeObjectId.Open(OpenMode.ForRead))
                    return _linetypeName ??
                           ( _linetypeName = ( (LinetypeTableRecord) lineTypeRecord ).Name );
            }
            set
            {
                Modifying(new ModifiedEventArgs<string> {ValueBefore = LineType, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    var lineTable = (LinetypeTable) trans.GetObject(Database.LinetypeTableId, OpenMode.ForRead);
                    if (!lineTable.Has(value))
                        StaticExtenstions.LoadLinetype(value);

                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).LinetypeObjectId =
                        lineTable[value];
                    _linetypeName = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Current layers color.
        /// </summary>
        public Color Color
        {
            get { return OriginalRecord.Color; }
            set
            {
                Modifying(new ModifiedEventArgs<Color> {ValueBefore = Color, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).Color = value;
                    trans.Commit();
                }
            }
        }


        /// <summary>
        ///     Current layers color index.
        /// </summary>
        public short ColorIndex
        {
            get { return OriginalRecord.Color.ColorIndex; }
            set
            {
                Modifying(new ModifiedEventArgs<short> {ValueBefore = ColorIndex, ValueAfter = value});
                this.Color = Color.FromColorIndex(ColorMethod.ByAci , value);  
            }
        }

        /// <summary>
        ///     Current layers ObjectId.
        /// </summary>
        public ObjectId ObjectId
        {
            get { return OriginalRecord.ObjectId; }
        }

        /// <summary>
        ///     Checks if the layer has been erased.
        /// </summary>
        public bool IsErased
        {
            get { return OriginalRecord.IsErased; }
        }

        /// <summary>
        ///     Used to determine if the layer is hidden.
        /// </summary>
        public bool IsHidden
        {
            get { return OriginalRecord.IsHidden; }
            set
            {
                Modifying(new ModifiedEventArgs<bool> {ValueBefore = IsHidden, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).IsHidden = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Used to determine if the layer is frozen.
        /// </summary>
        public bool IsFrozen
        {
            get { return OriginalRecord.IsFrozen; }
            set
            {
                Modifying(new ModifiedEventArgs<bool> {ValueBefore = IsFrozen, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).IsFrozen = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Used to determine if the layer is locked.
        /// </summary>
        public bool IsLocked
        {
            get { return OriginalRecord.IsLocked; }
            set
            {
                Modifying(new ModifiedEventArgs<bool> {ValueBefore = IsLocked, ValueAfter = value});
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).IsLocked = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        ///     Used to determine if the layer is in use.
        /// </summary>
        public bool IsUsed
        {
            get { return OriginalRecord.IsUsed; }
        }

        /// <summary>
        ///     Used to determine if the layer is On or Off.
        /// </summary>
        public bool IsOff
        {
            get { return OriginalRecord.IsOff; }
            set
            {
                Modifying(new ModifiedEventArgs<bool> {ValueBefore = IsOff, ValueAfter = value});

                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ( (LayerTableRecord) trans.GetObject(ObjectId, OpenMode.ForWrite) ).IsOff = value;
                    trans.Commit();
                }
            }
        }

        #endregion

        #region Constructor

        public Layer()
        {
            OriginalRecord = new LayerTableRecord();
        }

        public Layer(LayerTableRecord recordParameter)
        {
            OriginalRecord = recordParameter;
        }

        #endregion

        #region Methods

        public void Erase()
        {
            if (Name.Equals("0")) return;
            using (DBObject layerRecord = ObjectId.Open(OpenMode.ForWrite))
                layerRecord.Erase();
            Erasing(EventArgs.Empty);
        }

        #endregion

        #region Events

        public event ModifiedEventHandler WillBeErased;
        public event ModifiedEventHandler Modified;

        protected virtual void Erasing(EventArgs eventArgs, object sender = null)
        {
            if (WillBeErased != null)
                WillBeErased(sender ?? this, eventArgs);
        }

        protected virtual void Modifying<T>(ModifiedEventArgs<T> eventArgs , object sender = null)
        {
            if (Modified != null)
                Modified(sender ?? this , eventArgs);
        }

        #endregion
    }
}