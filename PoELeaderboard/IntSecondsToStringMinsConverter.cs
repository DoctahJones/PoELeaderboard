using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PoELeaderboard
{
    class IntSecondsToStringMinsConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var intSeconds = (int)value;
            TimeSpan time = TimeSpan.FromSeconds(intSeconds);
            var outputString = "";
            if (time.Hours > 0)
            {
                outputString = time.ToString(@"h\:mm\:ss");
            }
            else if (time.Minutes > 0)
            {
                outputString = time.ToString(@"m\:ss");
            }
            else
            {
                outputString = time.ToString(@"ss");
            }
            return outputString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
