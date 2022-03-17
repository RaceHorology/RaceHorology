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
  /// Interaction logic for RaceConfigurationUC.xaml
  /// </summary>
  public partial class RaceConfigurationUC : UserControl
  {

    RaceConfiguration _raceConfiguration;
    RaceConfigurationPresets _raceConfigurationPresets;


    public RaceConfigurationUC()
    {
      InitializeComponent();
    }


    public void Init(RaceConfiguration raceConfig)
    {
      // ApplicationFolder + raceconfigpresets
      _raceConfigurationPresets = new RaceConfigurationPresets(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"raceconfigpresets"));
      _raceConfiguration = raceConfig.Copy();

      refreshConfigPresetsUI();

      // Configuration Screen
      cmbRuns.Items.Clear();
      cmbRuns.Items.Add(new CBItem { Text = "1", Value = 1 });
      cmbRuns.Items.Add(new CBItem { Text = "2", Value = 2 });
      cmbRuns.Items.Add(new CBItem { Text = "3", Value = 3 });
      cmbRuns.Items.Add(new CBItem { Text = "4", Value = 4 });
      cmbRuns.Items.Add(new CBItem { Text = "5", Value = 5 });
      cmbRuns.Items.Add(new CBItem { Text = "6", Value = 6 });

      // Result
      UiUtilities.FillGrouping(cmbConfigErgebnisGrouping);

      cmbConfigErgebnis.Items.Clear();
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Bester Durchgang", Value = "RaceResult_BestOfTwo" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe der besten 2 Durchgänge", Value = "RaceResult_SumBest2" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe", Value = "RaceResult_Sum" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach DSV Schülerreglement", Value = "RaceResult_SumDSVPointsSchool" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach Tabelle", Value = "RaceResult_SumPointsViaTable" });

      // Run 1
      UiUtilities.FillGrouping(cmbConfigStartlist1Grouping);
      cmbConfigStartlist1.Items.Clear();
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_1stRun_StartnumberAscending" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (nicht gelost)", Value = "Startlist_1stRun_Points_0" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 15 gelost)", Value = "Startlist_1stRun_Points_15" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 30 gelost)", Value = "Startlist_1stRun_Points_30" });

      // Run 2
      UiUtilities.FillGrouping(cmbConfigStartlist2Grouping);
      cmbConfigStartlist2.Items.Clear();
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_2nd_StartnumberAscending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (aufsteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberAscending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (absteigend)", Value = "Startlist_2nd_StartnumberDescending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (absteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberDescending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (nicht gedreht)", Value = "Startlist_2nd_PreviousRun_0_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (nicht gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_0_AlsoWithoutResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 15 gedreht)", Value = "Startlist_2nd_PreviousRun_15_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 15 gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_15_AlsoWithoutResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 30 gedreht)", Value = "Startlist_2nd_PreviousRun_30_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 30 gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_30_AlsoWithoutResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (alle gedreht)", Value = "Startlist_2nd_PreviousRun_all_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (alle gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_all_AlsoWithoutResults" });

      ResetConfigurationSelectionUI(_raceConfiguration);
    }


    private void CmbTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTemplate.SelectedValue is CBItem selected)
      {
        if (selected.Value is string configName)
        {
          RaceConfiguration config = _raceConfigurationPresets.GetConfiguration(configName);
          RaceConfiguration configToSet = RaceConfigurationMerger.MainConfig(_raceConfiguration, config);

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
      cmbConfigStartlist1.SelectCBItem(cfg.Run1_StartistView);
      cmbConfigStartlist1Grouping.SelectCBItem(cfg.Run1_StartistViewGrouping);
      cmbConfigStartlist2.SelectCBItem(cfg.Run2_StartistView);
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

      if (cmbConfigStartlist1.SelectedIndex >= 0)
        cfg.Run1_StartistView = (string)((CBItem)cmbConfigStartlist1.SelectedValue).Value;

      if (cmbConfigStartlist1Grouping.SelectedIndex >= 0)
        cfg.Run1_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist1Grouping.SelectedValue).Value;

      if (cmbConfigStartlist2.SelectedIndex >= 0)
        cfg.Run2_StartistView = (string)((CBItem)cmbConfigStartlist2.SelectedValue).Value;

      if (cmbConfigStartlist2Grouping.SelectedIndex >= 0)
        cfg.Run2_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist2Grouping.SelectedValue).Value;

      try { cfg.ValueF = double.Parse(txtValueF.Text); } catch (Exception) { }
      try { cfg.ValueA = double.Parse(txtValueA.Text); } catch (Exception) { }
      try { cfg.ValueZ = double.Parse(txtValueZ.Text); } catch (Exception) { }
      try { cfg.MinimumPenalty = double.Parse(txtMinPenalty.Text); } catch (Exception) { }
      try { cfg.ValueCutOff = double.Parse(txtValueCutOff.Text); } catch (Exception) { }

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
  }
}
