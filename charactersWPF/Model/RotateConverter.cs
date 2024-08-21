using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace charactersWPF.Model
{
    class RotateConverter : IValueConverter
    {
        private readonly double left;
        private readonly double top;

        public RotateConverter(double left, double top)
        {
            this.left = left;
            this.top = top;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new RotateTransform((double)value, left, top);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((RotateTransform)value).Angle;
        }
    }
}