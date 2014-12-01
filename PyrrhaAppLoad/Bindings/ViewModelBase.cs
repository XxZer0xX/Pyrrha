#region Referenceing

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PyrrhaAppLoad.Properties;

#endregion

namespace PyrrhaAppLoad.Bindings
{

    #region Referenceing

    #endregion

    internal class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotifyPropertyChangedInvocator]
        protected virtual void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null,
            params string[] notifiedProperties)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;
            field = value;

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));

                if (notifiedProperties == null)
                    return;

                foreach (var notifiedProperty in notifiedProperties)
                {
                    handler(this, new PropertyChangedEventArgs(notifiedProperty));
                }
            }
        }
    }
}