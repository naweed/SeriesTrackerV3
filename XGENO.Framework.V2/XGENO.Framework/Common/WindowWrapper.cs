using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Core;
using XGENO.Framework.Services;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace XGENO.Framework.Common
{
    public class WindowWrapper
    {
        public readonly static List<WindowWrapper> ActiveWrappers = new List<WindowWrapper>();

        public Window Window { get; }

        public DispatcherWrapper Dispatcher { get; }

        public List<NavigationService> NavigationServices { get; } = new List<NavigationService>();


        static WindowWrapper()
        {
        }

        public WindowWrapper()
        {
        }

        internal WindowWrapper(Window window)
        {
            if (ActiveWrappers.Any(x => x.Window == window))
                throw new Exception("Windows already has a wrapper; use Current(window) to fetch.");

            Window = window;
            ActiveWrappers.Add(this);
            Dispatcher = new DispatcherWrapper(window.Dispatcher);
            window.Closed += (s, e) => { ActiveWrappers.Remove(this); };
        }

        public static WindowWrapper Default()
        {
            try
            {
                var mainDispatcher = CoreApplication.MainView.Dispatcher;

                return ActiveWrappers.FirstOrDefault(x => x.Window.Dispatcher == mainDispatcher) ?? ActiveWrappers.FirstOrDefault();
            }
            catch (COMException)
            {
                //MainView might exist but still be not accessible
                return ActiveWrappers.FirstOrDefault();
            }
        }

        public void Close() 
        { 
            Window.Close(); 
        }

        public static WindowWrapper Current() => ActiveWrappers.FirstOrDefault(x => x.Window == Window.Current) ?? Default();

        public static WindowWrapper Current(Window window) => ActiveWrappers.FirstOrDefault(x => x.Window == window);
        public static WindowWrapper Current(NavigationService nav) => ActiveWrappers.FirstOrDefault(x => x.NavigationServices.Contains(nav)); 

        public DisplayInformation DisplayInformation() => Dispatcher.Dispatch(() => Windows.Graphics.Display.DisplayInformation.GetForCurrentView());

        public ApplicationView ApplicationView() => Dispatcher.Dispatch(() => Windows.UI.ViewManagement.ApplicationView.GetForCurrentView());

        public UIViewSettings UIViewSettings() => Dispatcher.Dispatch(() => Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView());

    }
}
