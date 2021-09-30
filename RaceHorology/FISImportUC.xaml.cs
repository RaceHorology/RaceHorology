/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
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

using Microsoft.Win32;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for FISImportUC.xaml
  /// </summary>
  public partial class FISImportUC : UserControl
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    AppDataModel _dm;

    FISInterfaceModel _importData;
    CollectionViewSource _viewList;

    public FISImportUC()
    {
      InitializeComponent();
    }


    public void Init(AppDataModel dm, FISInterfaceModel fisData)
    {
      _dm = dm;
      _importData = fisData;

      this.KeyDown += new KeyEventHandler(KeyDownHandler);

      initAddToList();
    }



    void initAddToList()
    {
      updateGrid();

      txtSearch.TextChanged += new DelayedEventHandler(
          TimeSpan.FromMilliseconds(300),
          txtSearch_TextChanged
      ).Delayed;
    }

    void updateGrid()
    {
      if (_importData.Data != null)
      {
        _viewList = new CollectionViewSource();
        _viewList.Source = _importData.Data.Tables[0].DefaultView;
        dgList.ItemsSource = _viewList.View;

        lblVersion.Content = string.Format("Version: {0} ({1})", _importData?.UsedList, _importData.Date?.ToString("d"));

        txtSearch_TextChanged(null, null); // Update search
        dgList_SelectionChanged(null, null); // Update button status
      }
      else
      {
        lblVersion.Content = string.Format("Version: --- (keine FIS Liste importiert)");
      }
    }


    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (_importData?.Data != null)
        {
          DataTable table = _importData.Data.Tables[0];

          string sFilterText = txtSearch.Text;
          if (string.IsNullOrEmpty(sFilterText))
            table.DefaultView.RowFilter = string.Empty;
          else
          {
            StringBuilder sb = new StringBuilder();
            foreach (DataColumn column in table.Columns)
            {
              sb.AppendFormat("CONVERT({0}, System.String) Like '%{1}%' OR ", column.ColumnName, sFilterText);
            }

            sb.Remove(sb.Length - 3, 3); // Remove "OR "
            table.DefaultView.RowFilter = sb.ToString();
          }
          _viewList.View.Refresh();
        }
      });
    }


    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
      addSelectedItemsToDataModel();
    }


    private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      addSelectedItemsToDataModel();
    }


    private void addSelectedItemsToDataModel()
    {
      foreach (var item in dgList.SelectedItems)
      {
        if (item is DataRowView rowView)
        {
          DataRow row = rowView.Row;
          if (_dm.GetRaces().Count > 0)
          {
            foreach (var r in _dm.GetRaces())
            {
              RaceImport imp = new RaceImport(r, _importData.Mapping, new ClassAssignment(_dm.GetParticipantClasses()));
              RaceParticipant rp = imp.ImportRow(row);
            }
          }
          else
          {
            ParticipantImport partImp = new ParticipantImport(_dm.GetParticipants(), _importData.Mapping, _dm.GetParticipantCategories(), new ClassAssignment(_dm.GetParticipantClasses()));
            partImp.ImportRow(row);
          }
        }
      }
    }


    private void btnImportFile_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
      {
        string path = openFileDialog.FileName;

        FISImportReader importReader = new FISImportReader(path);

        try
        {
          _importData.UpdateFISList(importReader);
        }
        catch (Exception exc)
        {
          MessageBox.Show("Die FIS Liste konnte nicht importiert werden.\n\nFehlermeldung: " + exc.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        updateGrid();
      }
    }


    private void btnUpdatePoints_Click(object sender, RoutedEventArgs e)
    {
      var impRes = DSVUpdatePoints.UpdatePoints(_dm, _importData.Data, _importData.Mapping, _importData.UsedList);
      showUpdatePointsResult(impRes, _importData.UsedList);
    }


    private void showUpdatePointsResult(List<ImportResults> impRes, string usedLists)
    {
      string messageTextDetails = "";

      messageTextDetails += string.Format("Benutzte FIS Liste: {0}\n\n", usedLists);

      int nRace = 0;
      foreach (var i in impRes)
      {
        Race race = _dm.GetRace(nRace);

        string notFoundParticipants = string.Join("\n", i.Errors);

        messageTextDetails += string.Format(
          "Zusammenfassung für das Rennen \"{0}\":\n" +
          "- Punkte erfolgreich aktualisiert: {1}\n",
          race.ToString(), i.SuccessCount);

        if (i.ErrorCount > 0)
        {
          messageTextDetails += string.Format("\n" +
            "- Teilnehmer nicht gefunden: {0}\n" +
            "{1}",
            i.ErrorCount, notFoundParticipants);
        }

        messageTextDetails += "\n";
      }

      MessageBox.Show("Der Importvorgang wurde abgeschlossen: \n\n" + messageTextDetails, "Importvorgang", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void dgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      btnAdd.IsEnabled = dgList.SelectedItems.Count > 0;
    }


    private void KeyDownHandler(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.D && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        txtSearch.Focus();
        txtSearch.SelectAll();
      }
    }

  }
}
