/*
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

using Microsoft.Win32;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
  /// Interaction logic for CompetitionUC.xaml
  /// </summary>
  public partial class CompetitionUC : UserControl
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    AppDataModel _dm;
    DSVInterfaceModel _dsvData;
    FISInterfaceModel _fisData;

    LiveTimingMeasurement _liveTimingMeasurement;
    TextBox _txtLiveTimingStatus;

    public ObservableCollection<ParticipantClass> ParticipantClasses { get; }
    public ObservableCollection<Team> Teams { get; }
    public ObservableCollection<ParticipantCategory> ParticipantCategories { get; }

    public CompetitionUC(AppDataModel dm, LiveTimingMeasurement liveTimingMeasurement, TextBox txtLiveTimingStatus)
    {
      _dm = dm;
      _dsvData = new DSVInterfaceModel(_dm);
      _fisData = new FISInterfaceModel(_dm);

      ParticipantClasses = _dm.GetParticipantClasses();
      Teams = _dm.GetTeams();
      ParticipantCategories = _dm.GetParticipantCategories();

      _liveTimingMeasurement = liveTimingMeasurement;
      _txtLiveTimingStatus = txtLiveTimingStatus;

      InitializeComponent();

      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      _dm.GetRaces().CollectionChanged += OnRacesChanged;

      txtSearch.TextChanged += new DelayedEventHandler(
          TimeSpan.FromMilliseconds(300),
          txtSearch_TextChanged
      ).Delayed;

      this.KeyDown += new KeyEventHandler(KeyDownHandler);

      ConnectGUIToDataModel();
      ConnectGUIToParticipants();

      ucSaveOrReset.Init("Teilnehmeränderungen", null, null, null, storeParticipant, resetParticipant);

      ucClassesAndGroups.Init(_dm);
      ucDSVImport.Init(_dm, _dsvData);
      ucFISImport.Init(_dm, _fisData);

      InitializeGlobalConfig();
    }

    #region RaceTabs

    /// <summary>
    /// Connects the GUI (e.g. Data Grids, ...) to the data model
    /// </summary>
    private void ConnectGUIToDataModel()
    {
      fillAvailableRacesTypes();

      foreach (var r in _dm.GetRaces())
        addRaceTab(r);
    }


    /// <summary>
    /// Represents a tab item for a race
    /// Reason to exist: Getting a "close" or "delete" button within the tab.
    /// </summary>
    public class RaceTabItem : TabItem
    {
      Race _race;
      public RaceTabItem(Race r)
      {
        _race = r;
        RaceTabHeaderUC tabHeader = new RaceTabHeaderUC();
        Header = tabHeader;
        Name = r.RaceType.ToString();
        tabHeader.lblName.Content = r.ToString();
        tabHeader.btnClose.Click += BtnClose_Click;
      }

      public Race Race { get { return _race; } }

      private void BtnClose_Click(object sender, RoutedEventArgs e)
      {
        if (MessageBox.Show(string.Format("Rennen \"{0}\" wirklich löschen?", _race.RaceType), "Rennen löschen?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
          _race.GetDataModel().RemoveRace(_race);
      }
    }


    private void addRaceTab(Race r)
    {
      TabItem tabRace = new RaceTabItem(r);
      tabControlTopLevel.Items.Insert(1, tabRace);

      tabRace.FontSize = 16;

      RaceUC raceUC = new RaceUC(_dm, r, _liveTimingMeasurement, _txtLiveTimingStatus);
      tabRace.Content = raceUC;
    }


    private void OnRacesChanged(object source, NotifyCollectionChangedEventArgs args)
    {
      fillAvailableRacesTypes();

      // Create tabs
      if (args.NewItems != null)
        foreach (var item in args.NewItems)
          if (item is Race race)
            addRaceTab(race);

      if (args.OldItems != null)
        foreach (var item in args.OldItems)
          if (item is Race race)
            foreach (var tab in tabControlTopLevel.Items)
              if (tab is RaceTabItem rTab && rTab.Race == race)
              {
                tabControlTopLevel.Items.Remove(tab);
                break;
              }

      ConnectGUIToParticipants();
    }

    private void OnLiveTimingMeasurementStatusChanged(object sender, bool isRunning)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        EnsureOnlyCurrentRaceCanBeSelected(isRunning);
      });
    }

    private void EnsureOnlyCurrentRaceCanBeSelected(bool onlyCurrentRace)
    {
      foreach (TabItem tab in tabControlTopLevel.Items)
      {
        RaceUC raceUC = tab.Content as RaceUC;
        if (raceUC != null)
        {
          bool isEnabled = !onlyCurrentRace || (_dm.GetCurrentRace() == raceUC.GetRace());
          tab.IsEnabled = isEnabled;
        }
      }
    }

    private void TabControlTopLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var selected = tabControlTopLevel.SelectedContent as RaceUC;
      if (selected != null)
      {
        _dm.SetCurrentRace(selected.GetRace());
        _dm.SetCurrentRaceRun(selected.GetRaceRun());
      }
    }


    Race.ERaceType[] raceTypes = new Race.ERaceType[]
    {
      Race.ERaceType.DownHill,
      Race.ERaceType.SuperG,
      Race.ERaceType.GiantSlalom,
      Race.ERaceType.Slalom,
      Race.ERaceType.KOSlalom,
      Race.ERaceType.ParallelSlalom
    };

    private void fillAvailableRacesTypes()
    {
      cmbRaceType.Items.Clear();
      foreach (var rt in raceTypes)
      {
        if (_dm.GetRaces().FirstOrDefault(r => r.RaceType == rt) == null)
        {
          cmbRaceType.Items.Add(new CBItem { Value = rt, Text = RaceUtil.ToString(rt) });
        }
      }
    }

    private void btnCreateRace_Click(object sender, RoutedEventArgs e)
    {

      if (cmbRaceType.SelectedIndex < 0)
        return;

      Race.ERaceType rt = (Race.ERaceType)((CBItem)cmbRaceType.SelectedItem).Value;

      Race.RaceProperties raceProps = new Race.RaceProperties
      {
        RaceType = rt,
        Runs = 2
      };

      _dm.AddRace(raceProps);
    }

    #endregion

    #region Particpants


    private void btnImport_Click(object sender, RoutedEventArgs e)
    {
      if (_dm?.GetParticipants() == null)
      {
        Logger.Error("Import not possible: datamodel not available");
        return;
      }
      ImportWizard importWizard = new ImportWizard(_dm);
      importWizard.Owner = Window.GetWindow(this);
      importWizard.ShowDialog();
    }


    ParticipantList _editParticipants;
    CollectionViewSource _viewParticipants;
    FilterEventHandler _viewParticipantsFilterHandler;

    /// <summary>
    /// Connects the GUI (e.g. Data Grids, ...) to the data model
    /// </summary>


    private void ConnectGUIToParticipants()
    {
      // Connect with GUI DataGrids
      ObservableCollection<Participant> participants = _dm.GetParticipants();

      _editParticipants = new ParticipantList(participants, _dm, new IImportListProvider[] { _dsvData, _fisData });

      _viewParticipants = new CollectionViewSource();
      _viewParticipants.Source = _editParticipants;

      dgParticipants.ItemsSource = _viewParticipants.View;

      createParticipantOfRaceColumns();
      createParticipantOfRaceCheckboxes();
    }



    /// <summary>
    /// (Re-)Creates the columns for adding/removing the participants to an race
    /// </summary>
    private void createParticipantOfRaceColumns()
    {
      Binding createPointsBinding(int i)
      {
        Binding b = new Binding(string.Format("PointsOfRace[{0}]", i));
        b.Converter = new PointsConverter();
        return b;
      }

      // Delete previous race columns
      while (dgParticipants.Columns.Count > 10)
        dgParticipants.Columns.RemoveAt(dgParticipants.Columns.Count - 1);

      // Add columns for each race
      for (int i = 0; i < _dm.GetRaces().Count; i++)
      {
        Race race = _dm.GetRace(i);
        dgParticipants.Columns.Add(new DataGridCheckBoxColumn
        {
          Binding = new Binding(string.Format("ParticipantOfRace[{0}]", i)),
          Header = race.RaceType.ToString()
        });
        dgParticipants.Columns.Add(new DataGridTextColumn
        {
          Binding = createPointsBinding(i),
          
          Header = string.Format("Points {0}", race.RaceType.ToString())
        });
      }
    }
    /// <summary>
    /// (Re-)Creates the checkboxes for adding/removing the participants to an race
    /// </summary>
    private void createParticipantOfRaceCheckboxes()
    {
      // Delete previous check boxes
      spRaces.Children.Clear();

      // Add checkbox for each race
      for (int i = 0; i < _dm.GetRaces().Count; i++)
      {
        Race race = _dm.GetRace(i);

        CheckBox cb = new CheckBox
        {
          Content = race.RaceType.ToString(),
          Margin = new Thickness(0, 0, 0, 5),
          IsThreeState = true
        };

        spRaces.Children.Add(cb);
      }
    }


    private void dgParticipants_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!_withinStoreParticipant) // for some reason, comboboxes within the data grid send the same signal
        updatePartcipantEditFields();
    }


    private IList<object> GetPropertyValues(IList<object> objects, string propertyName)
    {
      List<object> values = new List<object>();
      foreach (var o in objects)
      {
        object value = PropertyUtilities.GetPropertyValue(o, propertyName);
        values.Add(value);
      }
      return values;
    }

    private void updatePartcipantEditFields()
    {
      IList<object> items = dgParticipants.SelectedItems.Cast<object>().ToList();

      updatePartcipantEditField(txtName, GetPropertyValues(items, "Name"));
      updatePartcipantEditField(txtFirstname, GetPropertyValues(items, "Firstname"));
      updatePartcipantCombobox(cmbSex, GetPropertyValues(items, "Sex"));
      updatePartcipantEditField(txtYear, GetPropertyValues(items, "Year"));
      updatePartcipantEditField(txtClub, GetPropertyValues(items, "Club"));
      updatePartcipantEditField(txtSvId, GetPropertyValues(items, "SvId"));
      updatePartcipantEditField(txtCode, GetPropertyValues(items, "Code"));
      updatePartcipantEditField(txtNation, GetPropertyValues(items, "Nation"));
      updatePartcipantCombobox(cmbClass, GetPropertyValues(items, "Class"));
      updatePartcipantCombobox(cmbTeam, GetPropertyValues(items, "Team"));

      for (int i = 0; i < spRaces.Children.Count; i++)
      {
        List<object> values = new List<object>();
        foreach (var item in items)
        {
          Race race = _dm.GetRace(i);
          if (race != null)
            values.Add(race.GetParticipants().FirstOrDefault(rp => rp.Participant == ((ParticipantEdit)item).Participant) != null);
          else
            values.Add(false);
        }
        updatePartcipantCheckbox(spRaces.Children[i] as CheckBox, values);
      }
    }

    private void updatePartcipantEditField(TextBox control, IList<object> values)
    {
      IEnumerable<object> vsDistinct = values.Distinct();

      if (vsDistinct.Count() == 1)
      {
        var o = vsDistinct.First();
        if (o == null)
          control.Text = null;
        else
          control.Text = o.ToString();
      }
      else if (vsDistinct.Count() > 1)
      {
        control.Text = "<Verschiedene>";
      }
      else // vsDistinct.Count() == 0
      {
        control.Text = "";
      }

      control.IsEnabled = values.Count() > 0;
    }

    private void updatePartcipantCheckbox(CheckBox control, IList<object> values)
    {
      IEnumerable<object> vsDistinct = values.Distinct();

      if (vsDistinct.Count() == 1)
      {
        control.IsChecked = (bool)vsDistinct.First() == true;
        control.IsThreeState = false;
      }
      else if (vsDistinct.Count() > 1)
      {
        control.IsChecked = null;
        control.IsThreeState = true;
      }
      else // vsDistinct.Count() == 0
      {
        control.IsChecked = null;
        control.IsThreeState = true;
      }

      control.IsEnabled = values.Count() > 0;
    }

    private void updatePartcipantCombobox(ComboBox control, IList<object> values)
    {
      IEnumerable<object> vsDistinct = values.Distinct();

      if (vsDistinct.Count() == 1)
      {
        control.SelectedValue = vsDistinct.First();
      }
      else if (vsDistinct.Count() > 1)
      {
        control.SelectedValue = null;
      }
      else // vsDistinct.Count() == 0
      {
        control.SelectedValue = null;
      }

      control.IsEnabled = values.Count() > 0;
    }


    private void resetParticipant()
    {
      updatePartcipantEditFields();
    }


    private bool _withinStoreParticipant = false;
    private void storeParticipant()
    {
      _withinStoreParticipant = true;

      IList<ParticipantEdit> items = dgParticipants.SelectedItems.Cast<ParticipantEdit>().ToList();

      storePartcipantEditField(txtName, items, "Name");
      storePartcipantEditField(txtFirstname, items, "Firstname");
      storePartcipantComboBox(cmbSex, items, "Sex");
      storePartcipantEditField(txtYear, items, "Year");
      storePartcipantEditField(txtClub, items, "Club");
      storePartcipantEditField(txtSvId, items, "SvId");
      storePartcipantEditField(txtCode, items, "Code");
      storePartcipantEditField(txtNation, items, "Nation");
      storePartcipantComboBox(cmbClass, items, "Class");
      storePartcipantComboBox(cmbTeam, items, "Team");

      for (int i = 0; i < spRaces.Children.Count; i++)
      {
        CheckBox cb = (CheckBox)spRaces.Children[i];
        if (cb.IsChecked != null) // Either true or false, but not "third state"
        {
          bool bVal = (bool)cb.IsChecked;
          foreach (var item in items.Cast<ParticipantEdit>())
          {
            item.ParticipantOfRace[i] = bVal;
          }
        }
      }

      if (items.Count == 1 && items[0].Class == null)
      {
        ClassAssignment ca = new ClassAssignment(_dm.GetParticipantClasses());
        ca.Assign(items[0].Participant);
        updatePartcipantEditFields();
      }

      _withinStoreParticipant = false;
    }

    private void storePartcipantEditField(TextBox control, IList<ParticipantEdit> items, string propertyName)
    {
      if (control.Text == "<Verschiedene>")
        return;

      foreach (var item in items.Cast<ParticipantEdit>())
        PropertyUtilities.SetPropertyValue(item, propertyName, control.Text);
    }

    private void storePartcipantComboBox(ComboBox control, IList<ParticipantEdit> items, string propertyName)
    {
      if (control.SelectedValue == null)
        return;

      var value = control.SelectedValue;
      foreach (var item in items.Cast<ParticipantEdit>())
        PropertyUtilities.SetPropertyValue(item, propertyName, value);
    }

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (_viewParticipantsFilterHandler != null)
        _viewParticipants.Filter -= _viewParticipantsFilterHandler;

      string sFilter = txtSearch.Text;

      _viewParticipantsFilterHandler = null;
      if (!string.IsNullOrEmpty(sFilter))
      {
        _viewParticipantsFilterHandler = new FilterEventHandler(delegate (object s, FilterEventArgs ea)
        {
          bool contains(string bigString, string part)
          {
            if (string.IsNullOrEmpty(bigString))
              return false;

            return System.Threading.Thread.CurrentThread.CurrentCulture.CompareInfo.IndexOf(bigString, part, CompareOptions.IgnoreCase) >= 0;
          }

          ParticipantEdit p = (ParticipantEdit)ea.Item;

          ea.Accepted =
                contains(p.Name, sFilter)
            || contains(p.Firstname, sFilter)
            || contains(p.Club, sFilter)
            || contains(p.Nation, sFilter)
            || contains(p.Year.ToString(), sFilter)
            || contains(p.Code, sFilter)
            || contains(p.SvId, sFilter)
            || contains(p.Class?.ToString(), sFilter)
            || contains(p.Group?.ToString(), sFilter);
        });
      }
      if (_viewParticipantsFilterHandler != null)
        _viewParticipants.Filter += _viewParticipantsFilterHandler;

      _viewParticipants.View.Refresh();
    }

    private void KeyDownHandler(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        txtSearch.Focus();
        txtSearch.SelectAll();
      }
    }

    private void btnClearSearch_Click(object sender, RoutedEventArgs e)
    {
      txtSearch.Text = "";
      txtSearch.Focus();
    }

    private void btnAssignAllClasses_Click(object sender, RoutedEventArgs e)
    {
      ClassAssignment ca = new ClassAssignment(_dm.GetParticipantClasses());
      ca.Assign(_dm.GetParticipants());
    }


    private void btnResetClass_Click(object sender, RoutedEventArgs e)
    {
      List<Participant> participants = new List<Participant>();
      foreach (var pe in dgParticipants.SelectedItems.Cast<ParticipantEdit>())
        participants.Add(pe.Participant);

      ClassAssignment ca = new ClassAssignment(_dm.GetParticipantClasses());
      ca.Assign(participants);
    }

    private void btnAddParticipant_Click(object sender, RoutedEventArgs e)
    {
      Participant participant = new Participant();
      _dm.GetParticipants().Add(participant);

      ParticipantEdit item = _editParticipants.FirstOrDefault(p => p.Participant == participant);
      dgParticipants.SelectedItem = item;

      txtName.Focus();
    }

    private void btnDeleteParticipant_Click(object sender, RoutedEventArgs e)
    {
      ParticipantEdit[] selectedItems = new ParticipantEdit[dgParticipants.SelectedItems.Count];
      dgParticipants.SelectedItems.CopyTo(selectedItems, 0);
      if (selectedItems.Length > 0)
      {
        string szQuestion = string.Format("Sollen die markierten {0} Teilnehmer gelöscht werden?", selectedItems.Length);
        if (MessageBox.Show(szQuestion, "Teilnehmer löschen?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
        {
          foreach (var item in selectedItems)
          {
            if (item is ParticipantEdit pe)
            {
              Participant participant = pe.Participant;
              _dm.GetParticipants().Remove(participant);
            }
          }
        }
      }
    }

    #endregion

    #region Global Config

    private void InitializeGlobalConfig()
    {
      ucRaceConfig.Init(_dm.GlobalRaceConfig, null);

      ucRaceConfigSaveOrReset.Init(
        "Konfigurationsänderungen",
        null, null,
        globalConfig_ExistingChanges, globalConfig_SaveChanges, globalConfig_ResetChanges);
    }


    private bool globalConfig_ExistingChanges()
    {
      return ucRaceConfig.ExistingChanges();
    }

    private void globalConfig_SaveChanges()
    {
      RaceConfiguration cfg = ucRaceConfig.GetConfig();
      _dm.GlobalRaceConfig = cfg;

      ucRaceConfig.Init(_dm.GlobalRaceConfig, null);
    }

    private void globalConfig_ResetChanges()
    {
      ucRaceConfig.ResetChanges();
    }

    #endregion

    private void txtControlGotFocus(object sender, RoutedEventArgs e)
    {
      if (sender is TextBox tb)
        tb.SelectAll();
    }
  }


  /// <summary>
  /// Represents and modified the participants membership to a race
  /// Example: ParticpantOfRace[i] = true | false
  /// </summary>
  public class ParticpantOfRace : INotifyPropertyChanged
  {
    Participant _participant;
    IList<Race> _races;

    public ParticpantOfRace(Participant p, IList<Race> races )
    {
      _participant = p;
      _races = races;
    }

    public bool this[int i]
    {
      get 
      {
        if (i >= _races.Count)
          return false;
        
        return _races[i].GetParticipants().FirstOrDefault(rp => rp.Participant == _participant) != null; 
      }

      set
      {
        if (i < _races.Count)
        {
          if (value == true)
          {
            _races[i].AddParticipant(_participant);
          }
          else
          {
            _races[i].RemoveParticipant(_participant);
          }

          NotifyPropertyChanged();
        }
      }
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }


  /// <summary>
  /// Represents and modified the points an participants has for a race
  /// Example: PointsOfRace[i] = 1.23
  /// </summary>
  public class PointsOfRace : INotifyPropertyChanged
  {
    Participant _participant;
    IList<Race> _races;

    public PointsOfRace(Participant p, IList<Race> races)
    {
      _participant = p;
      _races = races;

      foreach(var r in _races)
        r.GetParticipants().CollectionChanged += RaceParticipants_CollectionChanged;

      observeRaces();
    }
    private static bool relatedTo(NotifyCollectionChangedEventArgs e, Participant p)
    {
      bool relatedTo(System.Collections.IList list, Participant lp)
      {
        if (list != null)
          foreach (var i in list)
            if (i is RaceParticipant rp)
              if (rp.Participant == lp)
                return true;
        return false;
      }

      if (relatedTo(e.NewItems, p))
        return true;

      if (relatedTo(e.OldItems, p))
        return true;

      return false;
    }

    private void RaceParticipants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (relatedTo(e, _participant))
        observeRaces();
    }

    private void observeRaces()
    {
      for (int i = 0; i < _races.Count; i++)
      {
        var rp = getRaceParticipant(i);
        if (rp != null)
          rp.PropertyChanged += RaceParticipant_PropertyChanged;
      }
    }

    private void RaceParticipant_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyPropertyChanged("Item");
    }

    private RaceParticipant getRaceParticipant(int i)
    {
      if (i >= _races.Count)
        return null;

      return _races[i].GetParticipants().FirstOrDefault(rp => rp.Participant == _participant);
    }

    public double this[int i]
    {
      get
      {
        RaceParticipant rp = getRaceParticipant(i);
        return rp != null ? rp.Points : -1.0;
      }

      set
      {
        RaceParticipant rp = getRaceParticipant(i);
        if (rp!=null)
        {
          if (rp.Points != value)
          {
            rp.Points = value;
            NotifyPropertyChanged();
          }
        }
      }
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }


  /// <summary>
  /// Represents a row within the participant data grid for editing participants
  /// - Proxies standard participant properties
  /// - Contains ParticpantOfRace to modifiy the membership to a race (eg. pe.ParticpantOfRace[i] = true | false )
  /// </summary>
  public class ParticipantEdit : INotifyPropertyChanged
  {
    Participant _participant;
    ParticpantOfRace _participantOfRace;
    PointsOfRace _pointsOfRace;
    bool _existsInImportList;

    IImportListProvider[] _importList;

    public ParticipantEdit(Participant p, IList<Race> races, IImportListProvider[] importList)
    {
      _participant = p;
      _participant.PropertyChanged += OnParticpantPropertyChanged;

      _importList = importList;
      foreach(var il in _importList)
        il.DataChanged += onDataChangedImportList;
      updateExistsInImport();

      _participantOfRace = new ParticpantOfRace(p, races);
      _participantOfRace.PropertyChanged += OnParticpantOfRaceChanged;

      _pointsOfRace = new PointsOfRace(p, races);
      _pointsOfRace.PropertyChanged += OnPointsOfRaceChanged;
    }

    private void _pointsOfRace_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      throw new NotImplementedException();
    }

    void onDataChangedImportList(object sender, EventArgs e)
    {
      updateExistsInImport();
    }

    void updateExistsInImport()
    {
      ExistsInImportList = checkInImport();
    }


    bool checkInImport()
    {
      bool res = true;
      foreach (var il in _importList)
      {
        res &= (il == null || il.ContainsParticipant(_participant));
      }
      return res;
    }


    public Participant Participant
    {
      get => _participant;
    }

    public string Id 
    { 
      get => _participant.Id; 
    }

    public string Name
    {
      get => _participant.Name;
      set => _participant.Name = value;
    }

    public string Firstname
    {
      get => _participant.Firstname;
      set => _participant.Firstname = value;
    }
    public ParticipantCategory Sex
    {
      get => _participant.Sex;
      set => _participant.Sex = value;
    }
    public uint Year
    {
      get => _participant.Year;
      set => _participant.Year = value;
    }
    public string Club
    {
      get => _participant.Club;
      set => _participant.Club = value;
    }
    public string SvId
    {
      get => _participant.SvId;
      set => _participant.SvId = value;
    }
    public string Code
    {
      get => _participant.Code;
      set => _participant.Code = value;
    }
    public string Nation
    {
      get => _participant.Nation;
      set => _participant.Nation = value;
    }
    public ParticipantClass Class
    {
      get => _participant.Class;
      set => _participant.Class = value;
    }
    public ParticipantGroup Group
    {
      get => _participant.Group;
    }
    public Team Team
    {
      get => _participant.Team;
      set => _participant.Team = value;
    }


    public ParticpantOfRace ParticipantOfRace
    {
      get => _participantOfRace;
    }

    public PointsOfRace PointsOfRace
    {
      get => _pointsOfRace;
    }


    public bool ExistsInImportList
    {
      private set { if (_existsInImportList != value) { _existsInImportList = value; NotifyPropertyChanged(); } }
      get => _existsInImportList;
    }

    private void OnParticpantOfRaceChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ParticipantOfRace"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PointsOfRace"));
    }

    private void OnPointsOfRaceChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PointsOfRace"));
    }

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnParticpantPropertyChanged(object source, PropertyChangedEventArgs eargs)
    {
      updateExistsInImport();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(eargs.PropertyName));
    }

    private void OnPpointsOfRaceChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ParticipantOfRace"));
    }

    #endregion
  }


  /// <summary>
  /// Represents the complete participant data grid for editing participants
  /// </summary>
  public class ParticipantList : CopyObservableCollection<ParticipantEdit,Participant>
  {
    public ParticipantList(ObservableCollection<Participant> particpants, AppDataModel dm, IImportListProvider[] importList) 
      : base(particpants, p => new ParticipantEdit(p, dm.GetRaces(), importList), false)
    { }

  }
}
