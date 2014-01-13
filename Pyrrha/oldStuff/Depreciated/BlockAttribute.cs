using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Pyrrha
{
    [Obsolete("Depreciated", true)]
    public class BlockAttribute
    {
        public AttributeReference OriginalAttributeReference;

        #region Properties

        private Database Database
        {
            get { return OriginalAttributeReference.Database; }
        }

        /// <summary>
        /// 	The current attributes Handle
        /// </summary>
        public Handle Handle
        {
            get { return OriginalAttributeReference.Handle; }
        }

        /// <summary>
        /// 	The current attributes ObjectId
        /// </summary>
        public ObjectId ObjectId
        {
            get { return OriginalAttributeReference.ObjectId; }
        }

        /// <summary>
        /// 	The current attributes Position
        /// </summary>
        public Point3d Position
        {
            get { return OriginalAttributeReference.Position; }
        }

        /// <summary>
        /// 	The current attributes Rotation
        /// </summary>
        public double Rotation
        {
            get { return OriginalAttributeReference.Rotation; }
            set
            {
                Modifying(new ModifiedEventArgs<double> { ValueBefore = Rotation , ValueAfter = value });
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ((AttributeReference)trans.GetObject(ObjectId , OpenMode.ForWrite)).Rotation = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// 	The current attributes Tag
        /// </summary>
        public string Tag
        {
            get { return OriginalAttributeReference.Tag; }
        }

        /// <summary>
        /// 	The current attributes value
        /// </summary>
        public string TextString
        {
            get { return OriginalAttributeReference.TextString; }
            set
            {
                Modifying(new ModifiedEventArgs<string> { ValueBefore = TextString , ValueAfter = value });
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ((AttributeReference)trans.GetObject(ObjectId , OpenMode.ForWrite)).TextString = value;
                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// 	The current attributes visibility
        /// </summary>
        public bool Visible
        {
            get { return OriginalAttributeReference.Visible; }
            set
            {
                Modifying(new ModifiedEventArgs<bool> { ValueBefore = Visible , ValueAfter = value });
                using (OpenCloseTransaction trans = Database.TransactionManager.StartOpenCloseTransaction())
                {
                    ((AttributeReference)trans.GetObject(ObjectId , OpenMode.ForWrite)).Visible = value;
                    trans.Commit();
                }
            }
        }


        #endregion

        #region Constructor

        public BlockAttribute(AttributeReference attributeRefParameter)
        {
            OriginalAttributeReference = attributeRefParameter;
        }

        #endregion


        #region Events

        public event ModifiedEventHandler Modified;

        private void Modifying<T>(ModifiedEventArgs<T> modifiedEventArgs, object sender = null)
        {
            if (Modified != null)
                Modified(sender ?? this, modifiedEventArgs);
        }

        #endregion
    }
}
