using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XGENO.SeriesTracker.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using Windows.ApplicationModel.Background;

namespace XGENO.SeriesTracker.Views
{
    public sealed partial class MyShowsPage : Page
    {
        // DataTransfer Request Manager
        private DataTransferManager dataTransferManager;

        private Show showToShare;


        public MyShowsPage()
        {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Register the current page as a share source.
            this.dataTransferManager = DataTransferManager.GetForCurrentView();
            this.dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);

            //Register the background tasks
            this.RegisterBackgroundTask();

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Unregister the current page as a share source.
            this.dataTransferManager.DataRequested -= new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            var deferral = e.Request.GetDeferral();

            e.Request.Data.Properties.Title = showToShare.ShowTitle;

            var shareText = "Hey there," + Environment.NewLine + Environment.NewLine;

            if (showToShare.WatchedPercentage > 0)
                shareText += "I am currently watching this great show, and have already watched " + showToShare.WatchedEpisodesCount.ToString() + " episodes.";
            else
                shareText += "I am going to start watcing this great show.";

            shareText += " You should check it out too.";

            shareText += Environment.NewLine + Environment.NewLine;

            shareText += "ShowImage";

            shareText += "ShowURL" + Environment.NewLine;

            shareText += showToShare.ShowTitle + Environment.NewLine + Environment.NewLine;
            shareText += showToShare.overview + Environment.NewLine + Environment.NewLine + Environment.NewLine;

            shareText += "Shared using \"SERIES TRACKER\" Windows 10 app: " + "<<XGENO_App_URL>>" + Environment.NewLine + Environment.NewLine;

            e.Request.Data.SetText(shareText.Replace("ShowURL", "http://trakt.tv/shows/" + showToShare.ids.slug).Replace("ShowImage", "").Replace("<<XGENO_App_URL>>", "https://www.microsoft.com/store/apps/9nblggh3slj9"));
            e.Request.Data.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(shareText.Replace(Environment.NewLine, "<BR/>").Replace("ShowURL", "<a href='" + "http://trakt.tv/shows/" + showToShare.ids.slug + "'>" + "http://trakt.tv/shows/" + showToShare.ids.slug + "</a>").Replace("ShowImage", "<img width='300' height='200' src='" + showToShare.BackgroundImage + "' /><BR/><BR/><BR/>").Replace("<<XGENO_App_URL>>", "<a href='https://www.microsoft.com/store/apps/9nblggh3slj9'>https://www.microsoft.com/store/apps/9nblggh3slj9</a>")));

            deferral.Complete();
        }


        protected void Share_Click(object sender, RoutedEventArgs e)
        {
            showToShare = ((Button)sender).DataContext as Show;

            DataTransferManager.ShowShareUI();
        }

        private async void RegisterBackgroundTask()
        {

            bool registerLiveTileTask = true;
            bool registerTraktUpdateTask = true;

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;

            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity || backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity || backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "XGENOSeriesBackgroundTask")
                    {
                        if (DateTime.Now > appSettings.MainTaskLastSyncDate.AddDays(1))
                        {
                            task.Value.Unregister(true);
                            registerLiveTileTask = true;
                        }
                        else
                        {
                            registerLiveTileTask = false;
                        }
                    }

                    if (task.Value.Name == "XGENOSeriesTraktUpdateBackgroundTask")
                    {
                        if (DateTime.Now > appSettings.TraktTaskLastSyncDate.AddDays(1))
                        {
                            task.Value.Unregister(true);
                            registerTraktUpdateTask = true;
                        }
                        else
                        {
                            registerTraktUpdateTask = false;
                        }
                    }
                }


                BackgroundTaskBuilder taskBuilder;

                if (registerLiveTileTask)
                {
                    taskBuilder = new BackgroundTaskBuilder();
                    taskBuilder.Name = "XGENOSeriesBackgroundTask";
                    taskBuilder.TaskEntryPoint = "XGENO.SeriesTracker.BackgroundTasks.SeriesBackgroundTask";
                    taskBuilder.SetTrigger(new TimeTrigger(AppSettings.LiveTilesDuration, false));
                    taskBuilder.Register();
                    appSettings.MainTaskLastSyncDate = DateTime.Now;
                }

                if (registerTraktUpdateTask)
                {
                    taskBuilder = new BackgroundTaskBuilder();
                    taskBuilder.Name = "XGENOSeriesTraktUpdateBackgroundTask";
                    taskBuilder.TaskEntryPoint = "XGENO.SeriesTracker.BackgroundTasks.TraktUpdateBackgroundTask";
                    taskBuilder.SetTrigger(new TimeTrigger(AppSettings.TraktUpdateDuration, false));
                    taskBuilder.Register();
                    appSettings.TraktTaskLastSyncDate = DateTime.Now;
                }
            }
        }


    }
}
