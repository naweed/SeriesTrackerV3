using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Common;
using XGENO.Framework.DataModels;
using XGENO.Framework.Services;

namespace XGENO.Framework.MVVM
{
    public interface INavigable
    {
        NavigationService NavigationService { get; set; }
        DispatcherWrapper Dispatcher { get; set; }

        Task OnNavigatedToAsync(object parameter, NavigationMode mode);
    }
}
