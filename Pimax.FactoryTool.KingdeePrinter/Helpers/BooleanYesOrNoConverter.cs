using System;
using System.Globalization;
using System.Windows.Data;

namespace Pimax.FactoryTool.KingdeePrinter.Helpers
{
    public class BooleanYesOrNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "是" : "否";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
