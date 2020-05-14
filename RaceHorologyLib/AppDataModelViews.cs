/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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

    public string ActiveGrouping { get { return _activeGrouping;  } }


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
    public enum  Direction : int { Ascending = 1, Descending = -1 };
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
        return 1  * (int)_direction;
      else
        return 0;
    }



    string _groupingPropertyName;
    public void SetGrouping(string propertyName)
    {
      _groupingPropertyName = propertyName;
    }

    protected int CompareGroup(StartListEntry rrX, StartListEntry rrY)
    {
      int groupCompare = 0;
      if (_groupingPropertyName == "Participant.Class")
        groupCompare = rrX.Participant.Participant.Class.CompareTo(rrY.Participant.Participant.Class);
      else if (_groupingPropertyName == "Participant.Group")
        groupCompare = rrX.Participant.Participant.Group.CompareTo(rrY.Participant.Participant.Group);
      else if (_groupingPropertyName == "Participant.Sex")
        groupCompare = rrX.Participant.Participant.Sex.CompareTo(rrY.Participant.Participant.Sex);

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


  public class FirstRunStartListViewProvider :  StartListViewProvider
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

      // Ensure list is sorted again
      if (_viewList != null)
        _viewList.Sort(_comparer);
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
        foreach (INotifyPropertyChanged item in e.NewItems)
          _viewList.InsertSorted(CreateStartListEntry((RaceParticipant)item), _comparer);
    }


    private void _sourceItemChangedNotifier_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      // Ensure list is sorted again
      _viewList.Sort(_comparer);
    }

  }



  public class PointsStartListEntryComparer : StartListEntryComparer
  {
    protected int _firstNStartnumbers;
    public PointsStartListEntryComparer(int firstNStartnumbers)
    {
      _firstNStartnumbers = firstNStartnumbers;
    }

    public override int Compare(StartListEntry left, StartListEntry right)
    {
      if (left.StartNumber < _firstNStartnumbers + 1)
      {
        if (right.StartNumber < _firstNStartnumbers + 1)
          return left.StartNumber.CompareTo(right.StartNumber);
        else
          return -1;
      }

      // Left Startnumber is bigger than _firstNStartnumbers

      if (right.StartNumber < _firstNStartnumbers + 1)
        return +1;

      // According to points, but other direction
      int compPoints = left.Points.CompareTo(right.Points);
      if (compPoints != 0)
        return compPoints;

      // If Points are equal, sort by startnumber
      return left.StartNumber.CompareTo(right.StartNumber);
    }
  }



  // First n (15) per grouping are always kept constant
  public class DSVFirstRunStartListViewProvider : FirstRunStartListViewProvider
  {
    int _firstNStartnumbers;

    public DSVFirstRunStartListViewProvider(int firstNStartnumbers)
    {
      _firstNStartnumbers = firstNStartnumbers;
      _comparer = new PointsStartListEntryComparer(firstNStartnumbers);
    }

    public override ViewProvider Clone()
    {
      return new DSVFirstRunStartListViewProvider(_firstNStartnumbers);
    }
  }




  public abstract class SecondRunStartListViewProvider : StartListViewProvider
  {
    // Input: List<StartListEntry> (1st run),
    //        List<RaceResultWithPosition> (1st run)

    public abstract void Init(RaceRun previousRun);

    // Output: sorted List<StartListEntry> according to StartNumber

  }


  // wie 1. DG, 1. DG rückwärts
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


    //RaceRunResultViewProvider _resultVPPreviousRun;
    ItemsChangeObservableCollection<RunResult> _resultsPreviousRun;

    public BasedOnResultsFirstRunStartListViewProvider(int reverseBestN, bool allowNonResults)
    {
      _reverseBestN = reverseBestN;
      _allowNonResults = allowNonResults;

      _resultsComparer = new RuntimeSorter();
    }


    public override ViewProvider Clone()
    {
      return new BasedOnResultsFirstRunStartListViewProvider(_reverseBestN, _allowNonResults);
    }


    public override void Init(RaceRun previousRun)
    {
      _viewList = new ObservableCollection<StartListEntry>();

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
      // Find how many valid results are there (could be less than _reverseBestN)
      int firstBestN = 0;
      foreach( var item in resultsCurGroup)
      {
        if (firstBestN >= _reverseBestN)
          break;

        if (item.Runtime == null || item.ResultCode != RunResult.EResultCode.Normal)
          break;
          
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



    // Input: StartListViewProvider or List<StartListEntry>
    public void Init(StartListViewProvider startListProvider, RaceRun raceRun)
    {
      // Remember the source
      _srcStartListProvider = startListProvider;
      _raceRun = raceRun;


      // Create working list
      _viewList = new CopyObservableCollection<StartListEntry, StartListEntry>(_srcStartListProvider.GetViewList(), sle => sle.ShallowCopy());
      foreach (StartListEntry entry in _viewList)
        UpdateStartListEntry(entry);

      // Observe the results
      _raceRun.GetResultList().CollectionChanged += OnResultsChanged;
      _raceRun.GetResultList().ItemChanged += OnResultItemChanged;

      // Observe StartList 
      _viewList.CollectionChanged += OnStartListEntriesChanged;
      //_viewList.ItemChanged += OnStartListEntryItemChanged;

      // Create View with filtered items
      ObservableCollection<StartListEntry> startList = _viewList;
      _view.Source = startList;
      _view.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((StartListEntry)ea.Item).Started == false; });
      _view.LiveFilteringProperties.Add(nameof(StartListEntry.Started));
      _view.IsLiveFilteringRequested = true;
    }


    // Output: List<StartListEntry> same way sorted as input StartList
    public ICollectionView GetView()
    {
      return _view.View;
    }


    private void OnResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RunResult result = (RunResult)item;
          UpdateStartListEntry(result);
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

    private void UpdateStartListEntry(RunResult result)
    {
      StartListEntry se = _viewList.Where(r => r.Participant == result.Participant).FirstOrDefault();
      if (se != null)
        se.Started = _raceRun.IsOrWasOnTrack(result);
    }

    private void UpdateStartListEntry(StartListEntry se)
    {
      RunResult result = _raceRun.GetResultList().Where(r => r.Participant == se.Participant).FirstOrDefault();

      if (result != null)
        se.Started = _raceRun.IsOrWasOnTrack(result);
    }



  }



  public abstract class ResultViewProvider : ViewProvider
  {


  }



  public abstract class ResultSorter<T> : IComparer<T>
  {
    string _groupingPropertyName;
    public void SetGrouping(string propertyName)
    {
      _groupingPropertyName = propertyName;
    }

    protected int CompareGroup(RunResult rrX, RunResult rrY)
    {
      int groupCompare = 0;
      if (_groupingPropertyName == "Participant.Class")
        groupCompare = rrX.Participant.Participant.Class.CompareTo(rrY.Participant.Participant.Class);
      else if (_groupingPropertyName == "Participant.Group")
        groupCompare = rrX.Participant.Participant.Group.CompareTo(rrY.Participant.Participant.Group);
      else if (_groupingPropertyName == "Participant.Sex")
        groupCompare = rrX.Participant.Participant.Sex.CompareTo(rrY.Participant.Participant.Sex);

      return groupCompare;
    }
    protected int CompareGroup(RaceResultItem rrX, RaceResultItem rrY)
    {
      int groupCompare = 0;
      if (_groupingPropertyName == "Participant.Class")
        groupCompare = rrX.Participant.Participant.Class.CompareTo(rrY.Participant.Participant.Class);
      else if (_groupingPropertyName == "Participant.Group")
        groupCompare = rrX.Participant.Participant.Group.CompareTo(rrY.Participant.Participant.Group);
      else if (_groupingPropertyName == "Participant.Sex")
        groupCompare = rrX.Participant.Participant.Sex.CompareTo(rrY.Participant.Participant.Sex);

      return groupCompare;
    }

    public abstract int Compare(T rrX, T rrY);

  }


  //Propagate class to sorter
  public class RuntimeSorter : ResultSorter<RunResult>
  {

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

      // If equal, consider startnumber as well
      int timeComp = TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
      if (timeComp == 0)
        return rrX.Participant.StartNumber.CompareTo(rrY.Participant.StartNumber);

      return timeComp;
    }
  }


  public class RaceRunResultViewProvider : ResultViewProvider
  {
    // Input Data
    ItemsChangeObservableCollection<RunResult> _originalResults;
    AppDataModel _appDataModel;

    // Working Data
    ItemsChangeObservableCollection<RunResultWithPosition> _viewList;
    ResultSorter<RunResult> _comparer;


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
      _originalResults = raceRun.GetResultList();
      _appDataModel = appDataModel;

      _viewList = new ItemsChangeObservableCollection<RunResultWithPosition>();

      // Initialize and observe source list
      PopulateInitially<RunResultWithPosition, RunResult>(_viewList, _originalResults, _comparer, CreateRunResultWithPosition);
      UpdatePositions();
      _originalResults.CollectionChanged += OnOriginalResultsChanged;
      _originalResults.ItemChanged += OnOriginalResultItemChanged;

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
      UpdatePositions();
    }


    private static RunResultWithPosition CreateRunResultWithPosition(RunResult r)
    {
      return new RunResultWithPosition(r);
    }


    void OnOriginalResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RunResult result = (RunResult)item;
          var itemsToRemove = _viewList.Where(r => r == result).ToList();
          foreach (var itemToRemove in itemsToRemove)
            _viewList.Remove(itemToRemove);
        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          _viewList.InsertSorted(CreateRunResultWithPosition((RunResult)item), _comparer);
        }

      UpdatePositions();
    }


    void OnOriginalResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult result = (RunResult)sender;
      RunResultWithPosition rrWP = _viewList.FirstOrDefault(r => r.Participant == result.Participant);

      if (rrWP == null)
        _viewList.InsertSorted(CreateRunResultWithPosition(result), _comparer);
      else
      {
        rrWP.UpdateRunResult(result);
        _viewList.Sort(_comparer);
      }

      UpdatePositions();
    }

    void UpdatePositions()
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
          }
          else
            item.DiffToFirst = item.Runtime - firstTime;

          item.Position = curPosition;
          lastTime = item.Runtime;
        }
        else
        {
          item.Position = 0;
          item.DiffToFirst = null;
        }

        // Set the JustModified flag to highlight new results
        item.JustModified = _appDataModel.JustMeasured(item.Participant.Participant);
      }
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


    protected virtual double calculatePoints(RaceResultItem rri)
    {
      return -1.0;
    }

    public enum TimeCombination { BestRun, Sum };
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
      }

      _lastConsideredRuns = new List<RaceRun>();
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
        rrVP.GetViewList().CollectionChanged += OnResultListCollectionChanged;
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

      RaceResultItem rri = _viewList.SingleOrDefault(x => x.Participant == participant);
      if (rri == null)
      {
        rri = new RaceResultItem(participant);
        _viewList.Add(rri);
      }

      // Look for the sub-result
      Dictionary<uint, RunResultWithPosition> results = new Dictionary<uint, RunResultWithPosition>();
      foreach (RaceRun run in _lastConsideredRuns)
      {
        RaceRunResultViewProvider rrVP = (run.GetResultViewProvider() as RaceRunResultViewProvider);
        RunResultWithPosition result = rrVP.GetViewList().SingleOrDefault(x => x.Participant == participant);
        results.Add(run.Run, result);
      }

      // Combine and update the race result
      foreach (var res in results)
      {
        bool sigCh = rri.SetRunResult(res.Key, res.Value);
        //significantChange = significantChange || sigCh;
      }
      
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
          }
          else
            sortedItem.DiffToFirst = sortedItem.TotalTime - firstTime;

          sortedItem.Position = curPosition;

          sortedItem.Points = calculatePoints(sortedItem);

          lastTime = sortedItem.TotalTime;
        }
        else
        {
          sortedItem.Position = 0;
          sortedItem.DiffToFirst = null;
        }

        // Set the JustModified flag to highlight new results
        sortedItem.JustModified = _appDataModel.JustMeasured(sortedItem.Participant.Participant);
      }
    }


    TimeSpan? MinimumTime(Dictionary<uint, RunResultWithPosition> results, out RunResult.EResultCode resCode, out string disqualText)
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

    TimeSpan? SumTime(Dictionary<uint, RunResultWithPosition> results, out RunResult.EResultCode resCode, out string disqualText)
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
      _dsvCalcM = new DSVRaceCalculation(race, this, "M");
      _dsvCalcW = new DSVRaceCalculation(race, this, "W");

      base.Init(race, appDataModel);
    }


    protected override void ResortResults()
    {
      if (_viewList == null)
        return;

      try
      {
        _dsvCalcM.CalculatePenalty();
        _dsvCalcW.CalculatePenalty();
      }
      catch (Exception) { }

      base.ResortResults();
    }

    protected override double calculatePoints(RaceResultItem rri)
    { 
      if (rri.Participant.Sex == "M")
        return _dsvCalcM.CalculatePoints(rri, true);
      if (rri.Participant.Sex == "W")
        return _dsvCalcW.CalculatePoints(rri, true);

      return -1.0;
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
