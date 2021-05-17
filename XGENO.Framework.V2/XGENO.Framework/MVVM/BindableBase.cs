using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using XGENO.Framework.Common;

namespace XGENO.Framework.MVVM
{
    public abstract class BindableBase : IBindable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(storage, value))
                return false;

            storage = value;
            this.RaisePropertyChanged(propertyName);

            return true;
        }

        public virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;

            var handler = PropertyChanged;

            if (!object.Equals(handler, null))
            {
                var args = new PropertyChangedEventArgs(propertyName);
                var dispatcher = WindowWrapper.Current().Dispatcher;
                
                if (dispatcher.HasThreadAccess())
                {
                    try
                    {
                        handler.Invoke(this, args);
                    }
                    catch
                    {
                        dispatcher.Dispatch(() => handler.Invoke(this, args));
                    }
                }
                else
                {
                    dispatcher.Dispatch(() => handler.Invoke(this, args));
                }
            }
        }
    }
}
