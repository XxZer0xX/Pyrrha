#region Referenceing

using System;

#endregion

namespace Pyrrha
{
    public struct TransId : IComparable<TransId>
    {
        internal IntPtr RefPtr;

        public TransId( IntPtr intPtr )
        {
            RefPtr = intPtr;
        }

        public bool Equals( TransId other )
        {
            return RefPtr.Equals( other.RefPtr );
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) ) return false;
            return obj is TransId && Equals( (TransId) obj );
        }

        public override int GetHashCode()
        {
            return RefPtr.GetHashCode();
        }

        public static bool operator !=( TransId a, TransId b )
        {
            return !a.RefPtr.Equals( b.RefPtr );
        }

        public static bool operator ==( TransId a, TransId b )
        {
            return a.RefPtr.Equals( b.RefPtr );
        }

        public static bool operator <( TransId value1, TransId value2 )
        {
            return value1.RefPtr.ToInt64() < value2.RefPtr.ToInt64();
        }

        public static bool operator >( TransId value1, TransId value2 )
        {
            return value1.RefPtr.ToInt64() > value2.RefPtr.ToInt64();
        }

        public int CompareTo( TransId other )
        {
            return Convert.ToInt16( RefPtr == other.RefPtr );
        }
    }
}