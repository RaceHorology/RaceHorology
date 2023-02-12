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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RaceHorologyLib
{
  /* Just fro Debugging 
  class DebugSort : System.Collections.IComparer
  {
    public int Compare(object x, object y)
    {
      return 0;
    }
  }
  */

  /// <summary>
  /// BaseClass for all ViewProvider
  /// </summary>
  public abstract class ViewProvider
  {
    protected CollectionViewSource _view;

    protected string _defaultGrouping;
    protected string _activeGrouping;


    public ViewProvider()
    {
      _view = new CollectionViewSource();
    }

    public abstract ViewProvider Clone();


    protected void FinalizeInit()
    {
      _view.Source = GetViewSource();
    }

    public ICollectionView GetView()
    {
      return _view.View;
    }

    public void SetDefaultGrouping(string propertyName)
    {
      _defaultGrouping = propertyName;
      ResetToDefaultGrouping();
    }

    public void ChangeGrouping(string propertyName)
    {
      if (_activeGrouping == propertyName)
        return;

      if (!string.IsNullOrEmpty(_activeGrouping))
      {
        _view.GroupDescriptions.Clear();
        _view.LiveGroupingProperties.Clear();
        _view.IsLiveGroupingRequested = false;
      }

      if (!string.IsNullOrEmpty(propertyName))
      {
        GroupDescription gd = new PropertyGroupDescription(propertyName);
        gd.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        _view.GroupDescriptions.Add(gd);

        _view.LiveGroupingProperties.Add(propertyName);
        _view.IsLiveGroupingRequested = true;
      }

      _activeGrouping = propertyName;

      OnChangeGrouping(propertyName);
    }

    protected virtual void OnChangeGrouping(string propertyName) { }


    public void ResetToDefaultGrouping()
    {
      ChangeGrouping(_defaultGrouping);
    }

    public string ActiveGrouping { get { return _activeGrouping; } }


    public delegate T1 Creator<T1, T2>(T2 source);
    protected static void PopulateInitially<TC, TSource>(Collection<TC> collection, System.Collections.IEnumerable sourceItems, IComparer<TC> comparer, Creator<TC, TSource> creator)
    {
      foreach (TSource item in sourceItems)
      {
        TC colItem = creator(item);
        collection.InsertSorted(colItem, comparer);
      }
    }

    protected abstract object GetViewSource();

  }


  public class StartListEntryComparer : System.Collections.Generic.IComparer<StartListEntry>
  {
    public enum Direction : int { Ascending = 1, Descending = -1 };
    Direction _direction;
    public StartListEntryComparer(Direction direction = Direction.Ascending)
    {
      _direction = direction;
    }
    public virtual int Compare(StartListEntry left, StartListEntry right)
    {

      int groupCompare = CompareGroup(left, right);
      if (groupCompare != 0)
        return groupCompare;

      if (left.StartNumber < right.StartNumber)
        return -1 * (int)_direction;
      else if (left.StartNumber > right.StartNumber)
        return 1 * (int)_direction;
      else
        return 0;
    }



    string _groupingPropertyName;
    public void SetGrouping(string propertyName)
    {
      _groupingPropertyName = propertyName;
    }


    NullEnabledComparer nullEnabledComparer = new NullEnabledComparer();
    protected int CompareGroup(StartListEntry rrX, StartListEntry rrY)
    {
      int groupCompare = 0;
      if (_groupingPropertyName == "Participant.Class")
        groupCompare = nullEnabledComparer.Compare(rrX.Participant.Participant.Class, rrY.Participant.Participant.Class);
      else if (_groupingPropertyName == "Participant.Group")
        groupCompare = nullEnabledComparer.Compare(rrX.Participant.Participant.Group, rrY.Participant.Participant.Group);
      else if (_groupingPropertyName == "Participant.Sex")
        groupCompare = nullEnabledComparer.Compare(rrX.Participant.Participant.Sex, rrY.Participant.Participant.Sex);

      return groupCompare;
    }
  }

  public interface IStartListViewProvider
  {
    ICollectionView GetView();
  }

  public abstract class StartListViewProvider : ViewProvider, IStartListViewProvider
  {
    protected ObservableCollection<StartListEntry> _viewList;

    public StartListViewProvider()
    {
    }

    // Output: sorted List<StartListEntry> according to StartNumber
    public ObservableCollection<StartListEntry> GetViewList()
    {
      return _viewList;
    }


    protected static StartListEntry CreateStartListEntry(RaceParticipant participant)
    {
      return new StartListEntry(participant);
    }

    protected override object GetViewSource()
    {
      return _viewList;
    }

  }


  /// <summary>
  /// Creates a start list by comparing the start number taking into account the Sorting and Grouping
  /// </summary>
  public class FirstRunStartListViewProvider : StartListViewProvider
  {
    protected ObservableCollection<RaceParticipant> _participants;
    protected ItemsChangedNotifier _sourceItemChangedNotifier;

    protected StartListEntryComparer _comparer;


    public FirstRunStartListViewProvider()
    {
      _comparer = new StartListEntryComparer();
    }

    public override ViewProvider Clone()
    {
      return new FirstRunStartListViewProvider();
    }

    // Input: List<RaceParticipant>
    public void Init(ObservableCollection<RaceParticipant> participants)
    {
      _participants = participants;

      _viewList = new ObservableCollection<StartListEntry>();

      // Initialize and observe source list
      PopulateInitially<StartListEntry, RaceParticipant>(_viewList, _participants, _comparer, CreateStartListEntry);
      _participants.CollectionChanged += OnParticipantsChanged;
      _sourceItemChangedNotifier = new ItemsChangedNotifier(_participants);
      _sourceItemChangedNotifier.ItemChanged += _sourceItemChangedNotifier_ItemChanged;

      base.FinalizeInit();
    }


    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);

      sortViewList();
    }


    private void OnParticipantsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RaceParticipant participant = (RaceParticipant)item;
          var itemsToRemove = _viewList.Where(r => r.Participant.Participant == participant.Participant).ToList();
          foreach (var itemToRemove in itemsToRemove)
            _viewList.Remove(itemToRemove);
        }

      if (e.NewItems != null)
      {
        foreach (INotifyPropertyChanged item in e.NewItems)
          _viewList.Add(CreateStartListEntry((RaceParticipant)item));

        sortViewList();
      }
    }


    private void _sourceItemChangedNotifier_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      // Ensure list is sorted again
      sortViewList();
    }


    protected virtual void sortViewList()
    {
      // Ensure list is sorted again
      if (_viewList != null)
        _viewList.Sort(_comparer);
    }
  }



  public class PointsStartListEntryComparer : StartListEntryComparer
  {
    protected uint _firstStartnumber;
    protected int _firstNStartnumbers;
    public PointsStartListEntryComparer(int firstNStartnumbers)
    {
      _firstNStartnumbers = firstNStartnumbers;
      _firstStartnumber = 1;
    }

    public uint FirstStartNumber
    {
      get => _firstStartnumber;
      set => _firstStartnumber = value;
    }

    public override int Compare(StartListEntry left, StartListEntry right)
    {
      int groupCompare = CompareGroup(left, right);
      if (groupCompare != 0)
        return groupCompare;

      if ((left.StartNumber - _firstStartnumber) < _firstNStartnumbers)
      {
        if ((right.StartNumber - _firstStartnumber) < _firstNStartnumbers)
          return left.StartNumber.CompareTo(right.StartNumber);
        else
          return -1;
      }

      // Left Startnumber is bigger than _firstNStartnumbers
      if ((right.StartNumber - _firstStartnumber) < _firstNStartnumbers)
        return +1;

      // According to points, but other direction
      int compPoints = left.Points.CompareTo(right.Points);
      if (compPoints != 0)
        return compPoints;

      // If Points are equal, sort by startnumber
      return left.StartNumber.CompareTo(right.StartNumber);
    }
  }



  /// <summary>
  /// Provides a start list based on startnumber and points following the criterias:
  /// - Best first firstNStartnumbers (15) based on the points are randomized
  /// - Succeeding start list entries are sorted based on the points
  /// 
  /// Use case: 
  /// - Define start numbers based on DSV rules (15 random, remaining points descending)
  /// - Late registration: shall not influence first 15, however it shall be in the correct order after the first 15 starters.
  /// </summary>
  public class DSVFirstRunStartListViewProvider : FirstRunStartListViewProvider
  {
    int _firstNStartnumbers;

    public DSVFirstRunStartListViewProvider(int firstNStartnumbers)
    {
      _firstNStartnumbers = firstNStartnumbers;
    }

    public override ViewProvider Clone()
    {
      return new DSVFirstRunStartListViewProvider(_firstNStartnumbers);
    }

    protected override void sortViewList()
    {
      if (_viewList == null)
        return;

      // Re-Sort based on DSV Strategy per Group
      PointsStartListEntryComparer pointsComparer = new PointsStartListEntryComparer(_firstNStartnumbers);
      void sortGroup(int start, int end)
      {
        pointsComparer.FirstStartNumber = _viewList[start].StartNumber;
        _viewList.Sort(pointsComparer, start, end);
      }

      // Ensure list is sorted again
      if (_viewList != null)
        _viewList.Sort(_comparer);

      // Process each group separately
      object curGroup = null;
      int curGroupStart = 0, curGroupEnd = 0;
      for (int i = 0; i < _viewList.Count; ++i)
      {
        var item = _viewList[i];
        object itemGroup = PropertyUtilities.GetPropertyValue(item, _activeGrouping);
        if (!Equals(PropertyUtilities.GetPropertyValue(item, _activeGrouping), curGroup))
        {
          // New group starts
          sortGroup(curGroupStart, curGroupEnd);
          curGroup = itemGroup;
          curGroupStart = i;
        }
        curGroupEnd = i;
      }
      sortGroup(curGroupStart, curGroupEnd);
    }
  }



  /// <summary>
  /// Base class for start list providers using results from a previous run
  /// </summary>
  public abstract class SecondRunStartListViewProvider : StartListViewProvider
  {
    // Input: List<StartListEntry> (1st run),
    //        List<RaceResultWithPosition> (1st run)

    public abstract void Init(RaceRun previousRun);

    // Output: sorted List<StartListEntry>

  }


  /// <summary>
  /// Simplest form of a start list provider for 2nd run. Supports:
  /// - Based startnumber ascending
  /// - Based startnumber descending
  /// </summary>
  public class SimpleSecondRunStartListViewProvider : SecondRunStartListViewProvider
  {
    // Input
    protected ObservableCollection<StartListEntry> _startList1stRun;
    protected StartListEntryComparer.Direction _direction;

    // Working
    ItemsChangedNotifier _sourceItemChangedNotifier;
    StartListEntryComparer _comparer;


    public SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction direction)
    {
      _direction = direction;
      _comparer = new StartListEntryComparer(direction);
    }

    public override ViewProvider Clone()
    {
      return new SimpleSecondRunStartListViewProvider(_direction);
    }


    public override void Init(RaceRun previousRun)
    {
      _viewList = new ObservableCollection<StartListEntry>();

      StartListViewProvider slPR1st = previousRun.GetStartListProvider();
      _startList1stRun = slPR1st.GetViewList();

      // Initialize and observe source list
      PopulateInitially<StartListEntry, StartListEntry>(_viewList, _startList1stRun, _comparer, CreateStartListEntry);
      _startList1stRun.CollectionChanged += OnSourceChanged;
      _sourceItemChangedNotifier = new ItemsChangedNotifier(_startList1stRun);
      _sourceItemChangedNotifier.ItemChanged += _sourceItemChangedNotifier_ItemChanged;

      FinalizeInit();
    }


    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);

      // Ensure list is sorted again
      if (_viewList != null)
        _viewList.Sort(_comparer);
    }


    private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          StartListEntry sle = (StartListEntry)item;
          var itemsToRemove = _viewList.Where(i => i.Participant == sle.Participant).ToList();
          foreach (var itemToRemove in itemsToRemove)
            _viewList.Remove(itemToRemove);
        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
          _viewList.InsertSorted(CreateStartListEntry((StartListEntry)item), _comparer);
    }

    private StartListEntry CreateStartListEntry(StartListEntry sleSRC)
    {
      return new StartListEntry(sleSRC.Participant);
    }


    private void _sourceItemChangedNotifier_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      // Ensure list is sorted again
      _viewList.Sort(_comparer);
    }


  }


  // basierend auf (1. DG) Ergebnisliste: rückwärts, ersten n gelost, mit/ohne disqualifizierten vorwärts oder rückwärts
  public class BasedOnResultsFirstRunStartListViewProvider : SecondRunStartListViewProvider
  {
    int _reverseBestN;
    bool _allowNonResults;
    RuntimeSorter _resultsComparer;
    RaceRun _previousRun;

    //RaceRunResultViewProvider _resultVPPreviousRun;
    ItemsChangeObservableCollection<RunResult> _resultsPreviousRun;


    public RaceRun BasedOnRun { get { return _previousRun; } }

    public BasedOnResultsFirstRunStartListViewProvider(int reverseBestN, bool allowNonResults)
    {
      _reverseBestN = reverseBestN;
      _allowNonResults = allowNonResults;

      _resultsComparer = new RuntimeSorter(startNumberAscending: false);
    }


    public override ViewProvider Clone()
    {
      return new BasedOnResultsFirstRunStartListViewProvider(_reverseBestN, _allowNonResults);
    }


    public override void Init(RaceRun previousRun)
    {
      _viewList = new ObservableCollection<StartListEntry>();

      _previousRun = previousRun;
      _resultsPreviousRun = previousRun.GetResultList();
      _resultsPreviousRun.CollectionChanged += OnSourceChanged;
      _resultsPreviousRun.ItemChanged += _sourceItemChangedNotifier_ItemChanged;

      UpdateStartList();

      FinalizeInit();
    }


    private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      UpdateStartList();
    }
    private void _sourceItemChangedNotifier_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      UpdateStartList();
    }


    protected override void OnChangeGrouping(string propertyName)
    {
      _resultsComparer.SetGrouping(propertyName);
      UpdateStartList();
    }


    class SortByStartnumberDesc : IComparer<RunResult>
    {
      public int Compare(RunResult rrX, RunResult rrY)
      {
        return rrX.Participant.StartNumber.CompareTo(rrY.Participant.StartNumber) * -1;
      }

    }


    private void UpdateStartList()
    {
      // Not yet initialized
      if (_resultsPreviousRun == null)
        return;

      List<StartListEntry> newStartList = new List<StartListEntry>();

      // Create sorted results for all participants
      List<RunResult> srcResults = new List<RunResult>();
      srcResults.AddRange(_resultsPreviousRun);
      srcResults.Sort(_resultsComparer);

      // Process each group separately
      object curGroup = null;
      List<RunResult> resultsCurGroup = new List<RunResult>();
      foreach (var curSortedItem in srcResults)
      {
        object itemGroup = PropertyUtilities.GetPropertyValue(curSortedItem, _activeGrouping);
        if (!Equals(PropertyUtilities.GetPropertyValue(curSortedItem, _activeGrouping), curGroup))
        {
          ProcessGroup(resultsCurGroup, newStartList);
          curGroup = itemGroup;
        }
        resultsCurGroup.Add(curSortedItem);
      }
      ProcessGroup(resultsCurGroup, newStartList);

      // Copy at once to trigger only one change notification
      _viewList.Clear();
      _viewList.InsertRange(newStartList);
    }


    protected void ProcessGroup(List<RunResult> resultsCurGroup, List<StartListEntry> newStartList)
    {
      // Find how many valid results are there (could be less or more than _reverseBestN)
      // depending on:
      // a) less qualified participants then _reverseBestN => return only number of qualified participants
      // b) there are several participants at rank _reverseBestN
      int firstBestN = 0;
      TimeSpan? lastRuntime = null;
      foreach (var item in resultsCurGroup)
      {
        // Maximum 
        if (firstBestN >= _reverseBestN && (item.Runtime != lastRuntime && lastRuntime != null))
          break;

        if (item.Runtime == null || item.ResultCode != RunResult.EResultCode.Normal)
          break;

        lastRuntime = item.Runtime;
        firstBestN++;
      }

      // Pick best n starter in reverse order
      for (int i = firstBestN - 1; i >= 0; --i)
      {
        if (i >= resultsCurGroup.Count())
          continue;

        newStartList.Add(CreateStartListEntry(resultsCurGroup[i]));
      }

      // Separate remaining results and remember omitted results for appending to list
      List<RunResult> omittedResults = new List<RunResult>();
      for (int i = firstBestN; i < resultsCurGroup.Count(); ++i)
      {
        RunResult result = resultsCurGroup[i];
        if (result.Runtime == null || result.ResultCode != RunResult.EResultCode.Normal)
          omittedResults.Add(result);
        else
          newStartList.Add(CreateStartListEntry(result));
      }

      if (_allowNonResults)
      {
        // Add remaining starters with reverse startnumber order
        omittedResults.Sort(new SortByStartnumberDesc());
        foreach (RunResult result in omittedResults)
        {
          newStartList.Add(CreateStartListEntry(result));
        }
      }

      resultsCurGroup.Clear();
    }

    StartListEntry CreateStartListEntry(RunResult result)
    {
      return new StartListEntryAdditionalRun(result);
    }

  }




  /// <summary>
  /// Proxies a start list (see Init())
  /// If the starter already started, the flag Started of the StartListEntry is set to true.
  /// The view provided by GetView() is filtered to only contain entries with the flag Started set to false.
  /// </summary>
  public class RemainingStartListViewProvider : IStartListViewProvider
  {
    StartListViewProvider _srcStartListProvider;
    RaceRun _raceRun;

    ObservableCollection<StartListEntry> _viewList;
    CollectionViewSource _view;

    protected string _defaultGrouping;
    protected string _activeGrouping;


    public RemainingStartListViewProvider()
    {
      _view = new CollectionViewSource();
    }

    /// <summary>
    /// Initializes the view provider
    /// </summary>
    /// <param name="startListProvider">A StartListViewProvider that is the source to proxy.</param>
    /// <param name="raceRun">The corresonding race run to consider whether a specific starter already started.</param>
    public void Init(StartListViewProvider startListProvider, RaceRun raceRun)
    {
      // Remember the source
      _srcStartListProvider = startListProvider;
      SetDefaultGrouping(_srcStartListProvider.ActiveGrouping);

      _raceRun = raceRun;


      // Create working list
      _viewList = new CopyObservableCollection<StartListEntry, StartListEntry>(_srcStartListProvider.GetViewList(), sle => sle.ShallowCopy(), true);
      foreach (StartListEntry entry in _viewList)
        UpdateStartListEntry(entry);

      // Observe the results
      _raceRun.GetResultList().CollectionChanged += OnResultsChanged;
      _raceRun.GetResultList().ItemChanged += OnResultItemChanged;

      // Observe StartList 
      _viewList.CollectionChanged += OnStartListEntriesChanged;
      //_viewList.ItemChanged += OnStartListEntryItemChanged;

      // Observe additional properties
      _raceRun.PropertyChanged += OnRaceRun_PropertyChanged;

      // Create View with filtered items
      ObservableCollection<StartListEntry> startList = _viewList;
      _view.Source = startList;
      _view.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((StartListEntry)ea.Item).Started == false; });
      _view.LiveFilteringProperties.Add(nameof(StartListEntry.Started));
      _view.IsLiveFilteringRequested = true;
    }

    private void OnRaceRun_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "MarkedParticipantForStartMeasurement")
      {
        foreach (StartListEntry entry in _viewList)
          UpdateStartListEntry(entry);
      }
    }

    public void SetDefaultGrouping(string propertyName)
    {
      _srcStartListProvider.SetDefaultGrouping(propertyName);
      _defaultGrouping = propertyName;
      ResetToDefaultGrouping();
    }

    public void ChangeGrouping(string propertyName)
    {
      if (_activeGrouping == propertyName)
        return;

      _srcStartListProvider.ChangeGrouping(propertyName);

      if (!string.IsNullOrEmpty(_activeGrouping))
      {
        _view.GroupDescriptions.Clear();
        _view.LiveGroupingProperties.Clear();
        _view.IsLiveGroupingRequested = false;
      }

      if (!string.IsNullOrEmpty(propertyName))
      {
        GroupDescription gd = new PropertyGroupDescription(propertyName);
        gd.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        //gd.CustomSort = new DebugSort();
        _view.GroupDescriptions.Add(gd);
        _view.LiveGroupingProperties.Add(propertyName);
        _view.IsLiveGroupingRequested = true;
      }

      _activeGrouping = propertyName;
    }

    public void ResetToDefaultGrouping()
    {
      _srcStartListProvider.ChangeGrouping(_defaultGrouping);
      ChangeGrouping(_defaultGrouping);
    }

    public string ActiveGrouping { get { return _activeGrouping; } }


    // Output: List<StartListEntry> same way sorted as input StartList
    public ICollectionView GetView()
    {
      return _view.View;
    }

    public ObservableCollection<StartListEntry> GetViewList()
    {
      return _viewList;
    }


    #region implementation details

    private void OnResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RunResult result = (RunResult)item;
          SetStartListEntryStartedFalse(result); // Site note: UpdateStartListEntry(result) doesn't work, because result is actually not valid anymore

        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          // Remove from _results
          RunResult result = (RunResult)item;
          UpdateStartListEntry(result);
        }
    }

    private void OnResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult result = (RunResult)sender;
      UpdateStartListEntry(result);
    }

    private void OnStartListEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          StartListEntry sle = (StartListEntry)item;
          UpdateStartListEntry(sle);
        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          // Remove from _results
          StartListEntry sle = (StartListEntry)item;
          UpdateStartListEntry(sle);
        }
    }

    private void OnStartListEntryItemChanged(object sender, PropertyChangedEventArgs e)
    {
      StartListEntry sle = (StartListEntry)sender;
      UpdateStartListEntry(sle);
    }

    private void SetStartListEntryStartedFalse(RunResult result)
    {
      StartListEntry se = _viewList.Where(r => r.Participant == result.Participant).FirstOrDefault();
      if (se != null)
      {
        se.Started = false;
        se.MarkedForMeasurement = _raceRun.IsMarkedForStartMeasurement(se.Participant);
      }
    }

    private void UpdateStartListEntry(RunResult result)
    {
      StartListEntry se = _viewList.Where(r => r.Participant == result.Participant).FirstOrDefault();
      if (se != null)
      {
        se.Started = _raceRun.IsOrWasOnTrack(result);
        se.MarkedForMeasurement = _raceRun.IsMarkedForStartMeasurement(se.Participant);
      }
    }

    private void UpdateStartListEntry(StartListEntry se)
    {
      RunResult result = _raceRun.GetResultList().Where(r => r.Participant == se.Participant).FirstOrDefault();

      se.MarkedForMeasurement = _raceRun.IsMarkedForStartMeasurement(se.Participant);
      se.Started = result != null && _raceRun.IsOrWasOnTrack(result);
    }

    #endregion

  }



  public abstract class ResultViewProvider : ViewProvider
  {


  }


  /// <summary>
  /// BaseClass for RuntimeSorter and TotalTimeSorter
  ///
  /// Provides convenience methods for comparing by group.
  /// </summary>
  public abstract class ResultSorter<T> : IComparer<T>
  {
    string _groupingPropertyName;
    public void SetGrouping(string propertyName)
    {
      _groupingPropertyName = propertyName;
    }


    NullEnabledComparer nullEnabledComparer = new NullEnabledComparer();
    protected int CompareGroup(RunResult rrX, RunResult rrY)
    {
      int groupCompare = 0;
      if (_groupingPropertyName == "Participant.Class")
        groupCompare = nullEnabledComparer.Compare(rrX?.Participant?.Participant?.Class, rrY.Participant?.Participant?.Class);
      else if (_groupingPropertyName == "Participant.Group")
        groupCompare = nullEnabledComparer.Compare(rrX?.Participant?.Participant?.Group, rrY.Participant?.Participant?.Group);
      else if (_groupingPropertyName == "Participant.Sex")
        groupCompare = nullEnabledComparer.Compare(rrX?.Participant?.Participant?.Sex, rrY.Participant?.Participant?.Sex);

      return groupCompare;
    }
    protected int CompareGroup(RaceResultItem rrX, RaceResultItem rrY)
    {
      int groupCompare = 0;
      if (_groupingPropertyName == "Participant.Class")
        groupCompare = nullEnabledComparer.Compare(rrX?.Participant?.Participant?.Class, rrY?.Participant?.Participant?.Class);
      else if (_groupingPropertyName == "Participant.Group")
        groupCompare = nullEnabledComparer.Compare(rrX?.Participant?.Participant?.Group, rrY?.Participant?.Participant?.Group);
      else if (_groupingPropertyName == "Participant.Sex")
        groupCompare = nullEnabledComparer.Compare(rrX?.Participant?.Participant?.Sex, rrY?.Participant?.Participant?.Sex);

      return groupCompare;
    }

    public abstract int Compare(T rrX, T rrY);

  }


  /// <summary>
  /// Compares two RunResults, taking into account:
  /// - Group (Class, Group, Category)
  /// - Runtime
  /// - ResultCode
  /// - StartNumber
  /// </summary>
  public class RuntimeSorter : ResultSorter<RunResult>
  {
    bool _startNumberAscending;

    public RuntimeSorter(bool startNumberAscending = true)
    {
      _startNumberAscending = startNumberAscending;
    }

    public override int Compare(RunResult rrX, RunResult rrY)
    {
      TimeSpan? tX = null, tY = null;

      if (rrX.ResultCode == RunResult.EResultCode.Normal)
        tX = rrX.Runtime;

      if (rrY.ResultCode == RunResult.EResultCode.Normal)
        tY = rrY.Runtime;

      int groupCompare = CompareGroup(rrX, rrY);
      if (groupCompare != 0)
        return groupCompare;

      // Sort by time
      if (tX != null && tY == null)
        return -1;

      if (tX == null && tY != null)
        return 1;

      // If no time, use startnumber
      if (tX == null && tY == null)
        return rrX.Participant.StartNumber.CompareTo(rrY.Participant.StartNumber);

      // Main comparison: based on time
      int timeComp = TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
      // If equal, consider startnumber as well
      if (timeComp == 0)
        return (_startNumberAscending ? 1 : -1) * rrX.Participant.StartNumber.CompareTo(rrY.Participant.StartNumber);

      return timeComp;
    }
  }


  public class RaceRunResultViewProvider : ResultViewProvider
  {
    // Input Data
    protected RaceRun _raceRun;
    protected ItemsChangeObservableCollection<RaceParticipant> _participants;
    protected ItemsChangeObservableCollection<RunResult> _originalResults;
    protected AppDataModel _appDataModel;

    // Working Data
    protected ItemsChangeObservableCollection<RunResultWithPosition> _viewList;
    protected ResultSorter<RunResult> _comparer;


    public RaceRun RaceRun { get { return _raceRun; } }

    public RaceRunResultViewProvider()
    {
      _comparer = new RuntimeSorter();
    }


    public override ViewProvider Clone()
    {
      return new RaceRunResultViewProvider();
    }


    // Input: RaceRun
    public virtual void Init(RaceRun raceRun, AppDataModel appDataModel)
    {
      _raceRun = raceRun;
      _originalResults = raceRun.GetResultList();
      _appDataModel = appDataModel;

      _viewList = new ItemsChangeObservableCollection<RunResultWithPosition>();

      // Initialize and observe source list
      _participants = raceRun.GetRace().GetParticipants();
      PopulateInitially<RunResultWithPosition, RaceParticipant>(_viewList, _participants, _comparer, CreateRunResultWithPosition);

      // Trigger Updates on Participant Changes
      _participants.CollectionChanged += Participants_CollectionChanged;
      _participants.ItemChanged += Participants_ItemChanged;
      // Trigger Updates in RunResult changes
      _originalResults.CollectionChanged += OnOriginalResultsChanged;
      _originalResults.ItemChanged += OnOriginalResultItemChanged;

      updatePositions();
      FinalizeInit();
    }


    // Output: List<RunResultWithPosition>
    protected override object GetViewSource()
    {
      return _viewList;
    }

    public ItemsChangeObservableCollection<RunResultWithPosition> GetViewList()
    {
      return _viewList;
    }



    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);

      if (_viewList == null)
        return;

      _viewList.Sort(_comparer);
      updatePositions();
    }


    protected virtual RunResultWithPosition CreateRunResultWithPosition(RunResult r)
    {
      return new RunResultWithPosition(r);
    }


    protected virtual RunResultWithPosition CreateRunResultWithPosition(RaceParticipant r)
    {
      // Find RunResult
      var rr = _raceRun.GetRunResult(r);
      if (rr != null)
      {
        return CreateRunResultWithPosition(rr);
      }
      else
      {
        // Create empty run result
        return new RunResultWithPosition(r);
      }
    }


    private void Participants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RaceParticipant rp = (RaceParticipant)item;
          var itemsToRemove = _viewList.Where(r => r.Participant == rp).ToList();
          foreach (var itemToRemove in itemsToRemove)
            _viewList.Remove(itemToRemove);
        }
        updatePositions();
      }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          RaceParticipant rp = (RaceParticipant)item;
          updateRunResult(rp, _raceRun.GetRunResult(rp));
        }
    }

    private void Participants_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RaceParticipant rp = (RaceParticipant)sender;
      RunResultWithPosition rrWP = _viewList.FirstOrDefault(r => r.Participant == rp);

      updateRunResult(rp, _raceRun.GetRunResult(rp));
    }


    void OnOriginalResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          RunResult rr = (RunResult)item;
          updateRunResult(rr.Participant, null); // Delete run result
        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          RunResult rr = (RunResult)item;
          updateRunResult(rr.Participant, rr);
        }
    }


    void OnOriginalResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult rr = (RunResult)sender;
      updateRunResult(rr.Participant, rr);
    }


    void updateRunResult(RaceParticipant rp, RunResult rr)
    {
      RunResultWithPosition rrWP = _viewList.FirstOrDefault(r => r.Participant == rp);
      if (rrWP != null)
      {
        rrWP.UpdateRunResult(rr);
        _viewList.Sort(_comparer);
      }
      else
      {
        if (rr != null)
          _viewList.InsertSorted(CreateRunResultWithPosition(rr), _comparer);
        else
          _viewList.InsertSorted(CreateRunResultWithPosition(rp), _comparer);
      }

      updatePositions();
    }


    protected virtual void updatePositions()
    {
      uint curPosition = 0;
      uint samePosition = 1;
      object curGroup = null;
      TimeSpan? lastTime = null;
      TimeSpan? firstTime = null;
      foreach (RunResultWithPosition item in _viewList)
      {
        // New group
        if (!Equals(PropertyUtilities.GetPropertyValue(item, _activeGrouping), curGroup))
        {
          curGroup = PropertyUtilities.GetPropertyValue(item, _activeGrouping);
          curPosition = 0;
          samePosition = 1;
          firstTime = lastTime = null;
        }

        if (item.Runtime != null)
        {
          // Same position in case same time
          if (item.Runtime == lastTime)//< TimeSpan.FromMilliseconds(9))
            samePosition++;
          else
          {
            curPosition += samePosition;
            samePosition = 1;
          }


          if (firstTime == null)
          {
            System.Diagnostics.Debug.Assert(curPosition == 1);
            firstTime = item.Runtime;
            item.DiffToFirst = null;
            item.DiffToFirstPercentage = 0;
          }
          else
          {
            item.DiffToFirst = item.Runtime - firstTime;
          }

          if (item.DiffToFirst != null)
            item.DiffToFirstPercentage = ((double)((TimeSpan)item.DiffToFirst).TotalMilliseconds) / (double)((TimeSpan)firstTime).TotalMilliseconds * 100.0;

          item.Position = curPosition;
          lastTime = item.Runtime;
        }
        else
        {
          item.Position = 0;
          item.DiffToFirst = null;
          item.DiffToFirstPercentage = 0.0;
        }

        // Set the JustModified flag to highlight new results
        item.JustModified = _appDataModel.JustMeasured(item.Participant.Participant);
      }
    }
  }


  internal class PenaltyRunResultWithPosition : RunResultWithPosition
  {
    protected TimeSpan? _cutOffTime;
    public PenaltyRunResultWithPosition(RunResult result)
      : base(result)
    {
    }
    public PenaltyRunResultWithPosition(RaceParticipant rp)
      : base(rp)
    {
    }

    public virtual void SetCutOffTime(TimeSpan? time)
    {
      if (_cutOffTime != time)
      {
        _cutOffTime = time;
        if (base.GetRunTime() != GetRunTime())
        {
          NotifyPropertyChanged(propertyName: nameof(Runtime));
          NotifyPropertyChanged(propertyName: nameof(ResultCode));
          NotifyPropertyChanged(propertyName: nameof(DisqualText));
        }
      }
    }

    protected bool applyPenaltyByResultCode()
    {
      var rc = base.ResultCode;
      return rc == EResultCode.NQ || rc == EResultCode.NiZ || rc == EResultCode.DIS;
    }
    protected bool applyPenaltyByTime()
    {
      return _cutOffTime != null && _cutOffTime < base.GetRunTime();
    }

    public virtual TimeSpan? OrgRuntime { get { return base.GetRunTime(); } }

    /** Override to return the cut off time or the original time */
    public override TimeSpan? GetRunTime(bool calculateIfNotStored = true, bool considerResultCode = true)
    {
      TimeSpan? orgTime = base.GetRunTime(calculateIfNotStored, false);
      if (applyPenaltyByTime() || applyPenaltyByResultCode())
        return _cutOffTime;
      return orgTime;
    }

    override public EResultCode ResultCode { 
      get {
        if (applyPenaltyByTime() || applyPenaltyByResultCode())
          return EResultCode.Normal;
        return _resultCode; 
      } 
    }
  }


  public class PenaltyRaceRunResultViewProvider : RaceRunResultViewProvider
  {
    protected double _cutOffPercentage;
    protected TimeSpan _cutOffTime;

    public PenaltyRaceRunResultViewProvider(double cutOffPercentage) 
      : base()
    {
      _cutOffPercentage = cutOffPercentage;
    }


    protected override RunResultWithPosition CreateRunResultWithPosition(RunResult r)
    {
      return new PenaltyRunResultWithPosition(r);
    }
    protected override RunResultWithPosition CreateRunResultWithPosition(RaceParticipant r)
    {
      // Find RunResult
      var rr = _raceRun.GetRunResult(r);
      if (rr != null)
      {
        return CreateRunResultWithPosition(rr);
      }
      else
      {
        // Create empty run result
        return new PenaltyRunResultWithPosition(r);
      }
    }

    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);

      if (_viewList == null)
        return;

      updateCutOffTime();
      _viewList.Sort(_comparer);
      updatePositions();
    }


    protected void updateCutOffTime()
    {
      var groups = _viewList
        .GroupBy(item => PropertyUtilities.GetPropertyValue(item, _activeGrouping))
        .Select(g1 => new
        {
          g1.Key,
          BestTime = g1.Min(item => (item as PenaltyRunResultWithPosition).OrgRuntime)
        })
        .Select(g2 => new
        {
          g2.Key,
          g2.BestTime,
          CutOffTime = g2.BestTime == null ? (TimeSpan?)null : TimeSpan.FromMilliseconds(((TimeSpan)g2.BestTime).TotalMilliseconds * (1.00 + _cutOffPercentage/100.0))
        });

      foreach (PenaltyRunResultWithPosition item in _viewList)
        item.SetCutOffTime(groups.Where(g => g.Key == PropertyUtilities.GetPropertyValue(item, _activeGrouping)).First().CutOffTime);
    }

    protected override void updatePositions()
    {
      updateCutOffTime();
      base.updatePositions();
    }

  }



  public class TotalTimeSorter : ResultSorter<RaceResultItem>
  {
    public override int Compare(RaceResultItem rrX, RaceResultItem rrY)
    {
      int groupCompare = CompareGroup(rrX, rrY);
      if (groupCompare != 0)
        return groupCompare;

      TimeSpan? tX = rrX.TotalTime;
      TimeSpan? tY = rrY.TotalTime;

      // Sort by time
      if (tX != null && tY == null)
        return -1;

      if (tX == null && tY != null)
        return 1;

      // If no time, use startnumber
      if (tX == null && tY == null)
        return rrX.Participant.StartNumber.CompareTo(rrY.Participant.StartNumber);

      // If equal, consider startnumber as well
      int timeComp = TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
      if (timeComp == 0)
        return rrX.Participant.StartNumber.CompareTo(rrY.Participant.StartNumber);

      return timeComp;
    }
  }


  public class RaceResultViewProvider : ResultViewProvider
  {
    protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    delegate TimeSpan? RunResultCombiner(Dictionary<uint, RunResultWithPosition> results, out RunResult.EResultCode code, out string disqualText);

    TimeCombination _timeCombination;
    // Input Data
    protected Race _race;
    protected RaceRun[] _raceRuns;
    protected AppDataModel _appDataModel;

    // Working Data
    protected ItemsChangeObservableCollection<RaceResultItem> _viewList;
    protected ResultSorter<RaceResultItem> _comparer;
    RunResultCombiner _combineTime;
    protected List<RaceRun> _lastConsideredRuns;
    //protected List<ItemsChangedNotifier> _runResultsNotifier;


    protected virtual double calculatePoints(RaceResultItem rri)
    {
      return -1.0;
    }

    public enum TimeCombination { BestRun, Sum, SumBest2 };
    public RaceResultViewProvider(TimeCombination timeCombination)
    {
      _comparer = new TotalTimeSorter();

      _timeCombination = timeCombination;
      switch(_timeCombination)
      {
        case TimeCombination.BestRun:
          _combineTime = MinimumTime;
          break;
        case TimeCombination.Sum:
          _combineTime = SumTime;
          break;
        case TimeCombination.SumBest2:
          _combineTime = SumTimeOfBest2;
          break;
      }

      _lastConsideredRuns = new List<RaceRun>();
      //_runResultsNotifier = new List<ItemsChangedNotifier>();
    }


    public override ViewProvider Clone()
    {
      return new RaceResultViewProvider(_timeCombination);
    }

    
    // Input: Race
    public virtual void Init(Race race, AppDataModel appDataModel)
    {
      _race = race;
      _raceRuns = _race.GetRuns();
      _appDataModel = appDataModel;

      _viewList = new ItemsChangeObservableCollection<RaceResultItem>();

      foreach (RaceRun r in _raceRuns)
      {
        RaceRunResultViewProvider rrVP = (r.GetResultViewProvider() as RaceRunResultViewProvider);
        // Watch for changes
        rrVP.GetViewList().CollectionChanged += OnResultListCollectionChanged;
        // Initial update
        OnResultListCollectionChanged(rrVP.GetViewList(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, rrVP.GetViewList().ToList()));
      }

      UpdateAll();

      FinalizeInit();
    }


    // Output: sorted List<StartListEntry> according to StartNumber
    public ItemsChangeObservableCollection<RaceResultItem> GetViewList()
    {
      return _viewList;
    }


    // Output: List<RunResultWithPosition>
    protected override object GetViewSource()
    {
      return _viewList;
    }

    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);
      ResortResults();
    }

    private void OnRunResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResultWithPosition rr = sender as RunResultWithPosition;

      if (rr != null)
      {
        if (UpdateResultsFor(rr.Participant))
          ResortResults();
      }
    }

    private void OnResultListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      bool bSomethingChanged = false;

      if (e.OldItems != null)
      {
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          item.PropertyChanged -= OnRunResultItemChanged;
          RunResultWithPosition rr = item as RunResultWithPosition;
          if (UpdateResultsFor(rr?.Participant))
            bSomethingChanged = true;
        }
      }

      if (e.NewItems != null)
      {
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          item.PropertyChanged += OnRunResultItemChanged;
          RunResultWithPosition rr = item as RunResultWithPosition;
          if (UpdateResultsFor(rr?.Participant))
            bSomethingChanged = true;
        }
      }

      if (bSomethingChanged)
        ResortResults();
    }


    private void OnResultListItemChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is RunResultWithPosition rr)
        if (UpdateResultsFor(rr?.Participant))
          ResortResults();
    }


    private void CheckRunConsideration()
    {
      bool updateAll = false;

      foreach (RaceRun run in _raceRuns)
      {
        if (run.HasResults())
        {
          if (!_lastConsideredRuns.Contains(run))
          {
            updateAll = true;
            _lastConsideredRuns.Add(run);
          }
        }
        else
        {
          if (_lastConsideredRuns.Contains(run))
          {
            updateAll = true;
            _lastConsideredRuns.Remove(run);
          }

        }
      }

      if (updateAll)
        UpdateAll();
    }

    private void UpdateAll()
    {
      bool resortNeeded = false;

      foreach (RaceParticipant p in _race.GetParticipants())
        if (UpdateResultsFor(p))
          resortNeeded = true;

      if (resortNeeded)
        ResortResults();
    }

    private bool UpdateResultsFor(RaceParticipant participant)
    {
      CheckRunConsideration();

      bool significantChange = false;

      // Look for the sub-result
      Dictionary<uint, RunResultWithPosition> results = new Dictionary<uint, RunResultWithPosition>();
      foreach (RaceRun run in _lastConsideredRuns)
      {
        RaceRunResultViewProvider rrVP = (run.GetResultViewProvider() as RaceRunResultViewProvider);
        RunResultWithPosition result = rrVP.GetViewList().SingleOrDefault(x => x.Participant == participant);
        results.Add(run.Run, result);
      }
      var includeParticipant = _race.GetParticipants().SingleOrDefault(x => x == participant) != null;

      RaceResultItem rri = _viewList.SingleOrDefault(x => x.Participant == participant);
      if (rri == null && includeParticipant)
      { // Add Entry
        rri = new RaceResultItem(participant);
        _viewList.Add(rri);
        significantChange = true;
      } 
      else if (rri != null && !includeParticipant)
      { 
        // Remove Entry
        _viewList.Remove(rri);
        significantChange = true;
      }

      if (rri == null)
        return significantChange;

      // Combine and update the race result
      foreach (var res in results)
      {
        bool sigCh = rri.SetRunResult(res.Key, res.Value);
        //significantChange = significantChange || sigCh;
      }

      // It may happen, that caused by _lastConsideredRuns a race gets removed resulting in not-updated SubResults
      // In this casethe corresponding SubResult has to be removed 
      List<uint> toDelete = new List<uint>();
      foreach(var t in rri.SubResults) if (!results.ContainsKey(t.Key)) toDelete.Add(t.Key);
      foreach(var k in toDelete) rri.SubResults.Remove(k);

      
      RunResult.EResultCode code;
      string disqualText;

      TimeSpan? oldTime = rri.TotalTime;
      rri.TotalTime = _combineTime(results, out code, out disqualText);
      if (oldTime != rri.TotalTime)
        significantChange = true;

      if (rri.ResultCode != code)
      {
        rri.ResultCode = code;
        significantChange = true;
      }

      rri.DisqualText = disqualText;

      System.Diagnostics.Debug.Assert(
        (!significantChange && rri.TotalTime == oldTime) || significantChange,
        "no significant change but time did change!!!"
        );

      if (significantChange && rri.TotalTime == oldTime)
        Logger.Debug("Significant change although time did not change: {0}", rri.TotalTime);

      return significantChange;
    }


    protected virtual void ResortResults()
    {
      Logger.Debug(System.Reflection.MethodBase.GetCurrentMethod());

      if (_viewList == null)
        return;

      _viewList.Sort(_comparer);

      uint curPosition = 0;
      uint samePosition = 1;

      object curGroup = null;

      TimeSpan? lastTime = null;
      TimeSpan? firstTime = null;
      foreach (var sortedItem in _viewList)
      {
        // New group
        if (!Equals(PropertyUtilities.GetPropertyValue(sortedItem, _activeGrouping), curGroup))
        {
          curGroup = PropertyUtilities.GetPropertyValue(sortedItem, _activeGrouping);
          curPosition = 0;
          samePosition = 1;
          firstTime = lastTime = null;
        }

        if (sortedItem.TotalTime != null)
        {
          // Same position in case same time
          if (sortedItem.TotalTime == lastTime)//< TimeSpan.FromMilliseconds(9))
          {
            samePosition++;
          }
          else
          {
            curPosition += samePosition;
            samePosition = 1;
          }

          if (firstTime == null)
          {
            System.Diagnostics.Debug.Assert(curPosition == 1);
            firstTime = sortedItem.TotalTime;
            sortedItem.DiffToFirst = null;
            sortedItem.DiffToFirstPercentage = 0;
          }
          else
            sortedItem.DiffToFirst = sortedItem.TotalTime - firstTime;

          if (sortedItem.DiffToFirst != null)
            sortedItem.DiffToFirstPercentage = ((double)((TimeSpan)sortedItem.DiffToFirst).TotalMilliseconds) / (double)((TimeSpan)firstTime).TotalMilliseconds * 100.0;

          sortedItem.Position = curPosition;

          sortedItem.Points = calculatePoints(sortedItem);

          lastTime = sortedItem.TotalTime;
        }
        else
        {
          sortedItem.Position = 0;
          sortedItem.DiffToFirst = null;
          sortedItem.DiffToFirstPercentage = 0.0;
        }

        // Set the JustModified flag to highlight new results
        sortedItem.JustModified = _appDataModel.JustMeasured(sortedItem.Participant.Participant);
      }
    }


    internal static TimeSpan? MinimumTime(Dictionary<uint, RunResultWithPosition> results, out RunResult.EResultCode resCode, out string disqualText)
    {
      TimeSpan? minTime = null;
      RunResult.EResultCode bestCode = RunResult.EResultCode.NQ;
      resCode = RunResult.EResultCode.Normal;
      disqualText = "";

      foreach (var res in results)
      {
        if (res.Value == null)
          continue;

        if (res.Value.Runtime != null)
        {
          if (minTime == null || TimeSpan.Compare((TimeSpan)res.Value.Runtime, (TimeSpan)minTime) < 0)
          {
            minTime = res.Value.Runtime;
            bestCode = res.Value.ResultCode;
          }
        }
        if (res.Value.ResultCode != RunResult.EResultCode.Normal)
        {
          resCode = res.Value.ResultCode;
          if (!string.IsNullOrEmpty(disqualText))
            disqualText += ", ";
          disqualText += res.Value.DisqualText;
        }
      }

      // Clear if a result is available
      if ( bestCode == RunResult.EResultCode.Normal)
      {
        resCode = RunResult.EResultCode.Normal;
        disqualText = "";
      }

      return minTime;
    }

    internal static TimeSpan? SumTime(Dictionary<uint, RunResultWithPosition> results, out RunResult.EResultCode resCode, out string disqualText)
    {
      TimeSpan? sumTime = new TimeSpan(0);
      resCode = RunResult.EResultCode.Normal;
      disqualText = "";

      foreach (var res in results)
      {
        if (res.Value == null)
        {
          sumTime = null;
          continue;
        }

        if (res.Value?.Runtime != null)
          sumTime += (TimeSpan)res.Value.Runtime;
        else
          // no time ==> Invalid
          sumTime = null;

        if (res.Value.ResultCode != RunResult.EResultCode.Normal)
        {
          if (resCode == RunResult.EResultCode.Normal || resCode == RunResult.EResultCode.NotSet)
          {
            resCode = res.Value.ResultCode;
            disqualText = res.Value.DisqualText;
          }
        }
      }

      if (results.Count == 0)
        sumTime = null;

      return sumTime;
    }

    internal static TimeSpan? SumTimeOfBest2(Dictionary<uint, RunResultWithPosition> results, out RunResult.EResultCode resCode, out string disqualText)
    {
      int numberN = 2;

      TimeSpan? sumTime = new TimeSpan(0);
      resCode = RunResult.EResultCode.Normal;
      disqualText = "";

      // Find best N
      Dictionary<uint, RunResultWithPosition> bestNIn = new Dictionary<uint, RunResultWithPosition>();
      Dictionary<uint, RunResultWithPosition> bestN = new Dictionary<uint, RunResultWithPosition>();
      foreach (var res in results)
        bestNIn.Add(res.Key, res.Value);

      for (int i = 0; i < numberN; i++)
      {
        uint bestKey = 0;
        TimeSpan? bestTime = null;
        foreach (var res in bestNIn)
        {
          TimeSpan? time = res.Value?.Runtime;

          if (bestTime == null || bestTime > time)
          {
            bestTime = time;
            bestKey = res.Key;
          }
        }

        if (bestNIn.ContainsKey(bestKey))
        {
          bestN.Add(bestKey, bestNIn[bestKey]);
          bestNIn.Remove(bestKey);
        }
      }

      return SumTime(bestN, out resCode, out disqualText);
    }


  }


  public class DSVSchoolRaceResultViewProvider : RaceResultViewProvider
  {
    protected DSVRaceCalculation _dsvCalcM;
    protected DSVRaceCalculation _dsvCalcW;

    public DSVSchoolRaceResultViewProvider() : base(RaceResultViewProvider.TimeCombination.Sum)
    { }

    public override ViewProvider Clone()
    {
      return new DSVSchoolRaceResultViewProvider();
    }

    public DSVRaceCalculation GetDSVRaceCalculationMen()
    {
      return _dsvCalcM;
    }

    public DSVRaceCalculation GetDSVRaceCalculationWomen()
    {
      return _dsvCalcW;
    }

    public override void Init(Race race, AppDataModel appDataModel)
    {
      _dsvCalcM = new DSVRaceCalculation(race, this, 'M');
      _dsvCalcW = new DSVRaceCalculation(race, this, 'W');

      base.Init(race, appDataModel);
    }


    protected override void ResortResults()
    {
      if (_viewList == null)
        return;

      base.ResortResults();

      try
      {
        _dsvCalcM.CalculatePenalty();
        _dsvCalcW.CalculatePenalty();
      }
      catch (Exception) { }

      // Re-Update points
      foreach (var sortedItem in _viewList)
      {
        sortedItem.Points = calculatePoints(sortedItem);
      }
    }

    protected override double calculatePoints(RaceResultItem rri)
    { 
      if (rri.Participant.Sex?.Name == 'M')
        return _dsvCalcM.CalculatePoints(rri, true);
      if (rri.Participant.Sex?.Name == 'W')
        return _dsvCalcW.CalculatePoints(rri, true);

      return -1.0;
    }
  }




  public class PointsViaTableRaceResultViewProvider : RaceResultViewProvider
  {

    Dictionary<uint, double> _pointsTable;

    public PointsViaTableRaceResultViewProvider() : base(RaceResultViewProvider.TimeCombination.Sum)
    {
      // Default Points Table
      _pointsTable = new Dictionary<uint, double>
      {
        { 1, 15 },
        { 2, 12 },
        { 3, 10 },
        { 4,  8 },
        { 5,  6 },
        { 6,  5 },
        { 7,  4 },
        { 8,  3 },
        { 9,  2 },
        {10,  1 }
      };

    }

    public override ViewProvider Clone()
    {
      return new PointsViaTableRaceResultViewProvider();
    }

    public override void Init(Race race, AppDataModel appDataModel)
    {
      try
      {
        string filePath = System.IO.Path.Combine(appDataModel.GetDB().GetDBPathDirectory(), "PointsTable.txt");
        _pointsTable = readPointsTable(filePath);
      }
      catch (Exception)
      { }

      base.Init(race, appDataModel);
    }

    Dictionary<uint, double> readPointsTable(string filename)
    {
      Dictionary<uint, double> pointsTable = new Dictionary<uint, double>();

      uint startNumber = 1;
      using (TextReader reader = File.OpenText(filename))
      {
        string line = null;
        while ((line = reader.ReadLine()) != null) 
        { 
          double points = double.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
          pointsTable.Add(startNumber, points);
          startNumber++;
        }
      }
      return pointsTable;
    }


    protected override void ResortResults()
    {
      if (_viewList == null)
        return;

      base.ResortResults();

      // Re-Update points
      foreach (var sortedItem in _viewList)
      {
        sortedItem.Points = calculatePoints(sortedItem);
      }
    }

    protected override double calculatePoints(RaceResultItem rri)
    {
      double points = 0.0;
      _pointsTable.TryGetValue(rri.Position, out points);
      return points;
    }
  }


  /* e.g. FamilienWertung
    public class SpecialRaceResultViewProvider : ResultViewProvider
    {
      // Input: Race

      // Output: List<RunResultWithPosition>


    }
    */






}
