using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Arbiter.App.Behaviors;

public class ScrollToEndBehavior : AvaloniaObject
{
    #region AutoScrollToEnd Property

    public static readonly AttachedProperty<bool> AutoScrollToEndProperty =
        AvaloniaProperty.RegisterAttached<ScrollToEndBehavior, Interactive, bool>("AutoScrollToEnd");

    public static void SetAutoScrollToEnd(Interactive element, bool value) =>
        element.SetValue(AutoScrollToEndProperty, value);

    public static bool GetAutoScrollToEnd(Interactive element) => element.GetValue(AutoScrollToEndProperty);

    #endregion

    #region ScrollToEnd Property

    public static readonly AttachedProperty<bool> ScrollToEndProperty =
        AvaloniaProperty.RegisterAttached<ScrollToEndBehavior, Interactive, bool>("ScrollToEnd", false, false,
            BindingMode.TwoWay);

    public static void SetScrollToEnd(Interactive element, bool value) => element.SetValue(ScrollToEndProperty, value);
    public static bool GetScrollToEnd(Interactive element) => element.GetValue(ScrollToEndProperty);

    #endregion

    static ScrollToEndBehavior()
    {
        AutoScrollToEndProperty.Changed.AddClassHandler<Interactive>(HandleAutoScrollToEndChanged);
        ScrollToEndProperty.Changed.AddClassHandler<Interactive>(HandleScrollToEndChanged);
    }

    private static void HandleAutoScrollToEndChanged(Interactive element, AvaloniaPropertyChangedEventArgs e)
    {
        var itemsControl = element as ItemsControl ?? element.FindDescendantOfType<ItemsControl>();
        if (itemsControl is null)
        {
            return;
        }

        if (e.NewValue is true)
        {
            itemsControl.Items.CollectionChanged += OnCollectionChanged;
        }
        else if (e.NewValue is false)
        {
            itemsControl.Items.CollectionChanged -= OnCollectionChanged;
        }

        return;
        
        void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs _)
        {
            if (!GetAutoScrollToEnd(element))
            {
                return;
            }

            TryScrollToEnd(element);
        }
    }

    private static void HandleScrollToEndChanged(Interactive element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true)
        {
            return;
        }

        TryScrollToEnd(element, force: true);
        SetScrollToEnd(element, false);
    }

    private static void TryScrollToEnd(Interactive element, bool force = false)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => TryScrollToEnd(element, force), DispatcherPriority.Background);
            return;
        }
        
        var scrollViewer = element as ScrollViewer ?? element.FindDescendantOfType<ScrollViewer>();
        if (scrollViewer is null)
        {
            return;
        }

        var maxY = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
        var currentY = scrollViewer.Offset.Y;

        if (force || currentY >= maxY - 1)
        {
            Dispatcher.UIThread.Post(() => scrollViewer.ScrollToEnd(), DispatcherPriority.Background);
        }
    }
}