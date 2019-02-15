using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Ben.HomeAssistant.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visible = System.Convert.ToBoolean(value);
            var invert = System.Convert.ToBoolean(parameter);

            if (invert) visible = !visible;

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
