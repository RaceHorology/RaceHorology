/*
 *  Copyright (C) 2019 - 2024 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
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


  public class OnlineStatusToBrushConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return Brushes.Transparent;

      switch ((StatusType)value)
      {
        case StatusType.Online: return Brushes.LightGreen;
        case StatusType.Error_GotOffline: return Brushes.OrangeRed;
        case StatusType.NoDevice: return Brushes.OrangeRed;
      }
      return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool b && b)
        return Visibility.Visible;
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class ComparisonToVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        string inputParameter = parameter?.ToString() ?? "";

        IEnumerable<string> paramList = inputParameter.Contains("|") ? inputParameter.Split(new[] { "|" }, StringSplitOptions.None) : new[] { inputParameter };

        return paramList.Any(param => string.Equals(value?.ToString(), param)) ? Visibility.Visible : Visibility.Collapsed;
      }
      catch
      {
        return Visibility.Visible;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value?.ToString() != Visibility.Collapsed.ToString();
    }
  }

  public class RTTIToImageConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length > 2)
        throw new Exception("invalid number of argumnets");

      try
      {
        string imageName = "status_unsync.svg";

        int rssi = (int)values[0];
        string syncStatus = values.Length > 1 ? (string)values[1] : "";

        // Beschreibung nach Christian Hund:
        //>= -105 dBm ist zwei Funkwellen / grün
        //- 114 bis - 106dBm ist eine Funkwelle / grün
        //<= -115dBm ist keine Funkwellen/ gelb
        //- 999 bedeutet kein Signal mehr erhalten / ausser Funkreichweite
        //- 1000 bedeutet noch kein Signal erhalten
        if (syncStatus == "Synchronised" || syncStatus == "")
        {
          if (rssi >= -105)
            imageName = "Status_RSSI-Funksehrgut.svg";
          else if (rssi >= -114)
            imageName = "Status_RSSI-Funkgut.svg";
          else if (rssi > -999)
            imageName = "Status_RSSI-keinFunk.svg";
          else if (rssi == -999)
            imageName = "Status_RSSI-not-found.svg";
          else if (rssi == -1000)
            imageName = "Status_RSSI-not-found.svg";
        }
        return new Uri("pack://application:,,,/Icons/alpenhunde/" + imageName);
      }
      catch (Exception)
      {
        return new Uri("pack://application:,,,/Icons/alpenhunde/status_unsync.svg");
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


}
