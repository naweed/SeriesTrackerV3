using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using XGENO.Framework.Services;

namespace XGENO.Framework.Helpers
{
    public static class AppRatingHelper
    {
        // Name of the container for holding the settings.
        private static readonly string UniversalRateReminderContainerName = "ApplicationRateReminder";

        // Launch count setting property name.
        private static readonly string CountPropertyName = "Count";

        // Dismissed setting property name.
        private static readonly string DismissedPropertyName = "Dismissed";

        // Container for saving the settings.
        private static ApplicationDataContainer reminderContainer;

        // The default number of times the application needs to be launched before showing the reminder. The default value is 5.
        public static readonly int DefaultLaunchLimitForReminder = 25;

        // The title for the rate pop up. The default value is "Rate us!".
        public static string Title { get; set; }

        // The text content for the rate pop up. The default value is "Your feedback helps you improve this app. If you like it, please take a minute and rate it with five stars so we can continue working on new features and updates.".
        public static string Content { get; set; }

        // The text for the rate button. The default value is "Rate".
        public static string RateButtonText { get; set; }

        // The text for the cancel button. The default value is "No, thanks!".
        public static string CancelButtonText { get; set; }

        // The number of times the applications needs to be launched before showing the reminder. The default value is 25.
        public static int LaunchLimit { get; set; }

        // Static constructor for initializing default values.
        static AppRatingHelper()
        {
            Title = "Rate us!";
            Content = "Your feedback helps you improve this app. If you like it, please take a minute and rate it with five stars so we can continue working on new features and updates.";
            RateButtonText = "Rate";
            CancelButtonText = "No, thanks!";
            LaunchLimit = DefaultLaunchLimitForReminder;

            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(UniversalRateReminderContainerName))
            {
                ResetLaunchCount();
            }

            reminderContainer = ApplicationData.Current.LocalSettings.Containers[UniversalRateReminderContainerName];
        }

        // Increments the launch counter and if it is equal or greater than the current value of LaunchCount, shows the rating pop up. A flag will be set so the dialog only shows once.
        public static void CheckRateReminder()
        {
            if (((bool)reminderContainer.Values[DismissedPropertyName]) == false)
            {
                int launchCount = (int) reminderContainer.Values[CountPropertyName];
                launchCount++;
                reminderContainer.Values[CountPropertyName] = launchCount;

                if (launchCount >= LaunchLimit)
                {
                    var dialogService = new DialogService();
                    var cancelButton = new UICommand(CancelButtonText, e => { reminderContainer.Values[DismissedPropertyName] = true; });
                    var rateButton = new UICommand(RateButtonText, e =>
                    {
                        //Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
                        Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName));

                        reminderContainer.Values[DismissedPropertyName] = true;
                    });

                    dialogService.ShowAsync(Content, Title, rateButton, cancelButton);
                }
            }
        }

        // Resets the stored launch count to zero, and resets the flag that prevents the pop up from showing more than once.
        public static void ResetLaunchCount()
        {
            if (ApplicationData.Current.LocalSettings.Containers.ContainsKey(UniversalRateReminderContainerName))
            {
                ApplicationData.Current.LocalSettings.DeleteContainer(UniversalRateReminderContainerName);
            }

            reminderContainer = ApplicationData.Current.LocalSettings.CreateContainer(UniversalRateReminderContainerName, ApplicationDataCreateDisposition.Always);
            reminderContainer.Values.Add(CountPropertyName, (int)0);
            reminderContainer.Values.Add(DismissedPropertyName, (bool)false);
        }

    }
}
