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
    }

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
    public virtual int Compare(StartListEntry left, StartListEntry right)
    {
      if (left.StartNumber < right.StartNumber)
        return -1;
      else if (left.StartNumber > right.StartNumber)
        return 1;
      else
        return 0;
    }
  }

  interface IStartListViewProvider
  {
    ICollectionView GetView();


  }

  public class StartListViewProvider : ViewProvider, IStartListViewProvider
  {
    private ObservableCollection<RaceParticipant> _participants;
    private ObservableCollection<StartListEntry> _viewList;
    protected System.Collections.Generic.IComparer<StartListEntry> _comparer;
    protected ItemsChangedNotifier _sourceItemChangedNotifier;

    public StartListViewProvider()
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

    private static StartListEntry CreateStartListEntry(RaceParticipant participant)
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

    // Input: List<RaceParticipant>

    // Output: sorted List<StartListEntry> according to StartNumber

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
      return left.Points.CompareTo(right.Points);
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




  public class SecondRunStartListViewProvider : StartListViewProvider
  {
    // Input: List<StartListEntry> (1st run),
    //        List<RaceResultWithPosition> (1st run)

    // Output: sorted List<StartListEntry> according to StartNumber

  }


  // wie 1. DG, 1. DG rückwärts
  public class SimpleSecondRunStartListViewProvider : SecondRunStartListViewProvider
  {
    

  }


  // basierend auf (1. DG) Ergebnisliste: rückwärts, ersten n gelost, mit/ohne disqualifizierten vorwärts oder rückwärts
  public class BasedOnResultsSecondRunStartListViewProvider : SecondRunStartListViewProvider
  {


  }




  public class RemainingStartListViewProvider : IStartListViewProvider
  {
    StartListViewProvider _srcStartListProvider;
    RaceRun _raceRun;

    ObservableCollection<StartListEntry> _viewList;
    CollectionViewSource _view;

    public RemainingStartListViewProvider()
    {
      _view = new CollectionViewSource();
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
  }



  // Propagate class to sorter
  public class RuntimeSorter : System.Collections.Generic.IComparer<RunResultWithPosition>
  {
    public int Compare(RunResultWithPosition rrX, RunResultWithPosition rrY)
    {
      TimeSpan? tX = rrX.Runtime;
      TimeSpan? tY = rrY.Runtime;

      throw new NotImplementedException;
      // Sort by grouping (class or group or ...)
      // TODO: Shall be configurable
      int classCompare = rrX.Participant.Participant.Class.CompareTo(rrY.Participant.Participant.Class);
      if (classCompare != 0)
        return classCompare;

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
    System.Collections.Generic.IComparer<RunResultWithPosition> _comparer;


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
      _originalResults.CollectionChanged += OnOriginalResultsChanged;
      _originalResults.ItemChanged += OnOriginalResultItemChanged;

      FinalizeInit();
    }


    // Output: List<RunResultWithPosition>


    protected override object GetViewSource()
    {
      return _viewList;
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
      ParticipantClass curClass = null;
      TimeSpan? lastTime = null;
      foreach (RunResultWithPosition item in _viewList)
      {
        if (item.Class != curClass)
        {
          curClass = item.Class;
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

    static readonly TimeSpan delta = new TimeSpan(0, 0, 1); // 1 sec
  }


  public class RaceResultViewProvider : ResultViewProvider
  {
    // Input: Race
    public void Init(Race race)
    {

    }

    protected override object GetViewSource()
    {
      throw new NotImplementedException();
    }


    // Output: List<RunResultWithPosition>


  }

  /* e.g. FamilienWertung
    public class SpecialRaceResultViewProvider : ResultViewProvider
    {
      // Input: Race

      // Output: List<RunResultWithPosition>


    }
    */



}
