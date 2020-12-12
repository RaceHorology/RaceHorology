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
  /// Interaction logic for DSVImportUC.xaml
  /// </summary>
  public partial class DSVImportUC : UserControl
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    AppDataModel _dm;

    DSVInterfaceModel _dsvData;
    CollectionViewSource _viewDSVList;

    public DSVImportUC()
    {
      InitializeComponent();
    }


    public void Init(AppDataModel dm)
    {
      _dm = dm;

      initDSVAddToList();
    }



    void initDSVAddToList()
    {
      _dsvData = new DSVInterfaceModel(_dm);

      updateDSVGrid();

      txtDSVSearch.TextChanged += new DelayedEventHandler(
          TimeSpan.FromMilliseconds(300),
          txtDSVSearch_TextChanged
      ).Delayed;
    }

    void updateDSVGrid()
    {
      if (_dsvData.Data != null)
      {
        _viewDSVList = new CollectionViewSource();
        _viewDSVList.Source = _dsvData.Data.Tables[0].DefaultView;
        dgDSVList.ItemsSource = _viewDSVList.View;

        lblVersion.Content = string.Format("Version: {0} ({1})", _dsvData?.UsedDSVList, _dsvData.Date?.ToString("d"));
      }
    }


    private void txtDSVSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (_dsvData?.Data != null)
        {
          DataTable table = _dsvData.Data.Tables[0];

          string sFilterText = txtDSVSearch.Text;
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
          _viewDSVList.View.Refresh();
        }
      });
    }


    private void btnDSVAdd_Click(object sender, RoutedEventArgs e)
    {
      foreach (var item in dgDSVList.SelectedItems)
      {
        if (item is DataRowView rowView)
        {
          DataRow row = rowView.Row;
          foreach (var r in _dm.GetRaces())
          {
            RaceImport imp = new RaceImport(r, _dsvData.Mapping, new ClassAssignment(_dm.GetParticipantClasses()));

            RaceParticipant rp = imp.ImportRow(row);
          }
        }
      }
    }



    private void btnDSVImportOnline_Click(object sender, RoutedEventArgs e)
    {
      _dsvData.UpdateDSVList(new DSVImportReaderOnline());
      updateDSVGrid();
    }


    private void btnDSVImportFile_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
      {
        string path = openFileDialog.FileName;
        IDSVImportReaderFile dsvImportReader;
        if (System.IO.Path.GetExtension(path).ToLowerInvariant() == ".zip")
          dsvImportReader = new DSVImportReaderZip(path);
        else
          dsvImportReader = new DSVImportReaderFile(path);

        _dsvData.UpdateDSVList(dsvImportReader);
        updateDSVGrid();
      }
    }


    private void btnDSVUpdatePoints_Click(object sender, RoutedEventArgs e)
    {
      DSVUpdatePoints.UpdatePoints(_dm, _dsvData.Data, _dsvData.Mapping, _dsvData.UsedDSVList);
    }

  }
}
