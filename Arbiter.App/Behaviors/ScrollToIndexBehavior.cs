using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Arbiter.App.Behaviors;

public class ScrollToIndexBehavior : AvaloniaObject
{
    #region ScrollToIndex Property

    public static readonly AttachedProperty<int?> ScrollToIndexProperty =
        AvaloniaProperty.RegisterAttached<ScrollToEndBehavior, Interactive, int?>("ScrollToIndex", null, false,
            BindingMode.TwoWay);

    public static void SetScrollToIndex(Interactive element, int? index) => element.SetValue(ScrollToIndexProperty, index);
    public static int? GetScrollToIndex(Interactive element) => element.GetValue(ScrollToIndexProperty);

    #endregion

    static ScrollToIndexBehavior()
    {
        ScrollToIndexProperty.Changed.AddClassHandler<Interactive>(HandleScrollToIndexChanged);
    }

    private static void HandleScrollToIndexChanged(Interactive element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is null)
        {
            return;
        }

        TryScrollToIndex(element, force: true);
        SetScrollToIndex(element, null);
    }

    private static void TryScrollToIndex(Interactive element, bool force = false)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => TryScrollToIndex(element, force), DispatcherPriority.Background);
            return;
        }
        
        var itemsControl = element as ItemsControl ?? element.FindDescendantOfType<ItemsControl>();
        if (itemsControl is null)
        {
            return;
        }

        var index = GetScrollToIndex(element) ?? 0;
        Dispatcher.UIThread.Post(() => itemsControl.ScrollIntoView(index), DispatcherPriority.Background);
    }
}