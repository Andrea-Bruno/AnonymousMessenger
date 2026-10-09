using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace XamarinShared
{
    public static class MainThread
    {
        public static void BeginInvokeOnMainThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                action();
                return;
            }
            if (dispatcher.IsDispatchRequired)
                dispatcher.Dispatch(action);
            else
                action();
        }

        public static Task BeginInvokeOnMainThread(Func<Task> func)
        {
            var tcs = new TaskCompletionSource<object>();
            // Cast the async lambda to Action so it binds to the Action overload.
            // Without the cast the async lambda is a Func<Task> and overload
            // resolution would call this same method, recursing infinitely.
            BeginInvokeOnMainThread((Action)(async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }

        public static void InvokeOnMainThread(Action action) => BeginInvokeOnMainThread(action);

        public static Task InvokeOnMainThreadAsync(Func<Task> func) => BeginInvokeOnMainThread(func);
    }
}
