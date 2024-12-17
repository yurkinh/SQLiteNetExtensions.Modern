using System.Globalization;

namespace IntegratedTestsSampleApp.Converters;

public class TextToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (text == "Press to run test")
            {
                return Colors.Gray;
            }
            else if (text == "All tests passed successfully!")
            {
                return Colors.SeaGreen; 
            }
            else
            {
                return Colors.Red; 
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
