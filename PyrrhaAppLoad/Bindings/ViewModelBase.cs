#region Referenceing

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PyrrhaAppLoad.Properties;

#endregion

namespace PyrrhaAppLoad.Bindings
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [NotifyPropertyChangedInvocator]
        protected virtual void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null,
            params string[] notifiedProperties)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            Notify(propertyName);
            notifiedProperties?.ForEach(Notify);
        }
        
    }
}