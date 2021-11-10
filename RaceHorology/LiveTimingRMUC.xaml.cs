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
  /// Interaction logic for LiveTimingRMUC.xaml
  /// </summary>
  public partial class LiveTimingRMUC : UserControl
  {

    public LiveTimingRMUC()
    {
      InitializeComponent();
    }


    public LiveTimingRM _liveTimingRM;
    Race _thisRace;

    public void InitializeLiveTiming(Race race)
    {
      _thisRace = race;
      ResetLiveTimningUI(_thisRace.RaceConfiguration);
    }

    private void ResetLiveTimningUI(RaceConfiguration cfg)
    {
      if (cfg.LivetimingParams == null)
        return;

      try
      {
        txtLTBewerb.Text = cfg.LivetimingParams["RM_Bewerb"];
        txtLTLogin.Text = cfg.LivetimingParams["RM_Login"];
        txtLTPassword.Password = cfg.LivetimingParams["RM_Password"];
      }
      catch (KeyNotFoundException) { }
    }


    private void SelectLiveTimingEvent(string eventName)
    {
      cmbLTEvent.SelectedItem = eventName;
    }


    private void StoreLiveTiming(ref RaceConfiguration cfg)
    {
      cfg.LivetimingParams = new Dictionary<string, string>();
      cfg.LivetimingParams["RM_Bewerb"] = txtLTBewerb.Text;
      cfg.LivetimingParams["RM_Login"] = txtLTLogin.Text;
      cfg.LivetimingParams["RM_Password"] = txtLTPassword.Password;
      cfg.LivetimingParams["RM_EventName"] = cmbLTEvent.SelectedItem?.ToString();
    }


    private void BtnLTLogin_Click(object sender, RoutedEventArgs e)
    {
      RaceConfiguration cfg = _thisRace.RaceConfiguration;
      StoreLiveTiming(ref cfg);
      _thisRace.RaceConfiguration = cfg;

      try
      {
        _liveTimingRM = new LiveTimingRM();
        _liveTimingRM.Race = _thisRace;

        _liveTimingRM.Login(txtLTBewerb.Text, txtLTLogin.Text, txtLTPassword.Password);

        var events = _liveTimingRM.GetEvents();
        cmbLTEvent.ItemsSource = events;

        try
        {
          SelectLiveTimingEvent(cfg.LivetimingParams["RM_EventName"]);
        }
        catch (KeyNotFoundException)
        {
          cmbLTEvent.SelectedIndex = 0;
        }

      }
      catch (Exception error)
      {
        MessageBox.Show(error.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        _liveTimingRM = null;
      }

      UpdateLiveTimingUI();
    }


    private void CmbLTEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbLTEvent.SelectedIndex >= 0)
      {
        _liveTimingRM.SetEvent(cmbLTEvent.SelectedIndex);
      }
    }


    private void BtnLTStart_Click(object sender, RoutedEventArgs e)
    {
      if (_liveTimingRM == null)
        return;


      if (_liveTimingRM.Started)
      {
        _liveTimingRM.Stop();
      }
      else
      {
        // Start
        if (cmbLTEvent.SelectedIndex < 0)
        {
          MessageBox.Show("Bitte Veranstalltung auswählen", "Live Timing", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
          RaceConfiguration cfg = _thisRace.RaceConfiguration;
          StoreLiveTiming(ref cfg);
          _thisRace.RaceConfiguration = cfg;

          _liveTimingRM.SetEvent(cmbLTEvent.SelectedIndex);
          _liveTimingRM.Start();
        }
      }

      UpdateLiveTimingUI();
    }

    private void UpdateLiveTimingUI()
    {
      if (_liveTimingRM != null && _liveTimingRM.LoggedOn)
      {
        btnLTLogin.IsEnabled = false;
        btnLTStart.IsEnabled = true;
        cmbLTEvent.IsEnabled = true;
      }
      else
      {
        btnLTLogin.IsEnabled = true;
        btnLTStart.IsEnabled = false;
        cmbLTEvent.IsEnabled = false;
      }

      if (_liveTimingRM != null && _liveTimingRM.Started)
        btnLTStart.Content = "Stop";
      else
        btnLTStart.Content = "Start";
    }

  }
}
