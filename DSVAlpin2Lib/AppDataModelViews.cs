using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSVAlpin2Lib
{

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
    }

    public void ChangeGrouping(string propertyName)
    {
      if (_activeGrouping != propertyName)
      {
        _view.GroupDescriptions.Clear();
        _view.LiveGroupingProperties.Clear();
        _view.IsLiveGroupingRequested = false;
      }

      if (!string.IsNullOrEmpty(propertyName))
      { 
        _view.GroupDescriptions.Add(new PropertyGroupDescription(propertyName));
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

  interface IStartListViewProvider
  {
    ICollectionView GetView();
  }

  public abstract class StartListViewProvider : ViewProvider, IStartListViewProvider
  {
    protected ObservableCollection<StartListEntry> _viewList;
    protected System.Collections.Generic.IComparer<StartListEntry> _comparer;

    public StartListViewProvider()
    {
      _comparer = new StartListEntryComparer();
    }

    // Output: sorted List<StartListEntry> according to StartNumber
    public ObservableCollection<StartListEntry> GetViewList()
    {
      return _viewList;
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

    public FirstRunStartListViewProvider()
    {
      _comparer = new StartListEntryComparer();
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

    private void _sourceItemChangedNotifier_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      // Ensure list is sorted again
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
    public DSVFirstRunStartListViewProvider(int firstNStartnumbers)
    {
      _comparer = new PointsStartListEntryComparer(firstNStartnumbers);
    }

    // Parameter: first n

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

    // Working
    ItemsChangedNotifier _sourceItemChangedNotifier;


    public SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction direction)
    {
      _comparer = new StartListEntryComparer(direction);
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
    public override void Init(RaceRun previousRun)
    {
      throw new NotImplementedException();
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
      _defaultGrouping = propertyName;
    }

    public void ChangeGrouping(string propertyName)
    {
      if (_activeGrouping != propertyName)
      {
        _view.GroupDescriptions.Clear();
        _view.LiveGroupingProperties.Clear();
        _view.IsLiveGroupingRequested = false;
      }

      if (!string.IsNullOrEmpty(propertyName))
      {
        _view.GroupDescriptions.Add(new PropertyGroupDescription(propertyName));
        _view.LiveGroupingProperties.Add(propertyName);
        _view.IsLiveGroupingRequested = true;
      }

      _activeGrouping = propertyName;
    }

    public void ResetToDefaultGrouping()
    {
      ChangeGrouping(_defaultGrouping);
    }


    // Input: StartListViewProvider or List<StartListEntry>
    public void Init(StartListViewProvider startListProvider, RaceRun raceRun)
    {
      // Remember the source
      _srcStartListProvider = startListProvider;
      _raceRun = raceRun;

      // Observe the results
      _raceRun.GetResultList().CollectionChanged += OnResultsChanged;
      _raceRun.GetResultList().ItemChanged += OnResultItemChanged;

      // Create working list
      _viewList = new CopyObservableCollection<StartListEntry>(_srcStartListProvider.GetViewList(), sle => new StartListEntry(sle.Participant));
      foreach (StartListEntry entry in _viewList)
        UpdateStartListEntry(entry);


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

    public static object GetGroupValue(object obj, string propertyName)
    {
      if (propertyName == null || obj == null)
        return null;

      foreach (string part in propertyName.Split('.'))
      {
        if (obj == null) { return null; }

        Type type = obj.GetType();
        System.Reflection.PropertyInfo info = type.GetProperty(part);
        if (info == null) { return null; }

        obj = info.GetValue(obj, null);
      }
      return obj;
    }

  }



  public abstract class ResultSorter<T> : IComparer<T>
  {
    string _groupingPropertyName;
    public void SetGrouping(string propertyName)
    {
      _groupingPropertyName = propertyName;
    }

    protected int CompareGroup(RunResultWithPosition rrX, RunResultWithPosition rrY)
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
  public class RuntimeSorter : ResultSorter<RunResultWithPosition>
  {

    public override int Compare(RunResultWithPosition rrX, RunResultWithPosition rrY)
    {
      TimeSpan? tX = rrX.Runtime;
      TimeSpan? tY = rrY.Runtime;

      int groupCompare = CompareGroup(rrX, rrY);
      if (groupCompare != 0)
        return groupCompare;

      // Sort by time
      if (tX == null && tY == null)
        return 0;

      if (tX != null && tY == null)
        return -1;

      if (tX == null && tY != null)
        return 1;

      return TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
    }
  }


  public class RaceRunResultViewProvider : ResultViewProvider
  {
    // Input Data
    ItemsChangeObservableCollection<RunResult> _originalResults;
    AppDataModel _appDataModel;

    // Working Data
    ItemsChangeObservableCollection<RunResultWithPosition> _viewList;
    ResultSorter<RunResultWithPosition> _comparer;


    public RaceRunResultViewProvider()
    {
      _comparer = new RuntimeSorter();
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


    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);
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
      uint curPosition = 1;
      uint samePosition = 1;
      object curGroup = null;
      TimeSpan? lastTime = null;
      foreach (RunResultWithPosition item in _viewList)
      {
        if (!Equals(GetGroupValue(item, _activeGrouping), curGroup))
        {
          curGroup = GetGroupValue(item, _activeGrouping);
          curPosition = 1;
          lastTime = null;
        }

        if (item.Runtime != null)
        {
          item.Position = curPosition;

          // Same position in case same time
          if (item.Runtime == lastTime)//< TimeSpan.FromMilliseconds(9))
            samePosition++;
          else
          {
            curPosition += samePosition;
            samePosition = 1;
          }
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
      if (tX == null && tY == null)
        return 0;

      if (tX != null && tY == null)
        return -1;

      if (tX == null && tY != null)
        return 1;

      return TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
    }
  }


  public class RaceResultViewProvider : ResultViewProvider
  {
    // Input Data
    Race _race;
    RaceRun[] _raceRuns;
    AppDataModel _appDataModel;

    // Working Data
    ItemsChangeObservableCollection<RaceResultItem> _viewList;
    ResultSorter<RaceResultItem> _comparer;


    public RaceResultViewProvider()
    {
      _comparer = new TotalTimeSorter();
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
      rri.TotalTime = MinimumTime(results);
    }


    void ResortResults()
    {
      _viewList.Sort(_comparer);

      uint curPosition = 1;
      uint samePosition = 1;

      object curGroup = null;

      TimeSpan? lastTime = null;
      foreach (var sortedItem in _viewList)
      {
        if (!Equals(GetGroupValue(sortedItem, _activeGrouping), curGroup))
        {
          curGroup = GetGroupValue(sortedItem, _activeGrouping);
          curPosition = 1;
          lastTime = null;
        }

        if (sortedItem.TotalTime != null)
        {
          sortedItem.Position = curPosition;

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


    TimeSpan? MinimumTime(Dictionary<uint, RunResult> results)
    {
      TimeSpan? minTime = null;

      foreach (var res in results)
      {
        if (res.Value != null && res.Value.Runtime != null)
        {
          if (minTime == null || TimeSpan.Compare((TimeSpan)res.Value.Runtime, (TimeSpan)minTime) < 0)
            minTime = res.Value.Runtime;
        }
      }

      return minTime;
    }

    TimeSpan? SumTime(Dictionary<uint, RunResult> results)
    {
      TimeSpan sumTime = new TimeSpan(0);

      foreach (var res in results)
      {
        if (res.Value != null && res.Value.Runtime != null)
        {
          sumTime += (TimeSpan)res.Value.Runtime;
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
