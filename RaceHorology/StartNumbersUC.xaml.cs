/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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
  /// Interaction logic for StartNumbersUC.xaml
  /// </summary>
  public partial class StartNumbersUC : UserControl
  {
    private AppDataModel _dm;
    private Race _race;

    private StartNumberAssignment _snaWorkspace;

    public StartNumbersUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;

      _snaWorkspace = new StartNumberAssignment();

      dgStartList.ItemsSource = _snaWorkspace.ParticipantList;
      dgParticipants.ItemsSource = _race.GetParticipants();
      //RaceUC.EnableOrDisableColumns(_race, dgStartList);

      IsVisibleChanged += StartNumbersUC_IsVisibleChanged;
    }

    private void StartNumbersUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.OldValue && (bool)e.NewValue)
      {
        _snaWorkspace.LoadFromRace(_race);
        UpdateNextStartNumber();
      }
    }

    private void UpdateNextStartNumber()
    {
      txtNextStartNumber.Text = _snaWorkspace.GetNextFreeStartNumber().ToString();
      txtNextStartNumberManual.Text = _snaWorkspace.GetNextFreeStartNumber().ToString();
    }

    private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
    {
      var result = MessageBox.Show("Alle Startnummern löschen?\n\nDies kann nicht rückgängig gemacht werden.", "Startnummern löschen?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
      if (result == MessageBoxResult.Yes)
      {
        var particpants = _race.GetParticipants();
        foreach (var p in particpants)
          p.StartNumber = 0;

        UpdateNextStartNumber();
      }
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
      _snaWorkspace.LoadFromRace(_race);

      UpdateNextStartNumber();
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnInsert_Click(object sender, RoutedEventArgs e)
    {
      if (dgStartList.SelectedItem is AssignedStartNumber selItem)
      {
        _snaWorkspace.InsertAndShift(selItem.StartNumber);

        UpdateNextStartNumber();
      }
    }

    private void btnRemove_Click(object sender, RoutedEventArgs e)
    {
      if (dgStartList.SelectedItem is AssignedStartNumber selItem)
      {
        _snaWorkspace.Delete(selItem.StartNumber);

        UpdateNextStartNumber();
      }
    }

    private void btnAssign_Click(object sender, RoutedEventArgs e)
    {

      try
      {
        uint sn = uint.Parse(txtNextStartNumberManual.Text);

        if (dgParticipants.SelectedItem is RaceParticipant selItem)
        {
          _snaWorkspace.Assign(sn, selItem);

          UpdateNextStartNumber();
        }
      }
      catch (Exception)
      { }
    }
  }
}
