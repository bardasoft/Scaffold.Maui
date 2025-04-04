﻿using ScaffoldLib.Maui.Containers;
using ScaffoldLib.Maui.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaffoldLib.Maui.Internal;

internal static class Extensions
{
    public static void TryAppearing(this IView iview, bool isComplete = false)
    {
        if (iview is IAppear v)
            v.OnAppear(isComplete);

        if (iview is View view && view.BindingContext is IAppear data && data != iview)
            data.OnAppear(isComplete);
    }

    public static void TryDisappearing(this IView iview, bool isComplete = false)
    {
        if (iview is IDisappear v)
            v.OnDisappear(isComplete);

        if (iview is View view && view.BindingContext is IDisappear data && data != iview)
            data.OnDisappear(isComplete);
    }

    public static void TryRemoveFromNavigation(this IView iview)
    {
        if (iview is INavigationMember rm)
            rm.OnDisconnectedFromNavigation();
    }

    public static void TryAppearing(this IAgent agent, bool isComplete, AppearingStates parentStl, Color? navigationBarBgColor = null)
    {
        if (parentStl == AppearingStates.Disappear)
            return;

        if (isComplete == false)
        {
            agent.IsAppear = true;

            var statusBarStyle = Scaffold.GetStatusBarForegroundColor(agent.ViewWrapper.View);
            if (statusBarStyle != StatusBarColorTypes.DependsByNavigationBarColor)
            {
                Scaffold.SetupStatusBarColor(statusBarStyle);
            }
            else if (navigationBarBgColor != null)
            {
                if (navigationBarBgColor.IsDark())
                    Scaffold.SetupStatusBarColor(StatusBarColorTypes.Light);
                else
                    Scaffold.SetupStatusBarColor(StatusBarColorTypes.Dark);
            }
        }

        if (agent is IAppear ap)
            ap.OnAppear(isComplete);

        agent.ViewWrapper.View.TryAppearing(isComplete);
    }

    public static void TryDisappearing(this IAgent agent, bool isComplete, AppearingStates parentStl)
    {
        if (parentStl == AppearingStates.Disappear)
            return;

        if (isComplete == false)
            agent.IsAppear = false;

        if (agent is IDisappear dis)
            dis.OnDisappear(isComplete);

        agent.ViewWrapper.View.TryDisappearing(isComplete);
    }

    public static void TryNotifyNavigationConnect(this IAgent agent)
    {
        if (agent.ViewWrapper.View is INavigationMember member)
            member.OnConnectedToNavigation();

        agent.OnConnectedToNavigation();
    }

    public static void TryNotifyNavigationDisconnect(this IAgent agent)
    {
        if (agent.ViewWrapper.View is INavigationMember member)
            member.OnDisconnectedFromNavigation();

        agent.OnDisconnectedFromNavigation();
    }

    public static void ResolveStatusBarColor(this IAgent frame)
    {
        if (!frame.IsAppear)
            return;

        var type = Scaffold.GetStatusBarForegroundColor(frame.ViewWrapper.View);
        if (type != StatusBarColorTypes.DependsByNavigationBarColor)
        {
            Scaffold.SetupStatusBarColor(type);
        }
        else
        {
            var rbar = (frame.NavigationBar as View)?.BackgroundColor;
            var vbar = Scaffold.GetNavigationBarBackgroundColor(frame.ViewWrapper.View);
            var bgColor = vbar ?? rbar ?? Scaffold.DefaultNavigationBarBackgroundColor;
            if (bgColor.IsDark())
                Scaffold.SetupStatusBarColor(StatusBarColorTypes.Light);
            else
                Scaffold.SetupStatusBarColor(StatusBarColorTypes.Dark);
        }
    }

    public static T? ItemOrDefault<T>(this IList<T> self, int index)
    {
        if (self.Count == 0)
            return default;

        if (index < 0 || index > self.Count - 1)
            return default;

        return self[index];
    }

    public static byte[] ToBytes(this Stream str)
    {
        byte[] byteArray;
        str.Position = 0;
        using (MemoryStream ms = new MemoryStream())
        {
            str.CopyTo(ms);
            byteArray = ms.ToArray();
        }
        str.Position = 0;
        return byteArray;
    }

    public static Task AwaitReady(this IAgent agent, CancellationToken? cancellationToken = null)
    {
        var view = (View)agent.ViewWrapper.View;
        return AwaitReady(view, cancellationToken);
    }

    public static Task AwaitReady(this View view, CancellationToken? cancellation = null)
    {
        var cancel = cancellation ?? CancellationToken.None;
#if ANDROID
        return AwaitReadyDroid(view, cancel);
#elif IOS
        return AwaitReadyIOS(view, cancel);
#else
        return Task.CompletedTask;
#endif
    }

#if IOS
    private static async Task AwaitReadyIOS(View view, CancellationToken cancellation)
    {
        var handler = await view.AwaitHandler(cancellation);
        if (handler == null)
            return;

        try
        {
            if (view is IHardView hardv)
                await hardv.ReadyToPush(cancellation);
        }
        catch (Exception)
        {
        }
    }
#endif

#if ANDROID
    private static async Task AwaitReadyDroid(View view, CancellationToken cancel)
    {
        var h = await AwaitHandler(view, cancel);
        if (h == null)
            return;
        try
        {
            if (view is IHardView hardv)
                await hardv.ReadyToPush(cancel);
        }
        catch (Exception)
        {
        }

        return;
    }
#endif

    public static async Task<IViewHandler?> AwaitHandler(this View view, CancellationToken? cancel = null)
    {
        if (view.Handler != null)
            return view.Handler;

        var tsc = new TaskCompletionSource<IViewHandler>();
        void eventDelegate(object? sender, EventArgs e)
        {
            tsc.TrySetResult(view.Handler!);
        }

        view.HandlerChanged += eventDelegate;
        var handler = await tsc.Task.WithCancelation(cancel ?? CancellationToken.None);
        view.HandlerChanged -= eventDelegate;

        return handler;
    }

    public static bool IsDark(this Color col)
    {
        double Y = 0.299 * col.Red + 0.587 * col.Green + 0.114 * col.Blue;
        if (Y > 0.5d)
            return false;
        else
            return true;
    }

    public static Scaffold? GetRootScaffold(this Microsoft.Maui.Controls.Page page)
    {
        if (page is ContentPage c)
        {
            //if (c.Content is Scaffold cv)
            //    return cv;
            //else
            return FindScaffold(c.Content) as Scaffold;
        }
        return null;
    }

    private static View? FindScaffold(View view, int immersion = 0)
    {
        if (immersion > 100)
            throw new InvalidOperationException("A lot deep immersion for trying find scaffold");

        immersion++;

        switch (view)
        {
            case Scaffold vc:
                return vc;

            case ContentView cv:
                return FindScaffold(cv.Content, immersion);

            case Layout l:
                foreach (var item in l.Children)
                    return FindScaffold((View)item, immersion);
                break;

            default:
                return null;
        }

        return null;
    }

    internal static INotifyCollectionChanged AsNotifyObs<T>(this ReadOnlyObservableCollection<T> obs)
    {
        return obs;
    }

    internal static Rect AbsRect(this View view)
    {
        var rect = view.Frame;
        var parent = view.Parent as View;
        while (parent != null)
        {
            rect = rect.Offset(parent.X, parent.Y);
            parent = parent.Parent as View;
        }
        return rect;
    }

    internal static IEnumerable<View> GetDeepAllChildren(this View view)
    {
        var list = new List<View>();
        switch (view)
        {
            case Layout l:
                foreach (View item in l.Children)
                {
                    list.Add(item);
                    list.AddRange(item.GetDeepAllChildren());
                }
                break;
            case ContentView c:
                list.Add(c.Content);
                list.AddRange(c.Content.GetDeepAllChildren());
                break;
            default:
                break;
        }
        return list;
    }

    internal static async Task<T?> WithCancelation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        try
        {
            return await task.WaitAsync(cancellationToken);
        }
        catch (Exception)
        {
            return default;
        }
    }

    internal static async Task WithCancelation(this Task task, CancellationToken cancellationToken)
    {
        try
        {
            await task.WaitAsync(cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    internal static Size WithPadding(this Size size, Thickness padding)
    {
        return new Size(size.Width + padding.HorizontalThickness, size.Height + padding.VerticalThickness);
    }

    internal static void InvalidateMeasureHardcore(this View view)
    {
        ((IView)view).InvalidateMeasure();
    }

    internal static T? GetTargetOrDefault<T>(this WeakReference<T> self)
        where T : class
    {
        if (self == null)
            throw new ArgumentNullException(nameof(self));

        if (self.TryGetTarget(out var target))
            return target;

        return default;
    }

    internal static Task RunAny(Task? task1, Task? task2, Task? task3)
    {
        var tasks = new List<Task>();

        if (task1 != null)
            tasks.Add(task1);

        if (task2 != null)
            tasks.Add(task2);

        if (task3 != null)
            tasks.Add(task3);

        return Task.WhenAny(tasks);
    }

    internal static Task ContinueWithInUIThread(this Task task, Action code)
    {
        return task.ContinueWith(x =>
        {
            MainThread.BeginInvokeOnMainThread(code);
        });
    }
}