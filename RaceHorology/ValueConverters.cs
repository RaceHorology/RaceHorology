using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RaceHorology
{

  public class BooleanToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return Brushes.Transparent;

      Brush[] brushes = parameter as Brush[];
      if (brushes == null)
        return Brushes.Transparent;

      bool isTrue;
      bool.TryParse(value.ToString(), out isTrue);

      if (isTrue)
      {
        var brush = (SolidColorBrush)brushes[0];
        return brush ?? Brushes.Transparent;
      }
      else
      {
        var brush = (SolidColorBrush)brushes[1];
        return brush ?? Brushes.Transparent;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


}
