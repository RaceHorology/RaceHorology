using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace RaceHorologyLib
{

  public class CategoryVM
  {
    public ObservableCollection<ParticipantCategory> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public CategoryVM()
    {
      Items = new ObservableCollection<ParticipantCategory>();

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
    }


    public void Clear()
    {
      Items.Clear();
    }

    public void Assign(IList<ParticipantCategory> categories)
    {
      Items.Clear();
      Items.InsertRange(categories);
      Items.Sort(new StdComparer());
    }

    public void Add(IList<ParticipantCategory> categories)
    {
      Items.InsertRange(categories);
      Items.Sort(new StdComparer());
    }


    public bool ContainsSimilar(ParticipantCategory c)
    {
      return Items.Contains(c);
    }


    public bool Merge(ParticipantCategory c)
    {
      if (!ContainsSimilar(c))
      {
        Items.Add(c);
        Items.Sort(new StdComparer());
        return true;
      }

      return false;
    }


    public void Merge(IList<ParticipantCategory> categories)
    {
      foreach (var c in categories)
        Merge(c);
    }
  }

  public class GroupVM
  {
    public ObservableCollection<ParticipantGroup> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public GroupVM()
    {
      Items = new ObservableCollection<ParticipantGroup>();

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
    }


    public void Clear()
    {
      Items.Clear();
    }


    public void Assign(IList<ParticipantGroup> groups)
    {
      Items.Clear();
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
    }


    public void Add(IList<ParticipantGroup> groups)
    {
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
    }


    public bool ContainsSimilar(ParticipantGroup g)
    {
      return Items.FirstOrDefault(i => i.Name == g.Name) != null;
    }


    public bool Merge(ParticipantGroup g)
    {
      if (!ContainsSimilar(g))
      {
        Items.Add(g);
        Items.Sort(new StdComparer());
        return true;
      }

      return false;
    }

    public void Merge(IList<ParticipantGroup> groups)
    {
      foreach (var g in groups)
        Merge(g);
    }

  }


  public class ClassVM
  {
    public ObservableCollection<ParticipantClass> Items { get; }

    public ClassVM()
    {
      Items = new ObservableCollection<ParticipantClass>();
    }



    public void Clear()
    {
      Items.Clear();
    }


    public void Assign(IList<ParticipantClass> classes)
    {
      Items.Clear();
      Items.InsertRange(classes);
      Items.Sort(new StdComparer());
    }


    public void Add(IList<ParticipantClass> classes)
    {
      Items.InsertRange(classes);
      Items.Sort(new StdComparer());
    }


    public bool ContainsSimilar(ParticipantClass c)
    {
      return Items.FirstOrDefault(i => (i.Name == c.Name && i.Year == c.Year && i.Sex == c.Sex)) != null;
    }


    public bool Merge(ParticipantClass c)
    {
      if (!ContainsSimilar(c))
      { 
        Items.Add(c);
        Items.Sort(new StdComparer());
        return true;
      }

      return false;
    }


    public void Merge(IList<ParticipantClass> classes)
    {
      foreach (var c in classes)
        Merge(c);
    }
  }




  public class ClassesGroupsCategoriesEditVM
  {
    private AppDataModel _dm;
    Dictionary<ParticipantGroup, ParticipantGroup> _group2Group;
    Dictionary<ParticipantClass, ParticipantClass> _class2Class;
    Dictionary<ParticipantCategory, ParticipantCategory> _category2Category;


    public GroupVM GroupViewModel { get; }
    public ClassVM ClassViewModel { get; }
    public CategoryVM CategoryViewModel { get; }


    public ClassesGroupsCategoriesEditVM(AppDataModel dm)
    {
      _dm = dm;
      _group2Group = new Dictionary<ParticipantGroup, ParticipantGroup>();
      _class2Class = new Dictionary<ParticipantClass, ParticipantClass>();
      _category2Category = new Dictionary<ParticipantCategory, ParticipantCategory>();

      GroupViewModel = new GroupVM();
      ClassViewModel = new ClassVM();
      CategoryViewModel = new CategoryVM();

      initialize();
    }

    private void initialize()
    {
      Clear();

      Import(_dm);
    }


    public void Clear()
    {
      GroupViewModel.Clear();
      ClassViewModel.Clear();
      CategoryViewModel.Clear();

      _group2Group.Clear();
      _class2Class.Clear();
      _category2Category.Clear();
    }


    public void Reset()
    {
      initialize();
    }


    public bool DifferentToDataModel()
    {
      return groupsDifferent() || categoriesDifferent() || classesDifferent();
    }


    public void Import(AppDataModel srcModel)
    {
      var srcGroups = srcModel.GetParticipantGroups();
      var srcClasses = srcModel.GetParticipantClasses();
      var srcCategories = srcModel.GetParticipantCategories();


      // TODO: only really added items are allowed to be in _x2x maps otherwise, class becomes inconsistent or points to a wrong group/category

      foreach (var g1 in srcGroups)
      {
        if (!GroupViewModel.ContainsSimilar(g1))
        {
          ParticipantGroup g2 = null;
          if (!_group2Group.TryGetValue(g1, out g2))
          {
            g2 = new ParticipantGroup(g1.Id, g1.Name, g1.SortPos);
            _group2Group.Add(g1, g2);
            GroupViewModel.Merge(g2);
          }
          else
            System.Diagnostics.Debug.Assert(false);
        }
      }

      foreach (var cat1 in srcCategories)
      {
        if (!CategoryViewModel.ContainsSimilar(cat1))
        {
          ParticipantCategory cat2 = null;
          if (!_category2Category.TryGetValue(cat1, out cat2))
          {
            cat2 = new ParticipantCategory(cat1.Name, cat1.PrettyName, cat1.SortPos, cat1.Synonyms);
            _category2Category.Add(cat1, cat2);
            CategoryViewModel.Merge(cat2);
          }
          else
            System.Diagnostics.Debug.Assert(false);
        }
      }

      foreach (var c1 in srcClasses)
      {
        if (!ClassViewModel.ContainsSimilar(c1))
        {
          ParticipantClass c2 = null;
          if (!_class2Class.TryGetValue(c1, out c2))
          {
            c2 = new ParticipantClass(
              c1.Id,
              c1.Group == null ? null : _group2Group[c1.Group],
              c1.Name,
              c1.Sex == null ? null : _category2Category[c1.Sex],
              c1.Year,
              c1.SortPos);
            _class2Class.Add(c1, c2);
            ClassViewModel.Merge(c2);
          }
          else
            System.Diagnostics.Debug.Assert(false);
        }
      }
    }



    public void Store()
    {
      storeGroups();
      storeCategories();
      storeClasses();
      Reset();
    }

    private void storeGroups()
    {
      // *** Delete removed one
      List<ParticipantGroup> toDelete = new List<ParticipantGroup>();
      foreach (var g2 in _dm.GetParticipantGroups())
      {
        ParticipantGroup g1 = null;
        _group2Group.TryGetValue(g2, out g1);
        if (g1 == null || GroupViewModel.Items.FirstOrDefault(i => i == g1) == null)
          toDelete.Add(g2);
      }
      foreach (var g in toDelete)
      {
        _dm.GetParticipantGroups().Remove(g);
        _group2Group.Remove(g);
      }

      // *** Update & create new ones
      uint curSortPos = 1;
      foreach (var g1 in GroupViewModel.Items)
      {
        var found = _group2Group.FirstOrDefault(i => i.Value == g1); // Find original
        var g2 = found.Key;
        g2 = _dm.GetParticipantGroups().FirstOrDefault(i => i == g2); // Check if already in DataModel

        if (g2 != null)
        { // Update existing one
          g2.Name = g1.Name;
          g2.SortPos = curSortPos;
        }
        else
        { // Create new one
          var gNew = new ParticipantGroup(null, g1.Name, curSortPos);
          _dm.GetParticipantGroups().Add(gNew);
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
      foreach (var g2 in _dm.GetParticipantGroups())
      {
        ParticipantGroup g1 = null;
        _group2Group.TryGetValue(g2, out g1);
        if (g1 == null || GroupViewModel.Items.FirstOrDefault(i => i == g1) == null)
          return true;
      }

      // *** Check updated & new ones
      uint curSortPos = 1;
      foreach (var g1 in GroupViewModel.Items)
      {
        var found = _group2Group.FirstOrDefault(i => i.Value == g1); // Find original
        var g2 = found.Key;
        g2 = _dm.GetParticipantGroups().FirstOrDefault(i => i == g2); // Check if already in DataModel

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


    private void storeCategories()
    {
      // *** Delete removed one
      List<ParticipantCategory> toDelete = new List<ParticipantCategory>();
      foreach (var cat2 in _dm.GetParticipantCategories())
      {
        ParticipantCategory cat1 = null;
         _category2Category.TryGetValue(cat2, out cat1);
        if (cat1 == null || CategoryViewModel.Items.FirstOrDefault(i => i == cat1) == null)
          toDelete.Add(cat2);
      }
      foreach (var cat in toDelete)
      {
        _dm.GetParticipantCategories().Remove(cat);
        _category2Category.Remove(cat);
      }

      // *** Update & create new ones
      uint curSortPos = 1;
      foreach (var cat1 in CategoryViewModel.Items)
      {
        var found = _category2Category.FirstOrDefault(i => i.Value == cat1);  // Find original
        var cat2 = found.Key;
        cat2 = _dm.GetParticipantCategories().FirstOrDefault(i => i == cat2); // Check if already in DataModel

        if (cat2 != null)
        { // Update existing one
          cat2.Name = cat1.Name;
          cat2.PrettyName = cat1.PrettyName;
          cat2.Synonyms = cat1.Synonyms;
          cat2.SortPos = curSortPos;
        }
        else
        { // Create new one
          var catNew = new ParticipantCategory(cat1.Name, cat1.PrettyName, curSortPos, cat1.Synonyms);
          _dm.GetParticipantCategories().Add(catNew);
          // Remove any old reference and replace with new one
          if (found.Key != null) _category2Category.Remove(found.Key);
          _category2Category.Add(catNew, cat1);
        }

        curSortPos++;
      }
    }


    private bool categoriesDifferent()
    {
      // *** Check removed one
      foreach (var cat2 in _dm.GetParticipantCategories())
      {
        ParticipantCategory cat1 = null;
        _category2Category.TryGetValue(cat2, out cat1);
        if (cat1 == null || CategoryViewModel.Items.FirstOrDefault(i => i == cat1) == null)
          return true;
      }

      // *** Check updated & new ones
      uint curSortPos = 1;
      foreach (var cat1 in CategoryViewModel.Items)
      {
        var found = _category2Category.FirstOrDefault(i => i.Value == cat1);  // Find original
        var cat2 = found.Key;
        cat2 = _dm.GetParticipantCategories().FirstOrDefault(i => i == cat2); // Check if already in DataModel

        if (cat2 != null)
        { // Check updated
          if (cat2.Name != cat1.Name || cat2.PrettyName != cat1.PrettyName || cat2.Synonyms != cat1.Synonyms || cat2.SortPos != curSortPos)
            return true;
        }
        else
          return true;

        curSortPos++;
      }

      return false;
    }


    private void storeClasses()
    {
      // Delete removed one
      List<ParticipantClass> toDelete = new List<ParticipantClass>();
      foreach (var c2 in _dm.GetParticipantClasses())
      {
        ParticipantClass c1 = null;
        _class2Class.TryGetValue(c2, out c1);
        if (c1 == null || ClassViewModel.Items.FirstOrDefault(i => i == c1) == null)
          toDelete.Add(c2);
      }
      foreach (var c in toDelete)
        _dm.GetParticipantClasses().Remove(c);

      // Update & create new ones
      uint curSortPos = 1;
      foreach (var c1 in ClassViewModel.Items)
      {
        var found = _class2Class.FirstOrDefault(i => i.Value == c1);
        var c2 = found.Key;
        c2 = _dm.GetParticipantClasses().FirstOrDefault(i => i == c2);

        var g2 = _group2Group.FirstOrDefault(i => i.Value == c1.Group);
        var cat2 = _category2Category.FirstOrDefault(i => i.Value == c1.Sex);

        if (c2 != null)
        { // Update existing one
          c2.Name = c1.Name;
          c2.Group = g2.Key;
          c2.Sex = cat2.Key;
          c2.Year = c1.Year;
          c2.SortPos = curSortPos;
        }
        else
        { // Create new one
          var cNew = new ParticipantClass(null, g2.Key, c1.Name, cat2.Key, c1.Year, curSortPos);
          _dm.GetParticipantClasses().Add(cNew);
          // Remove any old reference and replace with new one
          if (found.Key != null) _class2Class.Remove(found.Key);
          _class2Class.Add(cNew, c1);
        }

        curSortPos++;
      }
    }

    private bool classesDifferent()
    {
      // Check removed one
      foreach (var c2 in _dm.GetParticipantClasses())
      {
        ParticipantClass c1 = null;
        _class2Class.TryGetValue(c2, out c1);
        if (c1 == null || ClassViewModel.Items.FirstOrDefault(i => i == c1) == null)
          return true;
      }

      // Check update & new ones
      uint curSortPos = 1;
      foreach (var c1 in ClassViewModel.Items)
      {
        var found = _class2Class.FirstOrDefault(i => i.Value == c1);
        var c2 = found.Key;
        c2 = _dm.GetParticipantClasses().FirstOrDefault(i => i == c2);

        var g2 = _group2Group.FirstOrDefault(i => i.Value == c1.Group);
        var cat2 = _category2Category.FirstOrDefault(i => i.Value == c1.Sex);

        if (c2 != null)
        { // Update existing one
          if (c2.Name != c1.Name
              || c2.Group != g2.Key
              || c2.Sex != cat2.Key
              || c2.Year != c1.Year
              || c2.SortPos != curSortPos)
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