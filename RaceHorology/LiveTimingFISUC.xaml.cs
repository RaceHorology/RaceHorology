﻿/*
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

using RaceHorologyLib;
using LiveTimingFIS;
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
  /// Interaction logic for LiveTimingFISUC.xaml
  /// </summary>
  public partial class LiveTimingFISUC : UserControl
  {

    public LiveTimingFIS.LiveTimingFIS _liveTimingFIS;
    Race _thisRace;

    public LiveTimingFISUC()
    {
      InitializeComponent();

      Loaded += (s, e) => { // only at this point the control is ready
        Window.GetWindow(this) // get the parent window
              .Closing += (s1, e1) => Dispose(); //disposing logic here
      };
    }

    private void Dispose()
    {
      if (_liveTimingFIS!= null)
        _liveTimingFIS.Dispose();
    }

    public void InitializeLiveTiming(Race race)
    {
      _thisRace = race;
      ResetLiveTimningUI(_thisRace.RaceConfiguration);
    }


    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
      RaceConfiguration cfg = _thisRace.RaceConfiguration;
      StoreLiveTiming(ref cfg);
      _thisRace.RaceConfiguration = cfg;

      _liveTimingFIS = new LiveTimingFIS.LiveTimingFIS();

      try
      {
        _liveTimingFIS.Race = _thisRace;
        _liveTimingFIS.Connect(int.Parse(cfg.LivetimingParams["FIS_Port"]));
        _liveTimingFIS.Login(cfg.LivetimingParams["FIS_RaceCode"], cfg.LivetimingParams["FIS_Category"], cfg.LivetimingParams["FIS_Pasword"]);
        _liveTimingFIS.Start();
      }
      catch (Exception error)
      {
        MessageBox.Show(error.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        _liveTimingFIS = null;
      }
    }


    private void ResetLiveTimningUI(RaceConfiguration cfg)
    {
      if (cfg.LivetimingParams == null)
        return;

      try
      {
        txtRaceCode.Text = cfg.LivetimingParams["FIS_RaceCode"];
        txtCategory.Text = cfg.LivetimingParams["FIS_Category"];
        txtPassword.Password = cfg.LivetimingParams["FIS_Pasword"];
        txtPort.Text = cfg.LivetimingParams["FIS_Port"];
      }
      catch (KeyNotFoundException) { }
    }


    private void StoreLiveTiming(ref RaceConfiguration cfg)
    {
      cfg.LivetimingParams = new Dictionary<string, string>();
      cfg.LivetimingParams["FIS_RaceCode"] = txtRaceCode.Text;
      cfg.LivetimingParams["FIS_Category"] = txtCategory.Text;
      cfg.LivetimingParams["FIS_Pasword"] = txtPassword.Password;
      cfg.LivetimingParams["FIS_Port"] = txtPort.Text;
    }


  }
}