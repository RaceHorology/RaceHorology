/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceHorology
{
  public static class ExportUI
  {
    public static string ExportDsv(Race race)
    {
      string filePath = System.IO.Path.Combine(
        race.GetDataModel().GetDB().GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(race.GetDataModel().GetDB().GetDBFileName()) + ".zip");

      Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();
      openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
      openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
      openFileDialog.DefaultExt = ".zip";
      openFileDialog.Filter = "DSV Results (.zip)|*.zip";
      try
      {
        if (openFileDialog.ShowDialog() == true)
        {
          filePath = openFileDialog.FileName;
          DSVExport dsvExport = new DSVExport();
          dsvExport.Export(filePath, race);

          return filePath;
        }
      }
      catch (DSVExportException ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\nFehlermeldung: " + ex.GetHumanReadableError(),
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }
      return null;
    }


    public static string ExportDsvAlpin(Race race)
    {
      return ExportHelper<Race>.ExportToTextFile(
        race, race,
        "DSVAlpin - Tab Separated Text File (.txt)|*.txt|" +
        "DSVAlpin - Tab Separated Text File - UTF-8 (.txt)|*.txt",
        ".txt",
        (locRace, filePath, utf8) =>
        {
          DSVAlpinExport exp = new DSVAlpinExport(locRace);
          TsvExport tsvExp = new TsvExport();
          tsvExp.Export(filePath, exp.ExportToDataSet(), utf8);
        }
      );
    }


    public static string ExportCSV(Race race)
    {
      return ExportHelper<Race>.ExportToTextFile(
        race, race,
        "Comma Separated Text File (.csv)|*.csv|" +
        "Comma Separated Text File - UTF-8 (.csv)|*.csv",
        ".csv",
        (locRace, filePath, utf8) =>
        {
          RaceExport exp = new RaceExport(locRace);
          CsvExport csvExp = new CsvExport();
          csvExp.Export(filePath, exp.ExportToDataSet(), utf8);
        }
      );
    }

    public static string ExportXLSX(Race race)
    {
      return ExportHelper<Race>.ExportToTextFile(
        race, race,
        "Microsoft Excel (.xlsx)|*.xslx", ".xlsx",
        (locRace, filePath, utf8) =>
        {
          RaceExport exp = new RaceExport(locRace);
          ExcelExport csvExp = new ExcelExport();
          csvExp.Export(filePath, exp.ExportToDataSet());
        }
      );
    }

    public static string ExportAlpenhundeStartList(Race race, ICollectionView view)
    {
      return ExportHelper<ICollectionView>.ExportToTextFile(
        race, view,
          "Alpenhunde - UTF-8 CSV (.csv)|*.csv", ".csv",
          (obj, filePath, utf8) =>
          {
            var exp = new AlpenhundeStartlistExport(obj);
            var tsvExp = new CsvExport();
            tsvExp.Export(filePath, exp.ExportToDataSet(), utf8, ";");
          }
        );
    }

    public static string ExportGenericStartListCSV(Race race, ICollectionView view)
    {
      return ExportHelper<ICollectionView>.ExportToTextFile(
        race, view,
        "Comma Separated Text File (.csv)|*.csv|" +
        "Comma Separated Text File - UTF-8 (.csv)|*.csv",
        ".csv",
        (obj, filePath, utf8) =>
        {
          var exp = new GenericStartlistExport(obj);
          var tsvExp = new CsvExport();
          tsvExp.Export(filePath, exp.ExportToDataSet(), utf8);
        }
      );
    }
    public static string ExportGenericStartListXLSX(Race race, ICollectionView view)
    {
      return ExportHelper<ICollectionView>.ExportToTextFile(
        race, view,
          "Microsoft Excel (.xlsx)|*.xslx", ".xlsx",
          (obj, filePath, utf8) =>
          {
            var exp = new GenericStartlistExport(obj);
            var tsvExp = new ExcelExport();
            tsvExp.Export(filePath, exp.ExportToDataSet());
          }
        );
    }




  }


  public static class ExportHelper<Type>
  {
    public delegate void exportDelegate(Type exportObject, string filepath, bool utf8);
    public static string ExportToTextFile(Race race, Type exportObject, string fileDialogFilter, string suffix, exportDelegate expDelegate)
    {
      string filePath = System.IO.Path.Combine(
        race.GetDataModel().GetDB().GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(race.GetDataModel().GetDB().GetDBFileName()) + suffix);

      Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();
      openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
      openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
      openFileDialog.DefaultExt = suffix;
      openFileDialog.Filter = fileDialogFilter;
      try
      {
        if (openFileDialog.ShowDialog() == true)
        {
          filePath = openFileDialog.FileName;

          string appliedFilter;
          string[] filterstring = openFileDialog.Filter.Split('|');
          appliedFilter = filterstring[(openFileDialog.FilterIndex - 1) * 2];
          bool utf8 = appliedFilter.Contains("UTF-8");

          expDelegate(exportObject, filePath, utf8);
          return filePath;
        }
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }

      return null;
    }
  }

}
