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

  public class StartListProvider
  {
    private Race _race;

    ObservableCollection<RaceParticipant> _participants;
    CollectionViewSource _startListView;

    public StartListProvider(Race race, ObservableCollection<RaceParticipant> participants)
    {
      _race = race;

      _participants = participants;

      _startListView = new CollectionViewSource();

      _startListView.Source = _participants;

      _startListView.SortDescriptions.Clear();
      _startListView.SortDescriptions.Add(new SortDescription(nameof(RaceParticipant.StartNumber), ListSortDirection.Ascending));

      _startListView.LiveSortingProperties.Add(nameof(RaceParticipant.StartNumber));
      _startListView.IsLiveSortingRequested = true;

      //_startListView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));
      //_startListView.LiveGroupingProperties.Add(nameof(Participant.Class));
      //_startListView.IsLiveGroupingRequested = true;
    }

    public System.ComponentModel.ICollectionView GetStartList()
    {
      return _startListView.View;
    }
  }


  public class ResultViewProvider
  {
    ItemsChangeObservableCollection<RunResult> _originalResults;
    AppDataModel _appDataModel;
    ItemsChangeObservableCollection<RunResultWithPosition> _results;
    System.Collections.Generic.IComparer<RunResultWithPosition> _sorter;

    CollectionViewSource _resultListView;

    public class RuntimeSorter : System.Collections.Generic.IComparer<RunResultWithPosition>
    {
      public int Compare(RunResultWithPosition rrX, RunResultWithPosition rrY)
      {
        TimeSpan? tX = rrX.Runtime;
        TimeSpan? tY = rrY.Runtime;

        // Sort by grouping (class or group or ...)
        // TODO: Shall be configurable
        int classCompare = rrX.Class.CompareTo(rrY.Class);
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


    public ResultViewProvider(ItemsChangeObservableCollection<RunResult> results, AppDataModel appDataModel)
    {
      _sorter = new RuntimeSorter();

      _originalResults = results;
      _appDataModel = appDataModel;

      _originalResults.CollectionChanged += OnOriginalResultsChanged;
      _originalResults.ItemChanged += OnOriginalResultItemChanged;

      _resultListView = new CollectionViewSource();

      _results = new ItemsChangeObservableCollection<RunResultWithPosition>();
      foreach (var result in _originalResults)
        _results.Add(new RunResultWithPosition(result));
      ResortResults();

      _resultListView.Source = _results;


      _resultListView.SortDescriptions.Clear();
      //_resultListView.SortDescriptions.Add(new SortDescription(nameof(RunResult.Runtime), ListSortDirection.Ascending));

      //_resultListView.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RunResult)ea.Item).Runtime != null; });
      //_resultListView.LiveFilteringProperties.Add(nameof(RunResult.Runtime));
      //_resultListView.IsLiveFilteringRequested = true;

      // TODO: Check this out
      ListCollectionView llview = _resultListView.View as ListCollectionView;
      //llview.CustomSort = new RuntimeSorter();

      //_resultListView.LiveSortingProperties.Add(nameof(RunResult.Runtime));
      //_resultListView.IsLiveSortingRequested = true;

      _resultListView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));
      _resultListView.LiveGroupingProperties.Add(nameof(Participant.Class));
      _resultListView.IsLiveGroupingRequested = true;
    }

    public System.ComponentModel.ICollectionView GetResultView()
    {
      return _resultListView.View;
    }

    void OnOriginalResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RunResult result = (RunResult)item;
          var itemsToRemove = _results.Where(r => r == result).ToList();
          foreach (var itemToRemove in itemsToRemove)
            _results.Remove(itemToRemove);
        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          // Add to results
          RunResult result = (RunResult)item;
          _results.Add(new RunResultWithPosition(result));
        }

      ResortResults();
    }

    void OnOriginalResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult result = (RunResult)sender;
      RunResultWithPosition rrWP = _results.FirstOrDefault(r => r.Participant == result.Participant);

      if (rrWP == null)
        _results.Add(new RunResultWithPosition(result));
      else
        rrWP.UpdateRunResult(result);

      // Anything to update?
      ResortResults();
    }


    void ResortResults()
    {
      // TODO: Could be much more efficient; consumes O(nlogn * n); but underlaying data structure _results needs to be changed to support in-place sorting (e.g. an array)
      // Sort:
      // 1. by Class
      // 2. by Time

      var sortedResults = _results.ToList();
      sortedResults.Sort(_sorter);
      _results.Clear();

      uint curPosition = 1;
      uint samePosition = 1;
      string curClass = "";
      TimeSpan? lastTime = null;
      foreach (var sortedItem in sortedResults)
      {
        if (sortedItem.Class != curClass)
        {
          curClass = sortedItem.Class;
          curPosition = 1;
          lastTime = null;
        }

        if (sortedItem.Runtime != null)
        {
          sortedItem.Position = curPosition;

          // Same position in case same time
          if (sortedItem.Runtime == lastTime)//< TimeSpan.FromMilliseconds(9))
          {
            samePosition++;
          }
          else
          {
            curPosition += samePosition;
            samePosition = 1;
          }
          lastTime = sortedItem.Runtime;
        }
        else
        {
          sortedItem.Position = 0;
        }

        // Set the JustModified flag to highlight new results
        sortedItem.JustModified = _appDataModel.JustMeasured(sortedItem.Participant.Participant);

        _results.Add(sortedItem);
      }
    }

    static readonly TimeSpan delta = new TimeSpan(0, 0, 1); // 1 sec
  }



  public class RaceResultProvider
  {
    Race _race;
    RaceRun[] _raceRuns;
    AppDataModel _appDataModel;
    ItemsChangeObservableCollection<RaceResultItem> _raceResults;
    System.Collections.Generic.IComparer<RaceResultItem> _sorter = new TotalTimeSorter();
    CollectionViewSource _raceResultsView;


    public class TotalTimeSorter : System.Collections.Generic.IComparer<RaceResultItem>
    {
      public int Compare(RaceResultItem rrX, RaceResultItem rrY)
      {
        TimeSpan? tX = rrX.TotalTime;
        TimeSpan? tY = rrY.TotalTime;

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



    public RaceResultProvider(Race race, RaceRun[] rr, AppDataModel appDataModel)
    {
      _race = race;
      _raceRuns = rr;
      _appDataModel = appDataModel;
      _raceResults = new ItemsChangeObservableCollection<RaceResultItem>();

      foreach (RaceRun r in _raceRuns)
      {
        r.GetResultList().CollectionChanged += OnResultListCollectionChanged;
        OnResultListCollectionChanged(r.GetResultList(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, r.GetResultList().ToList()));
      }


      _raceResultsView = new CollectionViewSource();

      _raceResultsView.Source = _raceResults;

      _raceResultsView.SortDescriptions.Clear();
      //_raceResultsView.SortDescriptions.Add(new SortDescription(nameof(RaceResultItem.TotalTime), ListSortDirection.Ascending));

      //_raceResultsView.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RaceResultItem)ea.Item).TotalTime != null; });
      //_raceResultsView.LiveFilteringProperties.Add(nameof(RaceResultItem.TotalTime));
      //_raceResultsView.IsLiveFilteringRequested = true;

      // TODO: Seems like sorting null at the end does not work ... visible if the Filter above is turned off ...
      ListCollectionView llview = _raceResultsView.View as ListCollectionView;
      //llview.CustomSort = new TotalTimeSorter();

      //_raceResultsView.LiveSortingProperties.Add(nameof(RaceResultItem.TotalTime));
      //_raceResultsView.IsLiveSortingRequested = true;

      _raceResultsView.GroupDescriptions.Add(new PropertyGroupDescription("Participant.Class"));
      _raceResultsView.LiveGroupingProperties.Add("Participant.Class");
      _raceResultsView.IsLiveGroupingRequested = true;

      UpdateAll();
    }


    public System.ComponentModel.ICollectionView GetView()
    {
      return _raceResultsView.View;
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
      RaceResultItem rri = _raceResults.SingleOrDefault(x => x.Participant == participant);
      if (rri == null)
      {
        rri = new RaceResultItem(participant);
        _raceResults.Add(rri);
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
      // TODO: Could be much more efficient; consumes O(nlogn * n); but underlaying data structure _results needs to be changed to support in-place sorting (e.g. an array)
      // Sort:
      // 1. by Class
      // 2. by Time

      var sortedResults = _raceResults.ToList();
      sortedResults.Sort(_sorter);
      _raceResults.Clear();

      uint curPosition = 1;
      uint samePosition = 1;
      string curClass = "";
      TimeSpan? lastTime = null;
      foreach (var sortedItem in sortedResults)
      {
        if (sortedItem.Participant.Participant.Class != curClass)
        {
          curClass = sortedItem.Participant.Participant.Class;
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

        _raceResults.Add(sortedItem);
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



}
