﻿/*
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
using System.Configuration;
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
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for Settings.xaml
  /// </summary>
  public partial class SettingsDlg : Window
  {
    COMPortViewModel _comPorts;

    public SettingsDlg()
    {
      InitializeComponent();

      _comPorts = new COMPortViewModel();
      cbTimingDevicePort.ItemsSource = _comPorts.Items;
      cbTimingDevicePort.SelectedValuePath = "Port";
      cbTimingDevicePort.SelectedValue = Properties.Settings.Default.TimingDevice_Port;

      txtTimingDeviceUrl.Text = Properties.Settings.Default.TimingDevice_Url;

      chkTimingDeviceDebugDump.IsChecked = Properties.Settings.Default.TimingDevice_Debug_Dump;
      chkTimingDisplayPartcipantAssignment.IsChecked = Properties.Settings.Default.Timing_DisplayPartcipantAssignment;

      //cbTimingDevice.Items.Add("---");
      cbTimingDevice.Items.Add("ALGE TdC8000/8001, ALGE Timy via RS232");
      cbTimingDevice.Items.Add("ALGE Timy (via USB)");
      cbTimingDevice.Items.Add("Alpenhunde");
      cbTimingDevice.Items.Add("Microgate Racetime 2");
      cbTimingDevice.Items.Add("Microgate Rei 2");
      cbTimingDevice.Items.Add("Microgate Rei Pro");
      cbTimingDevice.Items.Add("Microgate RT Pro");
      cbTimingDevice.SelectedValue = Properties.Settings.Default.TimingDevice_Type;
      cbTimingDevice_SelectionChanged(null, null);

      txtAutomaticNiZTimeout.Text = Properties.Settings.Default.AutomaticNiZTimeout.ToString();
      txtAutomaticNaSStarters.Text = Properties.Settings.Default.AutomaticNaSStarters.ToString();
      txtStartTimeIntervall.Text = Properties.Settings.Default.StartTimeIntervall.ToString();
      chkAutoAddParticipant.IsChecked = Properties.Settings.Default.AutoAddParticipants;

      txtNotToBeAssigned.Text = Properties.Settings.Default.StartNumbersNotToBeAssigned;

      cbUpdateChannel.Items.Add("Stabil");
      cbUpdateChannel.Items.Add("Test");
      cbUpdateChannel.SelectedValue = Properties.Settings.Default.UpdateChannel;

          }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      if (cbTimingDevice.SelectedValue != null && Properties.Settings.Default.TimingDevice_Type != (string)cbTimingDevice.SelectedValue)
        Properties.Settings.Default.TimingDevice_Type = (string)cbTimingDevice.SelectedValue;

      if (cbTimingDevicePort.SelectedValue != null && Properties.Settings.Default.TimingDevice_Port != (string)cbTimingDevicePort.SelectedValue)
        Properties.Settings.Default.TimingDevice_Port = (string)cbTimingDevicePort.SelectedValue;

      if (Properties.Settings.Default.TimingDevice_Url != txtTimingDeviceUrl.Text)
        Properties.Settings.Default.TimingDevice_Url = txtTimingDeviceUrl.Text;

      if (Properties.Settings.Default.TimingDevice_Debug_Dump != chkTimingDeviceDebugDump.IsChecked == true )
        Properties.Settings.Default.TimingDevice_Debug_Dump = chkTimingDeviceDebugDump.IsChecked == true;

      if (Properties.Settings.Default.Timing_DisplayPartcipantAssignment != chkTimingDisplayPartcipantAssignment.IsChecked == true)
        Properties.Settings.Default.Timing_DisplayPartcipantAssignment = chkTimingDisplayPartcipantAssignment.IsChecked == true;

      try
      {
        if (Properties.Settings.Default.AutomaticNiZTimeout != uint.Parse(txtAutomaticNiZTimeout.Text))
          Properties.Settings.Default.AutomaticNiZTimeout = uint.Parse(txtAutomaticNiZTimeout.Text);
      }
      catch (Exception) { }

      try
      { 
        if (Properties.Settings.Default.AutomaticNaSStarters != uint.Parse(txtAutomaticNaSStarters.Text))
          Properties.Settings.Default.AutomaticNaSStarters = uint.Parse(txtAutomaticNaSStarters.Text); 
      }
      catch (Exception) { }

      if (cbUpdateChannel.SelectedValue != null && Properties.Settings.Default.UpdateChannel != (string)cbUpdateChannel.SelectedValue)
        Properties.Settings.Default.UpdateChannel = (string)cbUpdateChannel.SelectedValue;

      try 
      { 
        if (Properties.Settings.Default.StartTimeIntervall != uint.Parse(txtStartTimeIntervall.Text))
          Properties.Settings.Default.StartTimeIntervall = uint.Parse(txtStartTimeIntervall.Text); 
      }
      catch (Exception) { }

      Properties.Settings.Default.AutoAddParticipants = chkAutoAddParticipant.IsChecked == true;

      Properties.Settings.Default.StartNumbersNotToBeAssigned = txtNotToBeAssigned.Text;

      Properties.Settings.Default.Save();

      DialogResult = true;
    }

    private void cbTimingDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Enable ParticipantAssignment for Alpenhunde
      if (cbTimingDevice.SelectedValue != null && cbTimingDevice.SelectedValue.ToString().Contains("Alpenhunde") 
        && !(Properties.Settings.Default.TimingDevice_Type != null && Properties.Settings.Default.TimingDevice_Type.Contains("Alpenhunde")))
        chkTimingDisplayPartcipantAssignment.IsChecked = true;

      bool displayUrl = cbTimingDevice.SelectedValue != null && cbTimingDevice.SelectedValue.ToString().Contains("Alpenhunde");
      bool displayComPort= cbTimingDevice.SelectedValue != null && (cbTimingDevice.SelectedValue.ToString().Contains("ALGE TdC") ||
                cbTimingDevice.SelectedValue.ToString().Contains("Microgate"));

      cbTimingDevicePort.Visibility = displayComPort ? Visibility.Visible : Visibility.Collapsed;
      lblTimingDevicePort.Visibility = displayComPort ? Visibility.Visible : Visibility.Collapsed;

      txtTimingDeviceUrl.Visibility = displayUrl ? Visibility.Visible : Visibility.Collapsed;
      lblTimingDeviceUrl.Visibility = displayUrl ? Visibility.Visible : Visibility.Collapsed;

      lblTimingDeviceDebug.Visibility = displayComPort ? Visibility.Visible : Visibility.Collapsed;
      chkTimingDeviceDebugDump.Visibility = displayComPort ? Visibility.Visible : Visibility.Collapsed;
    }

  }
}
