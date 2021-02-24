using System;
using System.Globalization;
using System.Windows.Data;

namespace TestDetection
{
    public class TicksToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string time = "0:0:0";
            if (value == null)
                return time;
            long titck = (long)Math.Round(double.Parse(value.ToString()));
            TimeSpan timeSpan = TimeSpan.FromSeconds(titck);
            time = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            return time;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}