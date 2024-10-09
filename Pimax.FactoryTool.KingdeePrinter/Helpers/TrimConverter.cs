using System;
using System.Globalization;
using System.Windows.Data;

namespace Pimax.FactoryTool.KingdeePrinter.Helpers
{
    public class TrimConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string)?.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string)?.Trim();
        }
    }
}
