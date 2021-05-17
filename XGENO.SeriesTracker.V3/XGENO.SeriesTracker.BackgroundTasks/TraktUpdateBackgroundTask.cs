using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.BackgroundTasks
{
    public sealed class TraktUpdateBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a deferral, to prevent the task from closing prematurely while asynchronous code is still running
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            try
            {
                //Initialize Settings
                var appSettings = new AppSettings();
                await appSettings.LoadSettings();

                //Sync the progress
                await AppServiceHelper.SyncTraktProgress(appSettings);
            }
            catch
            {
            }

            // Inform the system that the task is finished.
            deferral.Complete();
        }


    }

}
