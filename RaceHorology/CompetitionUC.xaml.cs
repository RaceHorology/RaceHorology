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
    LiveTimingMeasurement _liveTimingMeasurement;
    TextBox _txtLiveTimingStatus;

    public ObservableCollection<ParticipantClass> ParticipantClasses { get; }
    public ObservableCollection<ParticipantCategory> ParticipantCategories { get; }

    public CompetitionUC(AppDataModel dm, LiveTimingMeasurement liveTimingMeasurement, TextBox txtLiveTimingStatus)
    {
      _dm = dm;

      ParticipantClasses = _dm.GetParticipantClasses();
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

      initDSVAddToList();

      ucClassesAndGroups.Init(_dm);
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
        tabHeader.lblName.Content = r.RaceType.ToString();
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
      EnsureOnlyCurrentRaceCanBeSelected(isRunning);
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
          cmbRaceType.Items.Add(rt);
        }
      }
    }

    private void btnCreateRace_Click(object sender, RoutedEventArgs e)
    {

      if (cmbRaceType.SelectedIndex < 0)
        return;

      Race.ERaceType rt = (Race.ERaceType)cmbRaceType.SelectedItem;

      Race.RaceProperties raceProps = new Race.RaceProperties
      {
        RaceType = rt
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
      _editParticipants = new ParticipantList(participants, _dm);

      _viewParticipants = new CollectionViewSource();
      _viewParticipants.Source = _editParticipants;

      dgParticipants.ItemsSource = _viewParticipants.View;

      createParticipantOfRaceColumns();
      createParticipantOfRaceCheckboxes();
    }


    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
      updatePartcipantEditFields();
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
      storeParticipant();
    }


    /// <summary>
    /// (Re-)Creates the columns for adding/removing the participants to an race
    /// </summary>
    private void createParticipantOfRaceColumns()
    {
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

      for (int i = 0; i < spRaces.Children.Count; i++)
      {
        List<object> values = new List<object>();
        foreach (var item in items)
        {
          values.Add(_dm.GetRace(i).GetParticipants().FirstOrDefault(rp => rp.Participant == ((ParticipantEdit)item).Participant) != null);
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


    private void storeParticipant()
    {
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

    #endregion

    private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
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
      });
    }

    private void KeyDownHandler(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        txtSearch.Focus();
        txtSearch.SelectAll();
      }
    }

    private void btnAssignClasses_Click(object sender, RoutedEventArgs e)
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

    private void showImportResult(List<ImportResults> impRes, string usedDSVLists)
    {
      string messageTextDetails = "";

      messageTextDetails += string.Format("Benutzte DSV Liste: {0}\n\n", usedDSVLists);

      int nRace = 0;
      foreach (var i in impRes)
      {
        Race race = _dm.GetRace(nRace);

        string notFoundParticipants = string.Join("\n", i.Errors);

        messageTextDetails += string.Format(
          "Zusammenfassung für das Rennen \"{0}\":\n" +
          "- Punkte erfolgreich aktualisiert: {1}\n",
          race.ToString(), i.SuccessCount);

        if (i.ErrorCount > 0)
        {
          messageTextDetails += string.Format("\n" +
            "- Teilnehmer nicht gefunden: {0}\n" +
            "{1}",
            i.ErrorCount, notFoundParticipants);
        }

        messageTextDetails += "\n";
      }

      MessageBox.Show("Der Importvorgang wurde abgeschlossen: \n\n" + messageTextDetails, "Importvorgang", MessageBoxButton.OK, MessageBoxImage.Information);
    }


    private void btnAddParticipant_Click(object sender, RoutedEventArgs e)
    {
      Participant participant = new Participant();
      _dm.GetParticipants().Add(participant);

      ParticipantEdit item = _editParticipants.FirstOrDefault(p => p.Participant == participant);
      dgParticipants.SelectedItem = item;
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


    #region DSVAddToList

    DSVInterfaceModel _dsvData;
    CollectionViewSource _viewDSVList;

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


    #endregion
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
  /// Represents a row within the participant data grid for editing participants
  /// - Proxies standard participant properties
  /// - Contains ParticpantOfRace to modifiy the membership to a race (eg. pe.ParticpantOfRace[i] = true | false )
  /// </summary>
  public class ParticipantEdit : INotifyPropertyChanged
  {
    Participant _participant;
    ParticpantOfRace _participantOfRace;

    public ParticipantEdit(Participant p, IList<Race> races)
    {
      _participant = p;
      _participant.PropertyChanged += OnParticpantPropertyChanged;

      _participantOfRace = new ParticpantOfRace(p, races);
      _participantOfRace.PropertyChanged += OnParticpantOfRaceChanged;
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


    public ParticpantOfRace ParticipantOfRace
    { 
      get => _participantOfRace; 
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
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(eargs.PropertyName));
    }

    private void OnParticpantOfRaceChanged(object source, PropertyChangedEventArgs eargs)
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
    public ParticipantList(ObservableCollection<Participant> particpants, AppDataModel dm) : base(particpants, p => new ParticipantEdit(p, dm.GetRaces()))
    { }

  }
}
