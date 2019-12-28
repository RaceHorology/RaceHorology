using RaceHorologyLib;
using System;
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
      foreach (CBItem item in cmb.Items)
        if (object.Equals(item.Value, value))
        {
          cmb.SelectedValue = item;
          return true;
        }

      return false;
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
      foreach (var col in dg.Columns)
        if (string.Equals(GetName(col), columnName))
          return col;

      return null;
    }

    public static readonly DependencyProperty NameProperty =
        DependencyProperty.RegisterAttached("Name", typeof(string), typeof(DataGridUtil), new UIPropertyMetadata(""));

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
