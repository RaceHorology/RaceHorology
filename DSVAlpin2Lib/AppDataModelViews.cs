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
    public int Compare(StartListEntry left, StartListEntry right)
    {
      if (left.StartNumber < right.StartNumber)
        return -1;
      else if (left.StartNumber > right.StartNumber)
        return 1;
      else
        return 0;
    }
  }


  public class StartListViewProvider : ViewProvider
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


  // First n (15) per grouping are always kept constant
  public class DSVFirstRunStartListViewProvider : FirstRunStartListViewProvider
  {

    // Input: List<RaceParticipant>

    // Output: sorted List<StartListEntry> according to StartNumber

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




  public class RemainingStartListViewProvider : StartListViewProvider
  {

    // Input: StartListViewProvider or List<StartListEntry>

    // Output: sorted List<StartListEntry> according to StartNumber

  }



  public class ResultViewProvider : ViewProvider
  {
  }


  public class RaceRunResultViewProvider : ResultViewProvider
  {
    // Input: RaceRun

    // Output: List<RunResultWithPosition>


  }


  public class RaceResultViewProvider : ResultViewProvider
  {
    // Input: Race

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
