using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Common;
using XGENO.Framework.Enums;
using XGENO.Framework.Helpers;
using XGENO.Framework.Services;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker
{
    sealed partial class App : AppBase
    {
        public AppSettings Settings { get; set; }

        public App()
        {
            InitializeComponent();

            UnhandledException += App_UnhandledException;

            SplashFactory = (e) => new Views.Splash(e);
        }

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var navService = NavigationServiceFactory(ExistingContent.Exclude);

            return new Views.AppShell(navService);
        }


        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Settings = new AppSettings();
            await Settings.LoadSettings();


        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            //Load Tracked Lists
            await SQLiteService.Instance.LoadTrackingShowsList();

            if (AppSettings.TrackingList.Count > 0)
            {
                if(Settings.StartPageIsWatchList)
                    await NavigationService.NavigateAsync(typeof(Views.MyShowsPage));
                else
                    await NavigationService.NavigateAsync(typeof(Views.MissedEpisodesPage));
            }
            else
                await NavigationService.NavigateAsync(typeof(Views.DiscoverPage), "#|#DISCOVER#|#");

            //App Rating Reminder
            AppRatingHelper.LaunchLimit = 25;
            AppRatingHelper.RateButtonText = "rate";
            AppRatingHelper.CancelButtonText = "cancel";
            AppRatingHelper.Title = "Rate & Review";
            AppRatingHelper.Content = "You have been using this application for quite some time. We hope you have liked it. Would you like to take a minute to rate our app?";
            AppRatingHelper.CheckRateReminder();

            ////Check for Version 3 Reminder
            //var v3Reminder = new AppReminderHelper("XGENO.v3.0", "SERIES TRACKER 3.0", "We are pleased to announce version 3.0 of the app." + Environment.NewLine + Environment.NewLine + "We value your feedback. Please send your comments to WinApps@xgeno.com to help us make this application even better.", 2);
            //v3Reminder.ShowReminder();


            ////Check for Release Notes Reminder
            //var releaseNotesReminder = new AppReminderHelper("XGENO.ReleaseNotes.v2.0", "New Features", "Please make sure to check the Release Notes in the help section every once in a while to learn about the new features added, or bugs fixed.", 10);
            //releaseNotesReminder.ShowReminder();

            ////V3.1.8 Reminder
            //var releaseNotesReminder = new AppReminderHelper("XGENO.SeriesTracker.v3.1.8", "BUG FIX", "Issue with Live Tiles has been fixed.", 1);
            //releaseNotesReminder.ShowReminder(true);

            //V3.5 Reminder
            var releaseNotesReminder = new AppReminderHelper("XGENO.SeriesTracker.v3.5.0", "GONE FREE", "Your favorite Series Tracker is now FREE. Hope you enjoy the full featured version.", 1);
            releaseNotesReminder.ShowReminder(true);



        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs arg)
        {
            arg.Handled = true;
        }
    }
}
