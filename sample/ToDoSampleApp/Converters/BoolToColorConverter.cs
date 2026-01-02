using System.Globalization;

namespace ToDoSampleApp.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isDone)
        {
            return isDone ? Color.FromArgb("#059669") : Color.FromArgb("#10B981");
        }
        return Color.FromArgb("#10B981");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}