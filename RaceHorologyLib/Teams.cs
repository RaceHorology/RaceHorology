using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace RaceHorologyLib
{

  public class TeamsVM
  {
    public ObservableCollection<Team> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public TeamsVM()
    {
      Items = new ObservableCollection<Team>();

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
    }


    public void Clear()
    {
      Items.Clear();
    }

    public void Assign(IList<Team> teams)
    {
      Items.Clear();
      Items.InsertRange(teams);
      Items.Sort(new StdComparer());
    }

    public void Add(IList<Team> teams)
    {
      Items.InsertRange(teams);
      Items.Sort(new StdComparer());
    }


    public bool ContainsSimilar(Team c)
    {
      return Items.Contains(c);
    }


    public bool Merge(Team c)
    {
      if (!ContainsSimilar(c))
      {
        Items.Add(c);
        Items.Sort(new StdComparer());
        return true;
      }

      return false;
    }


    public void Merge(IList<Team> teams)
    {
      foreach (var c in teams)
        Merge(c);
    }
  }

  public class TeamGroupVM
  {
    public ObservableCollection<TeamGroup> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public TeamGroupVM()
    {
      Items = new ObservableCollection<TeamGroup>();

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
    }


    public void Clear()
    {
      Items.Clear();
    }


    public void Assign(IList<TeamGroup> groups)
    {
      Items.Clear();
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
    }


    public void Add(IList<TeamGroup> groups)
    {
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
    }


    public bool ContainsSimilar(TeamGroup g)
    {
      return Items.FirstOrDefault(i => i.Name == g.Name) != null;
    }


    public bool Merge(TeamGroup g)
    {
      if (!ContainsSimilar(g))
      {
        Items.Add(g);
        Items.Sort(new StdComparer());
        return true;
      }

      return false;
    }

    public void Merge(IList<TeamGroup> groups)
    {
      foreach (var g in groups)
        Merge(g);
    }

  }


  public class TeamsEditVM
  {
    private AppDataModel _dm;
    Dictionary<TeamGroup, TeamGroup> _group2Group;
    Dictionary<Team, Team> _team2Team;


    public TeamGroupVM TeamGroupViewModel { get; }
    public TeamsVM TeamViewModel { get; }


    public TeamsEditVM(AppDataModel dm)
    {
      _dm = dm;
      _group2Group = new Dictionary<TeamGroup, TeamGroup>();
      _team2Team = new Dictionary<Team, Team>();

      TeamGroupViewModel = new TeamGroupVM();
      TeamViewModel = new TeamsVM();

      initialize();
    }

    private void initialize()
    {
      Clear();

      Import(_dm);
    }


    public void Clear()
    {
      TeamGroupViewModel.Clear();
      TeamViewModel.Clear();

      _group2Group.Clear();
      _team2Team.Clear();
    }

    public void Reset()
    {
      initialize();
    }

    public bool DifferentToDataModel()
    {
      return groupsDifferent() || teamsDifferent();
    }


    public void Import(AppDataModel srcModel)
    {
      var srcTeamGroups = srcModel.GetTeamGroups();
      var srcTeams = srcModel.GetTeams();

      foreach (var g1 in srcTeamGroups)
      {
        if (!TeamGroupViewModel.ContainsSimilar(g1))
        {
          TeamGroup g2 = null;
          if (!_group2Group.TryGetValue(g1, out g2))
          {
            g2 = new TeamGroup(g1.Id, g1.Name, g1.SortPos);
            _group2Group.Add(g1, g2);
            TeamGroupViewModel.Merge(g2);
          }
          else
            System.Diagnostics.Debug.Assert(false);
        }
      }

      foreach (var t1 in srcTeams)
      {
        if (!TeamViewModel.ContainsSimilar(t1))
        {
          Team t2 = null;
          if (!_team2Team.TryGetValue(t1, out t2))
          {
            t2 = new Team(
              t1.Id,
              t1.Group == null ? null : _group2Group[t1.Group],
              t1.Name,
              t1.SortPos);
            _team2Team.Add(t1, t2);
            TeamViewModel.Merge(t2);
          }
          else
            System.Diagnostics.Debug.Assert(false);
        }
      }
    }



    public void Store()
    {
      storeGroups();
      storeTeams();
      Reset();
    }

    private void storeGroups()
    {
      // *** Delete removed one
      var toDelete = new List<TeamGroup>();
      foreach (var g2 in _dm.GetTeamGroups())
      {
        TeamGroup g1 = null;
        _group2Group.TryGetValue(g2, out g1);
        if (g1 == null || TeamGroupViewModel.Items.FirstOrDefault(i => i == g1) == null)
          toDelete.Add(g2);
      }
      foreach (var g in toDelete)
      {
        _dm.GetTeamGroups().Remove(g);
        _group2Group.Remove(g);
      }

      // *** Update & create new ones
      uint curSortPos = 1;
      foreach (var g1 in TeamGroupViewModel.Items)
      {
        var found = _group2Group.FirstOrDefault(i => i.Value == g1); // Find original
        var g2 = found.Key;
        g2 = _dm.GetTeamGroups().FirstOrDefault(i => i == g2); // Check if already in DataModel

        if (g2 != null)
        { // Update existing one
          g2.Name = g1.Name;
          g2.SortPos = curSortPos;
        }
        else
        { // Create new one
          var gNew = new TeamGroup(null, g1.Name, curSortPos);
          _dm.GetTeamGroups().Add(gNew);
          // Remove any old reference and replace with new one
          if (found.Key != null) _group2Group.Remove(found.Key);
          _group2Group.Add(gNew, g1);
        }

        curSortPos++;
      }
    }

    private bool groupsDifferent()
    {
      // *** Check removed one
      foreach (var g2 in _dm.GetTeamGroups())
      {
        TeamGroup g1 = null;
        _group2Group.TryGetValue(g2, out g1);
        if (g1 == null || TeamGroupViewModel.Items.FirstOrDefault(i => i == g1) == null)
          return true;
      }

      // *** Check updated & new ones
      uint curSortPos = 1;
      foreach (var g1 in TeamGroupViewModel.Items)
      {
        var found = _group2Group.FirstOrDefault(i => i.Value == g1); // Find original
        var g2 = found.Key;
        g2 = _dm.GetTeamGroups().FirstOrDefault(i => i == g2); // Check if already in DataModel

        if (g2 != null)
        { // Update existing one
          if (g2.Name != g1.Name || g2.SortPos != curSortPos)
            return true;
        }
        else
          return true;

        curSortPos++;
      }

      return false;
    }


    private void storeTeams()
    {
      // Delete removed one
      var toDelete = new List<Team>();
      foreach (var t2 in _dm.GetTeams())
      {
        Team t1 = null;
        _team2Team.TryGetValue(t2, out t1);
        if (t1 == null || TeamViewModel.Items.FirstOrDefault(i => i == t1) == null)
          toDelete.Add(t2);
      }
      foreach (var t in toDelete)
        _dm.GetTeams().Remove(t);

      // Update & create new ones
      uint curSortPos = 1;
      foreach (var t1 in TeamViewModel.Items)
      {
        var found = _team2Team.FirstOrDefault(i => i.Value == t1);
        var t2 = found.Key;
        t2 = _dm.GetTeams().FirstOrDefault(i => i == t2);

        var g2 = _group2Group.FirstOrDefault(i => i.Value == t1.Group);

        if (t2 != null)
        { // Update existing one
          t2.Name = t1.Name;
          t2.Group = g2.Key;
          t2.SortPos = curSortPos;
        }
        else
        { // Create new one
          var tNew = new Team(null, g2.Key, t1.Name, curSortPos);
          _dm.GetTeams().Add(tNew);
          // Remove any old reference and replace with new one
          if (found.Key != null) _team2Team.Remove(found.Key);
          _team2Team.Add(tNew, t1);
        }

        curSortPos++;
      }
    }

    private bool teamsDifferent()
    {
      // Check removed one
      foreach (var t2 in _dm.GetTeams())
      {
        Team t1 = null;
        _team2Team.TryGetValue(t2, out t1);
        if (t1 == null || TeamViewModel.Items.FirstOrDefault(i => i == t1) == null)
          return true;
      }

      // Check update & new ones
      uint curSortPos = 1;
      foreach (var t1 in TeamViewModel.Items)
      {
        var found = _team2Team.FirstOrDefault(i => i.Value == t1);  
        var t2 = found.Key;
        t2 = _dm.GetTeams().FirstOrDefault(i => i == t2);

        var g2 = _group2Group.FirstOrDefault(i => i.Value == t1.Group);

        if (t2 != null)
        { 
          if (t2.Name != t1.Name
              || t2.Group != g2.Key
              || t2.SortPos != curSortPos)
            return true;
        }
        else
          return true;

        curSortPos++;
      }

      return false;
    }
  }
}