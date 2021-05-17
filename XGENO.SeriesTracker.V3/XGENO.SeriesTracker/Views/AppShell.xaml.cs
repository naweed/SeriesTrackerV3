using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Common;
using XGENO.Framework.Services;
using System.Numerics;
using XGENO.Framework.Controls;
using Windows.UI.Xaml.Automation;
using Windows.ApplicationModel;
using System.Threading.Tasks;

namespace XGENO.SeriesTracker.Views
{
    public sealed partial class AppShell : Page
    {
        public static AppShell Instance { get; set; }
        public NavigationService NavigationService { get; set; }

        private List<NavMenuItem> topNavlist = new List<NavMenuItem>(
        new[]
        {
                            new NavMenuItem()
                            {
                                Symbol = "\ue29b",
                                Label = "EXPLORE",
                                DestPage = typeof(Views.DiscoverPage),
                                Arguments = "#|#DISCOVER#|#"
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue71e",
                                Label = "DISCOVER",
                                DestPage = typeof(SearchPage)
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue70c",
                                Label = "MY SHOWS",
                                DestPage = typeof(MyShowsPage)
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue787",
                                Label = "UPCOMING",
                                DestPage = typeof(UpcomingSchedulePage)
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue730",
                                Label = "MISSED",
                                DestPage = typeof(MissedEpisodesPage)
                            },
                            //new NavMenuItem()
                            //{
                            //    Symbol = "\ue8f1",
                            //    Label = "STATISTICS",
                            //    DestPage = typeof(StatisticsPage)
                            //}
        });



        private List<NavMenuItem> bottomNavlist = new List<NavMenuItem>(
        new[]
        {
                            new NavMenuItem()
                            {
                                Symbol = "\ue115",
                                Label = "SETTINGS",
                                DestPage = typeof(SettingsPage)
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue939",
                                Label = "FEEDBACK",
                                ItemType = "Feedback"
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue209",
                                Label = "RATE & REVIEW",
                                ItemType = "Rate"
                            },
                            new NavMenuItem()
                            {
                                Symbol = "\ue714",
                                Label = "INTRO",
                                DestPage = typeof(TrailerPlayPage),
                                Arguments = "ms-appx:///Assets/Intro.mp4"
                            }
                            
        });

        public AppShell(NavigationService navigationService)
        {
            Instance = this;
            this.NavigationService = navigationService;

            this.InitializeComponent();

            RootSplitView.Content = navigationService.Frame;

            //Set the Menu Items Source
            TopNavMenuList.ItemsSource = topNavlist;
            BottomNavMenuList.ItemsSource = bottomNavlist;

            //Set Minimum Size
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(600, 500));

        }

        private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        private async void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            if (item != null)
            {
                switch (item.ItemType)
                {
                    case "Page":
                        if (item.DestPage != null && item.DestPage != this.NavigationService.CurrentPageType)
                        {
                            await this.NavigationService.NavigateAsync(item.DestPage, item.Arguments?.ToString());
                        }
                        break;
                    case "Feedback":
                        await SendFeedbackEmail();
                        //await LaunchFeedbackHub();
                        break;
                    case "Rate":
                        await RateApp();
                        break;
                    default:
                        break;
                }

            }
        }

        private async Task RateApp()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName));
        }

        private async Task SendFeedbackEmail()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(@"mailto:WinApps@xgeno.com?subject=Series Tracker V3 - Feedback"));
        }

        private async Task LaunchFeedbackHub()
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            var colorToUse = Color.FromArgb(255, 69, 90, 100);

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = colorToUse;
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundOpacity = 1;
                //statusBar.HideAsync();
            }
            else
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = colorToUse;
                titleBar.ForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = colorToUse;
                titleBar.ButtonForegroundColor = Colors.White;

            }
        }

        public static async void SetBusyIndicator(string text)
        {
            await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
            {
                Instance.ProgressControl.IsActive = !string.IsNullOrEmpty(text);
                Instance.BusyIndicatorGrid.Visibility = !string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
                Instance.BusyText.Text = "";
                Instance.BusyText2.Text = "";
                
                if(!string.IsNullOrEmpty(text))
                {
                    try
                    {
                        string[] busyText = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        Instance.BusyText.Text = busyText[0] ?? "LOADING";
                        Instance.BusyText2.Text = busyText[1]?.Replace("#|#", Environment.NewLine) ?? "";
                    }
                    catch
                    {
                    }
                }

            });
        }

        private void Hanburger_Clicked(object sender, RoutedEventArgs e)
        {
            RootSplitView.IsPaneOpen = !RootSplitView.IsPaneOpen;
        }
    }
}
