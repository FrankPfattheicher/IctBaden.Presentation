using System.Windows;

namespace IctBaden.Presentation
{
    //<Window.Style>
    //    <Style TargetType = "{x:Type Window}" >
    //        < Style.Triggers >
    //            <DataTrigger Binding="{Binding Done}" Value="true">
    //                <Setter Property = "local:WindowCloseBehaviour.Close" Value="true" />
    //            </DataTrigger>
    //        </Style.Triggers>
    //    </Style>
    //</Window.Style>

    /// <summary>
    /// Use a DataTrigger with a binding to a boolean Binding="{Binding Done}" Value="true".
    /// And a Setter Property = "WindowCloseBehaviour.Close" Value="true"
    /// </summary>
    public static class WindowCloseBehaviour
    {
        public static void SetClose(DependencyObject target, bool value)
        {
            target.SetValue(CloseProperty, value);
        }
        public static readonly DependencyProperty CloseProperty =
        DependencyProperty.RegisterAttached("Close", typeof(bool), typeof(WindowCloseBehaviour), new UIPropertyMetadata(false, OnClose));

        private static void OnClose(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && ((bool)e.NewValue))
            {
                var window = GetWindow(sender);
                window?.Close();
            }
        }
        private static Window GetWindow(DependencyObject sender)
        {
            Window window = null;
            if (sender is Window)
                window = (Window)sender;
            if (window == null)
                window = Window.GetWindow(sender);
            return window;
        }
    }
}