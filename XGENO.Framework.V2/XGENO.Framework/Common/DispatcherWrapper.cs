using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace XGENO.Framework.Common
{
    public class DispatcherWrapper
    {
        private CoreDispatcher dispatcher;

        internal DispatcherWrapper(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public static DispatcherWrapper Current() 
        {
            return WindowWrapper.Current().Dispatcher;
        }
        
        public bool HasThreadAccess() => dispatcher.HasThreadAccess;
                
        public async Task DispatchAsync(Action action, int delayms = 0, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            if (dispatcher.HasThreadAccess && priority == CoreDispatcherPriority.Normal) 
            { 
                action(); 
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();

                await dispatcher.RunAsync(priority, () =>
                {
                    try 
                    { 
                        action(); 
                        tcs.TrySetResult(null); 
                    }
                    catch (Exception ex) 
                    { 
                        tcs.TrySetException(ex); 
                    }
                });

                await tcs.Task;
            }
        }

        public async Task DispatchAsync(Func<Task> func, int delayms = 0, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            if (dispatcher.HasThreadAccess && priority == CoreDispatcherPriority.Normal) 
            { 
                await func?.Invoke(); 
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();

                await dispatcher.RunAsync(priority, async () =>
                {
                    try 
                    { 
                        await func(); tcs.TrySetResult(null); 
                    }
                    catch (Exception ex) 
                    { 
                        tcs.TrySetException(ex); 
                    }
                });

                await tcs.Task;
            }
        }

        public async Task<T> DispatchAsync<T>(Func<T> func, int delayms = 0, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            if (dispatcher.HasThreadAccess && priority == CoreDispatcherPriority.Normal) 
            {
                return func(); 
            }
            else
            {
                var tcs = new TaskCompletionSource<T>();

                await dispatcher.RunAsync(priority, () =>
                {
                    try 
                    { 
                        tcs.TrySetResult(func()); 
                    }
                    catch (Exception ex) 
                    { 
                        tcs.TrySetException(ex); 
                    }
                });

                await tcs.Task;

                return tcs.Task.Result;
            }
        }

        public async void Dispatch(Action action, int delayms = 0, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            if (dispatcher.HasThreadAccess && priority == CoreDispatcherPriority.Normal) 
            { 
                action(); 
            }
            else
            {
                dispatcher.RunAsync(priority, () => action()).AsTask().Wait();
            }
        }

        public T Dispatch<T>(Func<T> action, int delayms = 0, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) where T : class
        {
            if(delayms > 0)
                Task.Delay(delayms).Wait();

            if (dispatcher.HasThreadAccess && priority == CoreDispatcherPriority.Normal) 
            { 
                return action(); 
            }
            else
            {
                T result = null;

                dispatcher.RunAsync(priority, () => result = action()).AsTask().Wait();

                return result;
            }
        }

        public async Task DispatchIdleAsync(Action action, int delayms = 0)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            var tcs = new TaskCompletionSource<object>();

            await dispatcher.RunIdleAsync(delegate
            {
                try 
                { 
                    action(); tcs.TrySetResult(null); 
                }
                catch (Exception ex) 
                { 
                    tcs.TrySetException(ex); 
                }
            });

            await tcs.Task;
        }

        public async Task DispatchIdleAsync(Func<Task> func, int delayms = 0)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            var tcs = new TaskCompletionSource<object>();

            await dispatcher.RunIdleAsync(async delegate
            {
                try 
                { 
                    await func(); 
                    tcs.TrySetResult(null); 
                }
                catch (Exception ex) 
                { 
                    tcs.TrySetException(ex); 
                }
            });

            await tcs.Task;
        }

        public async Task<T> DispatchIdleAsync<T>(Func<T> func, int delayms = 0)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            var tcs = new TaskCompletionSource<T>();

            await dispatcher.RunIdleAsync(delegate
            {
                try 
                { 
                    tcs.TrySetResult(func()); 
                }
                catch (Exception ex) 
                { 
                    tcs.TrySetException(ex); 
                }
            });

            await tcs.Task;

            return tcs.Task.Result;
        }

        public async void DispatchIdle(Action action, int delayms = 0)
        {
            if(delayms > 0)
                await Task.Delay(delayms);

            dispatcher.RunIdleAsync(delegate { action(); }).AsTask().Wait();
        }

        public T DispatchIdle<T>(Func<T> action, int delayms = 0) where T : class
        {
            if(delayms > 0)
                Task.Delay(delayms).Wait();

            T result = null;

            dispatcher.RunIdleAsync(delegate { result = action(); }).AsTask().Wait();

            return result;
        }
    }
}
