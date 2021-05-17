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
    public class AppReminderHelper
    {
        // Name of the container for holding the settings.
        private string reminderContainerName;

        // The title for the pop up.
        private string title { get; set; }

        // The text content to be displayed
        private string content { get; set; }

        // The text for the Close button. The default value is "Got It!".
        private string closeButtonText { get; set; }

        // The number of times the applications needs to be launched before showing the reminder. The default value is 25.
        private int launchLimit { get; set; }

        // Container for saving the settings.
        private ApplicationDataContainer reminderContainer;
        
        // Launch count setting property name.
        private string countPropertyName = "Count";

        //Constructor
        public AppReminderHelper(string ReminderContainerName, string Title, string Content, int LaunchLimit = 25, string CloseButtonText = "Got it!")
        {
            reminderContainerName = ReminderContainerName;
            title = Title;
            content = Content;
            launchLimit = LaunchLimit;
            closeButtonText = CloseButtonText;

            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(reminderContainerName))
            {
                ResetLaunchCount();
            }

            reminderContainer = ApplicationData.Current.LocalSettings.Containers[reminderContainerName];
        }


        // Resets the stored launch count to zero, and resets the flag that prevents the pop up from showing more than once.
        public void ResetLaunchCount()
        {
            if (ApplicationData.Current.LocalSettings.Containers.ContainsKey(reminderContainerName))
            {
                ApplicationData.Current.LocalSettings.DeleteContainer(reminderContainerName);
            }

            reminderContainer = ApplicationData.Current.LocalSettings.CreateContainer(reminderContainerName, ApplicationDataCreateDisposition.Always);
            reminderContainer.Values.Add(countPropertyName, (int)0);
        }

        
        // Increments the launch counter and if it is equal or greater than the current value of LaunchCount, shows the pop up.
        public void ShowReminder(bool isTotast = false)
        {
            int launchCount = (int)reminderContainer.Values[countPropertyName];

            launchCount++;

            reminderContainer.Values[countPropertyName] = launchCount;

            if (launchCount == launchLimit)
            {
                if (!isTotast)
                {
                    var dialogService = new DialogService();

                    var closeButton = new UICommand(closeButtonText, e => { /*Do nothing*/ });

                    dialogService.ShowAsync(content, title, closeButton);
                }
                else
                {
                    var toastService = new ToastNotificationService();
                    toastService.ShowSimpleToast(content, title);
                }
            }
        }


    }
}
