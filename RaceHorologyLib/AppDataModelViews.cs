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
        //gd.CustomSort = new DebugSort();
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
      if (left.StartNumber < right.StartNumber)
        return -1 * (int)_direction;
      else if (left.StartNumber > right.StartNumber)
        return 1  * (int)_direction;
      else
        return 0;
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

    protected System.Collections.Generic.IComparer<StartListEntry> _comparer;


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
          _viewList.InsertSorted(CreateStartListEntry((RaceParticipant)item), _comparer);
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
      // Pick best n starter in reverse order
      for (int i = _reverseBestN - 1; i >= 0; --i)
      {
        if (i >= resultsCurGroup.Count())
          continue;

        newStartList.Add(CreateStartListEntry(resultsCurGroup[i]));
      }

      List<RunResult> omittedResults = new List<RunResult>();
      for (int i = _reverseBestN; i < resultsCurGroup.Count(); ++i)
      {
        RunResult result = resultsCurGroup[i];
        if (result.Runtime != null)
          newStartList.Add(CreateStartListEntry(result));
        else
          omittedResults.Add(result);
      }

      if (_allowNonResults)
      {
        // Remaining in reverse order

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
      _viewList = new CopyObservableCollection<StartListEntry>(_srcStartListProvider.GetViewList(), sle => sle.ShallowCopy());
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
    public void Init(RaceRun raceRun, AppDataModel appDataModel)
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
          }
          else
            item.DiffToFirst = item.Runtime - firstTime;

          item.Position = curPosition;
          lastTime = item.Runtime;
        }
        else
          item.Position = 0;

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
    delegate TimeSpan? RunResultCombiner(Dictionary<uint, RunResult> results, out RunResult.EResultCode code, out string disqualText);

    TimeCombination _timeCombination;
    // Input Data
    Race _race;
    RaceRun[] _raceRuns;
    AppDataModel _appDataModel;

    // Working Data
    ItemsChangeObservableCollection<RaceResultItem> _viewList;
    ResultSorter<RaceResultItem> _comparer;
    RunResultCombiner _combineTime;

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
    }


    public override ViewProvider Clone()
    {
      return new RaceResultViewProvider(_timeCombination);
    }

    
    // Input: Race
    public void Init(Race race, AppDataModel appDataModel)
    {
      _race = race;
      _raceRuns = _race.GetRuns();
      _appDataModel = appDataModel;

      _viewList = new ItemsChangeObservableCollection<RaceResultItem>();

      foreach (RaceRun r in _raceRuns)
      {
        r.GetResultList().CollectionChanged += OnResultListCollectionChanged;
        OnResultListCollectionChanged(r.GetResultList(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, r.GetResultList().ToList()));
      }

      UpdateAll();

      FinalizeInit();
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
      RunResult rr = sender as RunResult;

      if (rr != null)
      {
        UpdateResultsFor(rr.Participant);
        ResortResults();
      }
    }

    private void OnResultListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          item.PropertyChanged -= OnRunResultItemChanged;
          RunResult rr = item as RunResult;
          UpdateResultsFor(rr?.Participant);
        }
        ResortResults();
      }

      if (e.NewItems != null)
      {
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          item.PropertyChanged += OnRunResultItemChanged;
          RunResult rr = item as RunResult;
          UpdateResultsFor(rr?.Participant);
        }
        ResortResults();
      }
    }

    private void UpdateAll()
    {
      foreach (RaceParticipant p in _race.GetParticipants())
        UpdateResultsFor(p);

      ResortResults();
    }

    private void UpdateResultsFor(RaceParticipant participant)
    {
      RaceResultItem rri = _viewList.SingleOrDefault(x => x.Participant == participant);
      if (rri == null)
      {
        rri = new RaceResultItem(participant);
        _viewList.Add(rri);
      }

      // Look for the sub-result
      Dictionary<uint, RunResult> results = new Dictionary<uint, RunResult>();
      foreach (RaceRun run in _raceRuns)
      {
        RunResult result = run.GetResultList().SingleOrDefault(x => x.Participant == participant);
        results.Add(run.Run, result);
      }

      // Combine and update the race result
      foreach (var res in results)
        rri.SetRunResult(res.Key, res.Value);
      RunResult.EResultCode code;
      string disqualText;
      rri.TotalTime = _combineTime(results, out code, out disqualText);
      rri.ResultCode = code;
      rri.DisqualText = disqualText;
    }


    void ResortResults()
    {
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
          }
          else
            sortedItem.DiffToFirst = sortedItem.TotalTime - firstTime;

          sortedItem.Position = curPosition;

          double valueF = 1010.0;
          double valueA = 0.0;
          sortedItem.Points = Math.Round(valueF * ((TimeSpan)sortedItem.TotalTime).TotalSeconds / ((TimeSpan)firstTime).TotalSeconds - valueF + valueA, 2);

          lastTime = sortedItem.TotalTime;
        }
        else
        {
          sortedItem.Position = 0;
        }

        // Set the JustModified flag to highlight new results
        sortedItem.JustModified = _appDataModel.JustMeasured(sortedItem.Participant.Participant);
      }
    }


    TimeSpan? MinimumTime(Dictionary<uint, RunResult> results, out RunResult.EResultCode resCode, out string disqualText)
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

    TimeSpan? SumTime(Dictionary<uint, RunResult> results, out RunResult.EResultCode resCode, out string disqualText)
    {
      TimeSpan? sumTime = new TimeSpan(0);
      resCode = RunResult.EResultCode.Normal;
      disqualText = "";

      foreach (var res in results)
      {
        if (res.Value == null)
          continue;

        if (res.Value?.Runtime != null)
          sumTime += (TimeSpan)res.Value.Runtime;
        else
          // no time ==> Invalid
          sumTime = null;

        if (res.Value.ResultCode != RunResult.EResultCode.Normal)
        {
          resCode = res.Value.ResultCode;
          disqualText = res.Value.DisqualText;
        }

      }

      return sumTime;
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