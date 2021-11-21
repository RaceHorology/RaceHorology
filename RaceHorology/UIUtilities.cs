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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace RaceHorology
{
  public class CBItem
  {
    public string Text { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
      return Text;
    }
  }

  public static class CBItemExtensions
  {

    public static bool SelectCBItem(this ComboBox cmb, object value)
    {
      bool itemSelected = false;
      foreach (CBItem item in cmb.Items)
        if (object.Equals(item.Value, value))
        {
          cmb.SelectedValue = item;
          itemSelected = true;
          break;
        }

      if (!itemSelected)
        cmb.SelectedIndex = -1;
      
      return itemSelected;
    }

  }


  public static class UiUtilities
  {

    public static void FillCmbRaceRun(ComboBox cmb, Race race)
    {
      cmb.Items.Clear();

      // Fill Runs
      for (int i = 0; i < race.GetMaxRun(); i++)
      {
        string sz1 = String.Format("{0}. Durchgang", i + 1);
        cmb.Items.Add(new CBItem { Text = sz1, Value = race.GetRun(i) });
      }
      cmb.SelectedIndex = 0;
    }

    public static void FillGrouping(ComboBox comboBox, string selected = null)
    {
      comboBox.Items.Clear();
      comboBox.Items.Add(new CBItem { Text = "---", Value = null });
      comboBox.Items.Add(new CBItem { Text = "Klasse", Value = "Participant.Class" });
      comboBox.Items.Add(new CBItem { Text = "Gruppe", Value = "Participant.Group" });
      comboBox.Items.Add(new CBItem { Text = "Kategorie", Value = "Participant.Sex" });

      if (string.IsNullOrEmpty(selected))
        comboBox.SelectedIndex = 0;
      else
        comboBox.SelectCBItem(selected);
    }




    public static void EnableOrDisableColumns(Race race, DataGrid dg)
    {
      EnableOrDisableColumn(race, dg, "Year");
      EnableOrDisableColumn(race, dg, "Club");
      EnableOrDisableColumn(race, dg, "Nation");
      EnableOrDisableColumn(race, dg, "Code");
      EnableOrDisableColumn(race, dg, "Points");
      EnableOrDisableColumn(race, dg, "Percentage");
    }


    public static void EnableOrDisableColumn(Race race, DataGrid dg, string columnName)
    {
      foreach(var col in dg.ColumnsByName(columnName))
      {
        if (col != null)
        {
          if (race.IsFieldActive(columnName))
            col.Visibility = Visibility.Visible;
          else
            col.Visibility = Visibility.Collapsed;
        }
      }
    }
  }


  public static class DataGridUtil
  {

    public static string GetName(DependencyObject obj)
    {
      return (string)obj.GetValue(NameProperty);
    }

    public static void SetName(DependencyObject obj, string value)
    {
      obj.SetValue(NameProperty, value);
    }


    public static DataGridColumn ColumnByName(this DataGrid dg, string columnName)
    {
      foreach (var col in dg.ColumnsByName(columnName))
        return col;

      return null;
    }

    public static IEnumerable<DataGridColumn> ColumnsByName(this DataGrid dg, string columnName)
    {
      List<DataGridColumn> cols = new List<DataGridColumn>();

      foreach (var col in dg.Columns)
        if (string.Equals(GetName(col), columnName))
          yield return col;

      yield break;
    }

    public static readonly DependencyProperty NameProperty =
        DependencyProperty.RegisterAttached("Name", typeof(string), typeof(DataGridUtil), new UIPropertyMetadata(""));

  }



  public class ScrollToMeasuredItemBehavior
  {
    DataGrid _dataGrid;
    AppDataModel _dataModel;
    Participant _scrollToParticipant;

    System.Timers.Timer _timer;


    public ScrollToMeasuredItemBehavior(DataGrid dataGrid, AppDataModel dataModel)
    {
      _dataGrid = dataGrid;
      _dataModel = dataModel;
      _dataModel.ParticipantMeasuredEvent += OnParticipantMeasured;
      _timer = null;
    }


    void OnParticipantMeasured(object sender, Participant participant)
    {
      _scrollToParticipant = participant;

      _timer = new System.Timers.Timer(200);
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = false;
      _timer.Enabled = true;

    }

    void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (_dataGrid.ItemsSource != null)
        {
          foreach (var x in _dataGrid.ItemsSource)
          {
            Participant xp = null;
            xp = (x as RunResult)?.Participant.Participant;
            if (xp == null)
              xp = (x as RaceResultItem)?.Participant.Participant;

            if (xp == _scrollToParticipant)
            {
              _dataGrid.ScrollIntoView(x);
              break;
            }
          }
        }
      });
    }
  }



  /// <summary>
  /// Die Klasse ermöglicht es, dass ein Event erst mit Verzögerung ausgelöst wird.
  /// Tritt das selbe Event in der angegeben Zeitspanne erneut auf, wird das vorherige Event gestoppt.
  /// </summary>
  public class DelayedEventHandler
  {
    private Timer timer;

    /// <summary>
    /// Ruft das Delay ab oder legt es fest.
    /// </summary>
    public double Delay
    {
      get { return this.timer.Interval; }
      set { this.timer.Interval = value; }
    }

    /// <summary>
    /// EventHandler, der an Events von Steuerelementen gebunden werden kann.
    /// </summary>
    public TextChangedEventHandler Delayed { get; private set; }

    private TextChangedEventHandler handler;
    private object forwardSender;
    private TextChangedEventArgs forwardArgs;

    /// <summary>
    /// Erzeugt einen DelayedEventHandler mit der angegebenen Zeitspanne.
    /// </summary>
    /// <param name="delay">Das Delay, mit dem...</param>
    /// <param name="handler">...der gewünscht Handler aufgerufen wird.</param>
    public DelayedEventHandler(TimeSpan delay, TextChangedEventHandler handler)
      : this((int)delay.TotalMilliseconds, handler)
    {

    }

    /// <summary>
    /// Erzeugt einen DelayedEventHandler mit der angegebenen Zeitspanne in Millisekunden.
    /// </summary>
    /// <param name="delayInMilliseconds">Das Delay in Millisekunden, mit dem...</param>
    /// <param name="handler">...der gewünscht Handler aufgerufen wird.</param>
    public DelayedEventHandler(int delayInMilliseconds, TextChangedEventHandler handler)
    {
      timer = new Timer
      {
        Enabled = false,
        Interval = delayInMilliseconds
      };
      timer.Elapsed += new ElapsedEventHandler((s, e) =>
      {
        timer.Stop();

        if (handler != null)
        {
          handler(this.forwardSender, this.forwardArgs);
        }
      });

      this.handler = handler;

      Delayed = new TextChangedEventHandler((sender, e) =>
      {
        this.forwardSender = sender;
        this.forwardArgs = e;

        timer.Stop();
        timer.Start();
      });
    }
  }
}
