/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
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

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace RaceHorologyLib
{
  /// <summary>
  /// Converts a position number into a string. Position 0 is transferred into "./."
  /// </summary>
  public class PositionConverter : IValueConverter
  {
    private bool _inParantheses;
    public PositionConverter()
    {
      _inParantheses = false;
    }


    public PositionConverter(bool inParantheses)
    {
      _inParantheses = inParantheses;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      uint position = (uint)value;

      if (_inParantheses)
      {
        if (position == 0)
          return "(-)";

        return string.Format("({0})", position);
      }
      else
      {
        if (position == 0)
          return "---";

        return position.ToString();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  /// <summary>
  /// Converts a position number into a string. Position 0 is transferred into "./."
  /// </summary>
  public class PointsConverter : IValueConverter
  {
    public PointsConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double points = (double)value;

      if (points < 0 || points >= 9999.99)
        return "---";

      return string.Format("{0:0.00}", points);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        double d = double.Parse((string)value);
        return d;
      }
      catch(Exception )
      {
        return -1.0;
      }
    }
  }


  /// <summary>
  /// Converts a position number into a string. Position 0 is transferred into "./."
  /// </summary>
  public class PercentageConverter : IValueConverter
  {
    private bool _bPrintZero;
    public PercentageConverter()
    {
      _bPrintZero = false;
    }


    public PercentageConverter(bool bPrintZero)
    {
      _bPrintZero = bPrintZero;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double percentage = (double)value;

      if (!_bPrintZero && percentage == 0)
        return "";

      return string.Format("{0:0.0}", percentage);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class ResultCodeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        RunResult.EResultCode rc = (RunResult.EResultCode)value;

        return rc.Format();
      }
      catch (Exception)
      {
        return "";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class ResultCodeConverterWithNormal : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        RunResult.EResultCode rc = (RunResult.EResultCode)value;

        switch (rc)
        {
          case RunResult.EResultCode.Normal:
            return "keine Ausscheidung";
          case RunResult.EResultCode.NotSet:
            return "keine Zeit oder Ausscheidung";

          default:
            return rc.Format();
        }
      }
      catch (Exception)
      {
        return "";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class TimeSpanConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        string str = string.Empty;
        string strParameter = parameter?.ToString();

        if (value is TimeSpan ts)
          str = ((TimeSpan?)ts).ToRaceTimeString( formatString: strParameter);

        return str;
      }
      catch (Exception)
      {
        return "";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }



  public class ResultCodeWithCommentConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2)
        throw new Exception("invalid number of argumnets");

      try
      {
        RunResult.EResultCode rc = (RunResult.EResultCode)values[0];
        string comment = (string)values[1];

        if (rc == RunResult.EResultCode.Normal || rc == RunResult.EResultCode.NotSet)
          return "";

        if (string.IsNullOrEmpty(comment))
          return rc.Format();
        else
          return string.Format("{0} ({1})", rc.Format(), comment);
      }
      catch(Exception)
      {
        return "";
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class ResultTimeAndCodeConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2)
        throw new Exception("invalid number of argumnets");
      
      if (values.Any(x => x == DependencyProperty.UnsetValue))
        return DependencyProperty.UnsetValue;

      try
      {
        TimeSpan? t = (TimeSpan?)values[0];
        RunResult.EResultCode rc = (RunResult.EResultCode)values[1];

        // Return time
        if (rc == RunResult.EResultCode.Normal)
          return t.ToRaceTimeString();
        // Return result code
        return rc.Format();
      }
      catch (Exception)
      {
        return "";
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Converts a property name to a translated user names
  /// </summary>
  public class PropertyNameConverter : IValueConverter
  {
    public PropertyNameConverter()
    {
    }


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch((string)value)
      {
        case "Name":
          return "Name";
        case "Firstname":
          return "Vorname";
        case "Sex":
          return "Kategorie";
        case "Year":
          return "Jahrgang";
        case "Club":
          return "Verein";
        case "Nation":
          return "Nation / Verband";
        case "Code":
          return "Code";
        case "SvId":
          return "Skiverbands-Id";
        case "Class":
          return "Klasse";
        case "Group":
          return "Gruppe";

        default:
          return value;
      }

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  /// <summary>
  /// Converts an age input to a year
  /// </summary>
  public class AgeToYearInputConverter : IValueConverter
  {
    public AgeToYearInputConverter()
    {
    }


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        int input = int.Parse(value.ToString());
        if (input < 100) // assume this is an age not year
        {
          int currentYear = DateTime.Now.AddMonths(3).Year; // Season runs from 1.10.XX ... 31.9.XX
          int birthYear = currentYear - input;
          return birthYear;
        }
      }
      catch (Exception)
      { }

      return value;
    }
  }


  public static class EResultCodeUtil
  {
    public static string Format(this RunResult.EResultCode code)
    {
      return StrResultCode(code);
    }

    public static string StrResultCode(RunResult.EResultCode code)
    {
      switch (code)
      {
        case RunResult.EResultCode.NotSet: return "---";
        case RunResult.EResultCode.Normal: return "";
        case RunResult.EResultCode.NaS: return "NAS";
        case RunResult.EResultCode.NiZ: return "NIZ";
        case RunResult.EResultCode.DIS: return "DIS";
        case RunResult.EResultCode.NQ: return "NQ";
      }
      return "???";
    }
  }
}
