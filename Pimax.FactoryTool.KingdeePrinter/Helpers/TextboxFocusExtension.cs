using System.Windows;
using System.Windows.Controls;

namespace Pimax.FactoryTool.KingdeePrinter.Helpers
{
    public class TextboxFocusExtension : TextBox
    {
        public static bool GetFocus(TextBox obj)
        {
            return (bool)obj.GetValue(FocusProperty);
        }

        public static void SetFocus(TextBox obj, bool value)
        {
            obj.SetValue(FocusProperty, value);
        }

        public static readonly DependencyProperty FocusProperty =
                DependencyProperty.RegisterAttached("Focus", typeof(bool), typeof(TextboxFocusExtension), new PropertyMetadata(false, IsFocusChanged));
        
        private static void IsFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (d as TextBox).Focus();
                (d as TextBox).SelectAll();
            }
        }
    }
}
