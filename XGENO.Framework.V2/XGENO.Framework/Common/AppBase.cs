using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using XGENO.Framework.Enums;
using System.Threading.Tasks;
using XGENO.Framework.Services;
using XGENO.Framework.MVVM;
using XGENO.Framework.DataModels;

namespace XGENO.Framework.Common
{
    public abstract class AppBase : Application, INotifyPropertyChanged
    {
        #region Properties

        public const string DefaultTileID = "App";

        public bool ShowShellBackButton { get; set; } = true;

        public Func<SplashScreen, UserControl> SplashFactory { get; protected set; }
        public static new AppBase Current { get; private set; }

        public NavigationService NavigationService => WindowWrapper.Current().NavigationServices.FirstOrDefault();

        #endregion

        #region Constructor

        public AppBase()
        {
            Current = this;
            Resuming += HandleResuming;
            Suspending += HandleSuspending;
        }

        #endregion
        
        #region Suspension Management

        private async void HandleResuming(object sender, object e)
        {
            var args = OriginalActivatedArgs as LaunchActivatedEventArgs;

            if (args?.PrelaunchActivated ?? true)
            {
                OnResuming(sender, e, AppExecutionState.Prelaunch);

                var kind = args.PreviousExecutionState == ApplicationExecutionState.Running ? StartKind.Activate : StartKind.Launch;

                await CallOnStartAsync(false, kind);

                ActivateWindow(ActivateWindowSources.Resuming);
            }
            else
            {
                OnResuming(sender, e, AppExecutionState.Suspended);
            }
        }

        private async Task CallOnStartAsync(bool canRepeat, StartKind startKind)
        {
            if (!canRepeat && _HasOnStartAsync)
                return;

            _HasOnStartAsync = true;

            await OnStartAsync(startKind, OriginalActivatedArgs);
        }
        
        private async void HandleSuspending(object sender, SuspendingEventArgs e)
        {
            //Get Deferral
            var deferral = e.SuspendingOperation.GetDeferral();

            //To be done: Save the State

            //Complete Deferral
            deferral.Complete(); 
        }

        #endregion

        #region Overrides

        bool _HasOnStartAsync = false;

        public abstract Task OnStartAsync(StartKind startKind, IActivatedEventArgs args);

        public virtual Task OnPrelaunchAsync(IActivatedEventArgs args, out bool runOnStartAsync)
        {
            runOnStartAsync = false;

            return Task.CompletedTask;
        }

        public virtual Task OnInitializeAsync(IActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            return Task.CompletedTask;
        }

        public virtual void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
        }

        #endregion

        #region Activation
        
        public IActivatedEventArgs OriginalActivatedArgs { get; private set; }

        protected override sealed async void OnActivated(IActivatedEventArgs e) 
        { 
            await InternalActivatedAsync(e); 
        }

        protected override sealed async void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs args) 
        { 
            await InternalActivatedAsync(args); 
        }

        protected override sealed async void OnFileActivated(FileActivatedEventArgs args) 
        { 
            await InternalActivatedAsync(args); 
        }

        protected override sealed async void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs args) 
        {
            await InternalActivatedAsync(args); 
        }

        protected override sealed async void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs args) 
        { 
            await InternalActivatedAsync(args); 
        }

        protected override sealed async void OnSearchActivated(SearchActivatedEventArgs args) 
        { 
            await InternalActivatedAsync(args); 
        }

        protected override sealed async void OnShareTargetActivated(ShareTargetActivatedEventArgs args) 
        { 
            await InternalActivatedAsync(args); 
        }

        private async Task InternalActivatedAsync(IActivatedEventArgs e)
        {
            OriginalActivatedArgs = e;

            // sometimes activate requires a frame to be built
            if (Window.Current.Content == null)
            {
                await InitializeFrameAsync(e);
            }

            // onstart is shared with activate and launch
            await CallOnStartAsync(true, StartKind.Activate);

            // ensure active (this will hide any custom splashscreen)
            ActivateWindow(ActivateWindowSources.Activating);
        }

        private void ActivateWindow(ActivateWindowSources source)
        {
            if (source != ActivateWindowSources.SplashScreen)
            {
                CurrentState = States.ShowingContent;
            }

            Window.Current.Activate();
        }

        #endregion

        #region Frame Creation

        public States CurrentState { get; set; } = States.Starting;

        private async Task InitializeFrameAsync(IActivatedEventArgs e)
        {
            /*
                InitializeFrameAsync creates a default Frame preceeded by the optional 
                splash screen, then OnInitialzieAsync, then the new frame (if necessary).
                This is private because there's no reason for the developer to call this.
            */

            ShowSplashScreen(e);

            await CallOnInitializeAsync(true, e);

            // if there's custom content then there's nothing to do
            if (CurrentState == States.Splashing || Window.Current.Content == null)
            {
                Window.Current.Content = CreateRootElement(e);
            }
            else
            {
                // nothing: custom content
            }
        }

        #endregion

        #region Initialization / Splash Screen

        bool _HasOnInitializeAsync = false;

        public virtual UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var frame = new Frame();
            var nav = Current.NavigationServiceFactory(ExistingContent.Exclude, frame);

            return nav.Frame;
        }
        
        private async Task CallOnInitializeAsync(bool canRepeat, IActivatedEventArgs e)
        {
            if (!canRepeat && _HasOnInitializeAsync)
                return;

            _HasOnInitializeAsync = true;

            await OnInitializeAsync(e);
        }


        private void ShowSplashScreen(IActivatedEventArgs e)
        {
            if (SplashFactory != null && e.PreviousExecutionState != ApplicationExecutionState.Suspended)
            {
                CurrentState = States.Splashing;

                var splash = SplashFactory(e.SplashScreen);
                Window.Current.Content = splash;

                ActivateWindow(ActivateWindowSources.SplashScreen);
            }
        }

        #endregion

        #region Launch

        bool _HasOnPrelaunchAsync = false;

        protected override sealed void OnLaunched(LaunchActivatedEventArgs e) 
        { 
            InternalLaunchAsync(e); 
        }

        private async void InternalLaunchAsync(ILaunchActivatedEventArgs e)
        {
            OriginalActivatedArgs = e;

            if (e.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                try
                {
                    await InitializeFrameAsync(e);
                }
                catch (Exception)
                {
                    // nothing
                }
            }

            // okay, now handle launch
            bool restored = false;

            switch (e.PreviousExecutionState)
            {
                case ApplicationExecutionState.Suspended:
                case ApplicationExecutionState.Terminated:
                    {
                        OnResuming(this, null, AppExecutionState.Terminated);

                        /*
                            Restore state if you need to/can do.
                            Remember that only the primary tile should restore.
                            (this includes toast with no data payload)
                            The rest are already providing a nav path.

                            In the event that the cache has expired, attempting to restore
                            from state will fail because of missing values. 
                            This is okay & by design.
                        */

                        break;
                    }
                case ApplicationExecutionState.ClosedByUser:
                case ApplicationExecutionState.NotRunning:
                default:
                    break;
            }

            // handle pre-launch
            if ((e as LaunchActivatedEventArgs)?.PrelaunchActivated ?? false)
            {
                var runOnStartAsync = false;
                _HasOnPrelaunchAsync = true;

                await OnPrelaunchAsync(e, out runOnStartAsync);

                if (!runOnStartAsync)
                    return;
            }

            if (!restored)
            {
                var kind = e.PreviousExecutionState == ApplicationExecutionState.Running ? StartKind.Activate : StartKind.Launch;

                await CallOnStartAsync(true, kind);
            }

            ActivateWindow(ActivateWindowSources.Launching);
        }

        public static AdditionalKinds DetermineStartCause(IActivatedEventArgs args)
        {
            if (args is ToastNotificationActivatedEventArgs)
            {
                return AdditionalKinds.Toast;
            }

            var e = args as ILaunchActivatedEventArgs;

            if (e?.TileId == DefaultTileID && string.IsNullOrEmpty(e?.Arguments))
            {
                return AdditionalKinds.Primary;
            }
            else if (e?.TileId == DefaultTileID && !string.IsNullOrEmpty(e?.Arguments))
            {
                return AdditionalKinds.JumpListItem;
            }
            else if (e?.TileId != null && e?.TileId != DefaultTileID)
            {
                return AdditionalKinds.SecondaryTile;
            }
            else
            {
                return AdditionalKinds.Other;
            }
        }

        #endregion

        #region Windows Creation

        public event EventHandler<WindowCreatedEventArgs> WindowCreated;

        protected sealed override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            if (!WindowWrapper.ActiveWrappers.Any())
                Loaded();

            // handle window
            var window = new WindowWrapper(args.Window);

            WindowCreated?.Invoke(this, args);

            base.OnWindowCreated(args);
        }

        private void Loaded()
        {
            // Hook up the default Back handler
            SystemNavigationManager.GetForCurrentView().BackRequested += BackHandler;
        }

        private void BackHandler(object sender, BackRequestedEventArgs args)
        {
            var handled = false;

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                if (NavigationService?.CanGoBack == true)
                {
                    handled = true;
                }
            }
            else
            {
                handled = (NavigationService?.CanGoBack == false);
            }

            RaiseBackRequested(ref handled);

            args.Handled = handled;
        }

        private void RaiseBackRequested(ref bool handled)
        {
            NavigationService.GoBack();
        }
        
        #endregion
        
        #region NavigationServiceFacroty

        public NavigationService NavigationServiceFactory(ExistingContent existingContent)
        {
            return NavigationServiceFactory(existingContent, new Frame());
        }

        protected virtual NavigationService CreateNavigationService(Frame frame)
        {
            return new NavigationService(frame);
        }

        public NavigationService NavigationServiceFactory(ExistingContent existingContent, Frame frame)
        {
            frame.Content = (existingContent == ExistingContent.Include) ? Window.Current.Content : null;

            // if the service already exists for this frame, use the existing one.
            foreach (var nav in WindowWrapper.ActiveWrappers.SelectMany(x => x.NavigationServices))
            {
                if (nav.Frame.Equals(frame))
                    return nav;
            }

            var navigationService = CreateNavigationService(frame);
            WindowWrapper.Current().NavigationServices.Add(navigationService);

            frame.RegisterPropertyChangedCallback(Frame.BackStackDepthProperty, (s, args) => UpdateShellBackButton());
            frame.Navigated += (s, args) => UpdateShellBackButton();

            return navigationService;
        }

        public void UpdateShellBackButton()
        {
            // show the shell back only if there is anywhere to go in the default frame
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = (ShowShellBackButton && NavigationService.CanGoBack) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(propertyName);
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] String propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
        
    }
}
