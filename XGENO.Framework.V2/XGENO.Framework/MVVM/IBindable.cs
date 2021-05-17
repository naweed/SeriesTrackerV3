using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XGENO.Framework.MVVM
{
    internal interface IBindable : INotifyPropertyChanged
    {
        void RaisePropertyChanged([CallerMemberName]string propertyName = null);
    }
}
