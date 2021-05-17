using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XGENO.Framework.Common;
using XGENO.Framework.Services;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.DataModels;


namespace XGENO.Framework.MVVM
{
    public abstract class ViewModelBase : BindableBase, INavigable
    {
        [JsonIgnore]
        public virtual NavigationService NavigationService { get; set; }

        [JsonIgnore]
        public virtual DialogService DialogService { get; set; }

        [JsonIgnore]
        public virtual ToastNotificationService ToastNotificationService { get; set; }

        [JsonIgnore]
        public virtual NetworkAvailableService NetworkAvailableService { get; set; }
        
        [JsonIgnore]
        public virtual DispatcherWrapper Dispatcher { get; set; }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        
        public ViewModelBase()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                this.DialogService = new DialogService();
                this.ToastNotificationService = new ToastNotificationService();
                this.NetworkAvailableService = new NetworkAvailableService();
            }
        }

        
        public virtual Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            return Task.CompletedTask;
        }
    }
}