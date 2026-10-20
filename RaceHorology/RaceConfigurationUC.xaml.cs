using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Sub UI to edit the parameters of a view (e.g. number of participants to reverse)
  /// </summary>
  interface IViewParamsUC
  {
    void SetParams(Dictionary<string, object> viewParams);
    Dictionary<string, object> GetParams();
  }

  /// <summary>
  /// ComboBox item for a view, optionally with a sub UI for its parameters
  /// </summary>
  internal class ViewCBItem : CBItem
  {
    public Func<UserControl> ParamsUC;

    UserControl _paramsUCInstance;

    /// <summary>
    /// Returns the sub UI, created once (initialized with default parameters) and then reused,
    /// so entered values survive switching to another view and back.
    /// </summary>
    public UserControl GetParamsUC()
    {
      if (_paramsUCInstance == null && ParamsUC != null)
      {
        _paramsUCInstance = ParamsUC();
        (_paramsUCInstance as IViewParamsUC)?.SetParams(null);
      }
      return _paramsUCInstance;
    }
  }


  /// <summary>
  /// Interaction logic for RaceConfigurationUC.xaml
  /// </summary>
  public partial class RaceConfigurationUC : UserControl
  {

    RaceConfiguration _raceConfiguration;
    Race.ERaceType? _raceType;
    RaceConfigurationPresets _raceConfigurationPresets;


    public RaceConfigurationUC()
    {
      InitializeComponent();
    }


    public void Init(RaceConfiguration raceConfig, Race.ERaceType? raceType)
    {
      // ApplicationFolder + raceconfigpresets
      _raceConfigurationPresets = new RaceConfigurationPresets(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"raceconfigpresets"));
      _raceConfiguration = raceConfig.Copy();
      _raceType = raceType;

      refreshConfigPresetsUI();

      // Configuration Screen
      cmbRuns.Items.Clear();
      cmbRuns.Items.Add(new CBItem { Text = "1", Value = 1 });
      cmbRuns.Items.Add(new CBItem { Text = "2", Value = 2 });
      cmbRuns.Items.Add(new CBItem { Text = "3", Value = 3 });
      cmbRuns.Items.Add(new CBItem { Text = "4", Value = 4 });
      cmbRuns.Items.Add(new CBItem { Text = "5", Value = 5 });
      cmbRuns.Items.Add(new CBItem { Text = "6", Value = 6 });

      // Penalty options
      cmbConfigPenalty.Items.Clear();
      cmbConfigPenalty.Items.Add(new CBItem { Text = "Bestzeit + Sekunden", Value = RaceConfiguration.EPenaltyMode.BestPlusSeconds });
      cmbConfigPenalty.Items.Add(new CBItem { Text = "Bestzeit + Prozent", Value = RaceConfiguration.EPenaltyMode.BestPlusPercentage });

      // Result
      UiUtilities.FillGrouping(cmbConfigErgebnisGrouping, null, raceConfig.ActiveFields);

      cmbConfigErgebnis.Items.Clear();
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Bester Durchgang", Value = "RaceResult_BestOfTwo" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe der besten 2 Durchgänge", Value = "RaceResult_SumBest2" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "2x Best of 2", Value = "RaceResult_2xBestOf2" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe", Value = "RaceResult_Sum" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach IWO/DWO (DSV)", Value = "RaceResult_SumDSVPointsSchool" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach IWO/DWO (FIS)", Value = "RaceResult_SumFISPoints" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach Tabelle gesamt", Value = "RaceResult_SumPointsViaTable" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach Tabelle pro Lauf", Value = "RaceResult_SumPointsViaTablePerRun" });

      // Run 1
      UiUtilities.FillGrouping(cmbConfigStartlist1Grouping, null, raceConfig.ActiveFields);
      cmbConfigStartlist1.Items.Clear();
      cmbConfigStartlist1.Items.Add(new ViewCBItem { Text = "Startnummer (aufsteigend)", Value = StartlistViewConfig.Run1_StartnumberAscending });
      cmbConfigStartlist1.Items.Add(new ViewCBItem { Text = "Startnummer (absteigend)", Value = StartlistViewConfig.Run1_StartnumberDescending });
      cmbConfigStartlist1.Items.Add(new ViewCBItem { Text = "Punkte", Value = StartlistViewConfig.Run1_Points, ParamsUC = () => new StartlistPointsParamsUC() });

      // Run 2
      UiUtilities.FillGrouping(cmbConfigStartlist2Grouping, null, raceConfig.ActiveFields);
      cmbConfigStartlist2.Items.Clear();
      cmbConfigStartlist2.Items.Add(new ViewCBItem { Text = "Startnummer (aufsteigend)", Value = StartlistViewConfig.Run2_StartnumberAscending });
      cmbConfigStartlist2.Items.Add(new ViewCBItem { Text = "Startnummer (absteigend)", Value = StartlistViewConfig.Run2_StartnumberDescending });
      cmbConfigStartlist2.Items.Add(new ViewCBItem { Text = "Vorheriger Lauf nach Zeit", Value = StartlistViewConfig.Run2_PreviousRun, ParamsUC = () => new StartlistPreviousRunParamsUC() });
      cmbConfigStartlist2.Items.Add(new ViewCBItem { Text = "Summe aller vorherigen Läufe", Value = StartlistViewConfig.Run2_Cumulative, ParamsUC = () => new StartlistPreviousRunParamsUC() });

      chkTeam.Checked += ChkTeam_CheckChanged;
      chkTeam.Unchecked += ChkTeam_CheckChanged;

      ResetConfigurationSelectionUI(_raceConfiguration);
    }

    private void ChkTeam_CheckChanged(object sender, RoutedEventArgs e)
    {
      ucTeam.IsEnabled = chkTeam.IsChecked == true;
    }

    private void CmbTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTemplate.SelectedValue is CBItem selected)
      {
        if (selected.Value is string configName)
        {
          RaceConfiguration config = _raceConfigurationPresets.GetConfiguration(configName);
          RaceConfiguration configToSet = RaceConfigurationMerger.MainConfig(_raceConfiguration, config, _raceType);

          ResetConfigurationSelectionUI(configToSet);
        }
      }
    }


    private void btnTemplateDelete_Click(object sender, RoutedEventArgs e)
    {
      if (cmbTemplate.SelectedValue is CBItem selected)
      {
        if (selected.Value is string configName)
        {
          _raceConfigurationPresets.DeleteConfiguration(configName);
          refreshConfigPresetsUI();
        }
      }
    }


    private void btnTemplateSave_Click(object sender, RoutedEventArgs e)
    {
      RaceConfiguration newConfig = new RaceConfiguration();
      StoreConfigurationSelectionUI(ref newConfig);

      // Ask for the name to store
      string configName = string.Empty;
      if (cmbTemplate.SelectedValue is CBItem selected && selected.Value is string selConfigName)
        configName = selConfigName;

      RaceConfigurationSaveDlg dlg = new RaceConfigurationSaveDlg(configName);
      dlg.ShowDialog();
      if (dlg.TemplateName == null)
        return;

      configName = dlg.TemplateName;

      if (_raceConfigurationPresets.GetConfigurations().ContainsKey(configName))
      {
        var res = MessageBox.Show(string.Format("Die Konfiguration \"{0}\" existiert schon. Wollen Sie die Konfiguration überschreiben?", configName), "Konfiguration speichern", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res == MessageBoxResult.No)
          return;
      }

      _raceConfigurationPresets.SaveConfiguration(configName, newConfig);
      refreshConfigPresetsUI();
    }


    void refreshConfigPresetsUI()
    {
      // Add items and look if the used config name is in the list and if so, use this as selected one
      string usedConfig = null;
      cmbTemplate.Items.Clear();
      foreach (var config in _raceConfigurationPresets.GetConfigurations())
      {
        cmbTemplate.Items.Add(new CBItem { Text = config.Value?.Name, Value = config.Key });
        if (_raceConfiguration?.Name == config.Value?.Name
          && RaceConfigurationCompare.MainConfig(_raceConfiguration, config.Value))
          usedConfig = config.Key;
      }

      cmbTemplate.SelectCBItem(usedConfig);
    }


    private void ResetConfigurationSelectionUI(RaceConfiguration cfg)
    {
      cmbRuns.SelectCBItem(cfg.Runs);
      cmbConfigErgebnisGrouping.SelectCBItem(cfg.DefaultGrouping);
      cmbConfigErgebnis.SelectCBItem(cfg.RaceResultView);

      if (cfg.RaceResultView_PenaltyRuleMode != RaceConfiguration.EPenaltyMode.Off)
      {
        chkConfigPenalty.IsChecked = true;
        txtValuePenaltyCutOff.Text = string.Format("{0}", cfg.RaceResultView_PenaltyRuleCutOffValue);
        cmbConfigPenalty.SelectCBItem(cfg.RaceResultView_PenaltyRuleMode);
      }
      else
        chkConfigPenalty.IsChecked = false;
      chkConfigPenalty_CheckChanged(null, null);

      selectView(cmbConfigStartlist1, ccConfigStartlist1Params, cfg.Run1_StartistView, cfg.Run1_StartistViewParams);
      cmbConfigStartlist1Grouping.SelectCBItem(cfg.Run1_StartistViewGrouping);
      selectView(cmbConfigStartlist2, ccConfigStartlist2Params, cfg.Run2_StartistView, cfg.Run2_StartistViewParams);
      cmbConfigStartlist2Grouping.SelectCBItem(cfg.Run2_StartistViewGrouping);
      txtValueF.Text = cfg.ValueF.ToString();
      txtValueA.Text = cfg.ValueA.ToString();
      txtValueZ.Text = cfg.ValueZ.ToString();
      txtMinPenalty.Text = cfg.MinimumPenalty.ToString();
      txtValueCutOff.Text = cfg.ValueCutOff.ToString();

      chkConfigFieldsYear.IsChecked = cfg.ActiveFields.Contains("Year");
      chkConfigFieldsClub.IsChecked = cfg.ActiveFields.Contains("Club");
      chkConfigFieldsNation.IsChecked = cfg.ActiveFields.Contains("Nation");
      chkConfigFieldsCode.IsChecked = cfg.ActiveFields.Contains("Code");
      chkConfigFieldsPoints.IsChecked = cfg.ActiveFields.Contains("Points");
      chkConfigFieldsPercentage.IsChecked = cfg.ActiveFields.Contains("Percentage");

      chkTeam.IsChecked = cfg.TeamRaceResultConfig != null;
      ucTeam.SetConfig(cfg.TeamRaceResultConfig);

    }

    private bool StoreConfigurationSelectionUI(ref RaceConfiguration cfg)
    {
      // Store the template name
      string configName = null;
      if (cmbTemplate.SelectedValue is CBItem selected && selected.Value is string selConfigName)
        configName = selConfigName;
      cfg.Name = configName;

      var presetConfig = _raceConfigurationPresets.GetConfiguration(configName);
      if (presetConfig != null)
        cfg.InternalDSVAlpinCompetitionTypeWrite = presetConfig.InternalDSVAlpinCompetitionTypeWrite;

      if (cmbRuns.SelectedIndex >= 0)
        cfg.Runs = (int)((CBItem)cmbRuns.SelectedValue).Value;

      if (cmbConfigErgebnisGrouping.SelectedIndex >= 0)
        cfg.DefaultGrouping = (string)((CBItem)cmbConfigErgebnisGrouping.SelectedValue).Value;

      if (cmbConfigErgebnis.SelectedIndex >= 0)
        cfg.RaceResultView = (string)((CBItem)cmbConfigErgebnis.SelectedValue).Value;

      if (chkConfigPenalty.IsChecked == true)
      {
        try { cfg.RaceResultView_PenaltyRuleCutOffValue = double.Parse(txtValuePenaltyCutOff.Text); } catch (Exception) { }
        cfg.RaceResultView_PenaltyRuleMode = (RaceConfiguration.EPenaltyMode)((CBItem)cmbConfigPenalty.SelectedValue).Value;
      }
      else
        cfg.RaceResultView_PenaltyRuleMode = RaceConfiguration.EPenaltyMode.Off;

      if (cmbConfigStartlist1.SelectedIndex >= 0)
      {
        cfg.Run1_StartistView = (string)((CBItem)cmbConfigStartlist1.SelectedValue).Value;
        cfg.Run1_StartistViewParams = (ccConfigStartlist1Params.Content as IViewParamsUC)?.GetParams();
      }

      if (cmbConfigStartlist1Grouping.SelectedIndex >= 0)
        cfg.Run1_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist1Grouping.SelectedValue).Value;

      if (cmbConfigStartlist2.SelectedIndex >= 0)
      {
        cfg.Run2_StartistView = (string)((CBItem)cmbConfigStartlist2.SelectedValue).Value;
        cfg.Run2_StartistViewParams = (ccConfigStartlist2Params.Content as IViewParamsUC)?.GetParams();
      }

      if (cmbConfigStartlist2Grouping.SelectedIndex >= 0)
        cfg.Run2_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist2Grouping.SelectedValue).Value;

      try { cfg.ValueF = double.Parse(txtValueF.Text); } catch (Exception) { }
      try { cfg.ValueA = double.Parse(txtValueA.Text); } catch (Exception) { }
      try { cfg.ValueZ = double.Parse(txtValueZ.Text); } catch (Exception) { }
      try { cfg.MinimumPenalty = double.Parse(txtMinPenalty.Text); } catch (Exception) { }
      try { cfg.ValueCutOff = double.Parse(txtValueCutOff.Text); } catch (Exception) { }

      cfg.TeamRaceResultConfig = chkTeam.IsChecked == true ? ucTeam.GetConfig() : null;

      void enableField(List<string> fieldList, string field, bool? enabled)
      {
        if (enabled != null && (bool)enabled)
        {
          if (!fieldList.Contains(field))
            fieldList.Add(field);
        }
        else
        {
          if (fieldList.Contains(field))
            fieldList.Remove(field);
        }
      }

      enableField(cfg.ActiveFields, "Year", chkConfigFieldsYear.IsChecked);
      enableField(cfg.ActiveFields, "Club", chkConfigFieldsClub.IsChecked);
      enableField(cfg.ActiveFields, "Nation", chkConfigFieldsNation.IsChecked);
      enableField(cfg.ActiveFields, "Code", chkConfigFieldsCode.IsChecked);
      enableField(cfg.ActiveFields, "Points", chkConfigFieldsPoints.IsChecked);
      enableField(cfg.ActiveFields, "Percentage", chkConfigFieldsPercentage.IsChecked);

      return true;
    }


    public bool ExistingChanges()
    {
      RaceConfiguration cfgTemp = new RaceConfiguration();
      StoreConfigurationSelectionUI(ref cfgTemp);

      return !RaceConfigurationCompare.MainConfig(_raceConfiguration, cfgTemp);
    }


    public RaceConfiguration GetConfig()
    {
      RaceConfiguration cfg = new RaceConfiguration();
      StoreConfigurationSelectionUI(ref cfg);

      return cfg;
    }

    public void ResetChanges()
    {
      ResetConfigurationSelectionUI(_raceConfiguration);
      refreshConfigPresetsUI();
    }

    private void chkConfigPenalty_CheckChanged(object sender, RoutedEventArgs e)
    {
      txtValuePenaltyCutOff.IsEnabled = chkConfigPenalty.IsChecked == true;
      cmbConfigPenalty.IsEnabled = chkConfigPenalty.IsChecked == true;
    }


    private ContentControl getParamsContentControl(ComboBox cmb)
    {
      if (cmb == cmbConfigStartlist1)
        return ccConfigStartlist1Params;
      if (cmb == cmbConfigStartlist2)
        return ccConfigStartlist2Params;
      return null;
    }

    private void cmbConfigStartlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (sender is ComboBox cmb)
        updateParamsUC(cmb, getParamsContentControl(cmb));
    }

    /// <summary>
    /// Shows the sub UI for the parameters of the selected view
    /// </summary>
    private void updateParamsUC(ComboBox cmb, ContentControl ccParams)
    {
      if (ccParams == null)
        return;

      ccParams.Content = (cmb.SelectedItem as ViewCBItem)?.GetParamsUC();
    }

    /// <summary>
    /// Selects the view in the ComboBox and sets its parameters in the sub UI (legacy view keys are mapped)
    /// </summary>
    private void selectView(ComboBox cmb, ContentControl ccParams, string viewKey, Dictionary<string, object> viewParams)
    {
      StartlistViewConfig.Normalize(viewKey, viewParams, out string normKey, out var normParams);

      cmb.SelectCBItem(normKey);
      (ccParams.Content as IViewParamsUC)?.SetParams(normParams);
    }
  }
}
