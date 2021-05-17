using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Common;
using XGENO.Framework.Enums;
using XGENO.Framework.Helpers;
using XGENO.Framework.MVVM;
using XGENO.Framework.Services;

namespace XGENO.Framework.Services
{
    public class NavigationService
    {
        public Frame Frame { get; set; }
        public NavigationMode NavigationModeHint = NavigationMode.New;

        public Type CurrentPageType { get; internal set; }
        public string CurrentPageParam { get; internal set; }
        public Type LastPageType { get; internal set; }
        public string LastPageParam { get; internal set; }


        public bool CanGoBack => Frame.CanGoBack;
        public bool CanGoForward => Frame.CanGoForward;
        public DispatcherWrapper Dispatcher => WindowWrapper.Current(this).Dispatcher;


        protected internal NavigationService(Frame frame)
        {
            Frame = frame;

            Frame.Navigated += async (s, e) =>
            {
                var parameter = e.Parameter?.ToString();

                var currentContent = Frame.Content;

                if (Equals(e.Parameter?.ToString(), LastPageParam?.ToString()))
                    parameter = LastPageParam;

                await WindowWrapper.Current().Dispatcher.DispatchAsync(async () =>
                        {
                            try
                            {
                                if (currentContent == Frame.Content)
                                    await NavigateToAsync(e.NavigationMode, parameter, Frame.Content);
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                        },
                        1);
            };

            Frame.Navigating += async (s, e) =>
            {
                await WindowWrapper.Current().Dispatcher.DispatchAsync(async () =>
                {
                    try
                    {
                        await NavigateFromAsync(Frame.Content);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                },
                        1);
            };

            // setup animations
            var t = new NavigationThemeTransition
            {
                DefaultNavigationTransitionInfo = new ContinuumNavigationTransitionInfo()
            };

            Frame.ContentTransitions = new TransitionCollection { };
            Frame.ContentTransitions.Add(t);


        }

        public void GoBack()
        {
            NavigationModeHint = NavigationMode.Back;

            if (CanGoBack)
                Frame.GoBack();
        }

        public void GoForward()
        {
            NavigationModeHint = NavigationMode.Forward;

            if (CanGoForward)
                Frame.GoForward();
        }

        public void ClearHistory()
        {
            Frame.BackStack.Clear();
        }

        public async void Navigate(Type page, string parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            await NavigateAsync(page, parameter, infoOverride);
        }

        public async Task<bool> NavigateAsync(Type page, string parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if ((page.FullName == LastPageType?.ToString()) && (parameter?.ToString() == LastPageParam?.ToString()))
                return false;

            await Task.CompletedTask;

            return FrameNavigate(page, parameter, infoOverride);
        }

        private bool FrameNavigate(Type page, object parameter, NavigationTransitionInfo infoOverride)
        {
            if (Frame.Navigate(page, parameter, infoOverride))
            {
                return page.Equals(Frame.Content?.GetType());
            }
            else
            {
                return false;
            }
        }

        async Task NavigateToAsync(NavigationMode mode, string parameter, object frameContent = null)
        {
            frameContent = frameContent ?? Frame.Content;

            LastPageParam = parameter;
            LastPageType = frameContent.GetType();

            var page = frameContent as Page;

            if (page != null)
            {
                var dataContext = page.DataContext as INavigable;

                if (dataContext != null)
                {
                    // prepare for state load
                    dataContext.NavigationService = this;
                    dataContext.Dispatcher = WindowWrapper.Current(this)?.Dispatcher;

                    await dataContext.OnNavigatedToAsync(parameter, mode);

                    // update bindings after NavTo initializes data
                    XamlHelpers.InitializeBindings(page);
                    XamlHelpers.UpdateBindings(page);

                }
            }
        }

        async Task NavigateFromAsync(object frameContent = null)
        {
            frameContent = frameContent ?? Frame.Content;

            var page = frameContent as Page;

            if (page != null)
            {
                var dataContext = page.DataContext as INavigable;

                if (dataContext != null)
                {
                    dataContext = null;

                    XamlHelpers.StopTrackingBindings(page);

                    GC.Collect();
                }
            }

            await Task.CompletedTask;
        }

    }
}

