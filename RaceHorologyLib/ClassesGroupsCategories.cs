using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    public void Merge(IList<ParticipantCategory> categories)
    {
      foreach (var c in categories)
      {
        if (!Items.Contains(c))
          Items.Add(c);
      }
      Items.Sort(new StdComparer());
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


    public void Merge(IList<ParticipantGroup> groups)
    {
      foreach (var g in groups)
      {
        if (!Items.Contains(g))
          Items.Add(g);
      }
      Items.Sort(new StdComparer());
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


    public void Assign(IList<ParticipantClass> groups)
    {
      Items.Clear();
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
    }


    public void Add(IList<ParticipantClass> groups)
    {
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
    }


    public void Merge(IList<ParticipantClass> groups)
    {
      foreach (var g in groups)
      {
        if (!Items.Contains(g))
          Items.Add(g);
      }
      Items.Sort(new StdComparer());
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


    public void Import(AppDataModel srcModel)
    {
      var srcGroups = srcModel.GetParticipantGroups();
      var srcClasses = srcModel.GetParticipantClasses();
      var srcCategories = srcModel.GetParticipantCategories();

      List<ParticipantGroup> dstGroups = new List<ParticipantGroup>();
      List<ParticipantClass> dstClasses = new List<ParticipantClass>();
      List<ParticipantCategory> dstCategories = new List<ParticipantCategory>();

      foreach (var g1 in srcGroups)
      {
        ParticipantGroup g2 = null;
        if (!_group2Group.TryGetValue(g1, out g2))
        {
          g2 = new ParticipantGroup(g1.Id, g1.Name, g1.SortPos);
          _group2Group.Add(g1, g2);
          dstGroups.Add(g2);
        }
      }

      foreach (var cat1 in srcCategories)
      {
        ParticipantCategory cat2 = null;
        if (!_category2Category.TryGetValue(cat1, out cat2))
        {
          cat2 = new ParticipantCategory(cat1.Name, cat1.PrettyName, cat1.SortPos, cat1.Synonyms);
          _category2Category.Add(cat1, cat2);
          dstCategories.Add(cat2);
        }
      }

      foreach (var c1 in srcClasses)
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
          dstClasses.Add(c2);
        }
      }

      GroupViewModel.Add(dstGroups);
      ClassViewModel.Add(dstClasses);
      CategoryViewModel.Add(dstCategories);
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
      // Delete removed one
      List<ParticipantGroup> toDelete = new List<ParticipantGroup>();
      foreach (var g2 in _dm.GetParticipantGroups())
      {
        var g1 = _group2Group[g2];

        if (GroupViewModel.Items.FirstOrDefault(i => i == g1) == null)
          toDelete.Add(g2);
      }
      foreach (var g in toDelete)
        _dm.GetParticipantGroups().Remove(g);

      // Update & create new ones
      uint curSortPos = 1;
      foreach (var g1 in GroupViewModel.Items)
      {
        var g2 = _group2Group.FirstOrDefault(i => i.Value == g1);

        if (g2.Key != null)
        {
          // Update existing one
          g2.Key.Name = g1.Name;
          g2.Key.SortPos = curSortPos;
        }
        else
        {
          // Create new one
          var gNew = new ParticipantGroup(null, g1.Name, curSortPos);
          _dm.GetParticipantGroups().Add(gNew);
          _group2Group.Add(gNew, g1);
        }

        curSortPos++;
      }
    }


    private void storeCategories()
    {
      // Delete removed one
      List<ParticipantCategory> toDelete = new List<ParticipantCategory>();
      foreach (var cat2 in _dm.GetParticipantCategories())
      {
        var cat1 = _category2Category[cat2];

        if (CategoryViewModel.Items.FirstOrDefault(i => i == cat1) == null)
          toDelete.Add(cat2);
      }
      foreach (var cat in toDelete)
        _dm.GetParticipantCategories().Remove(cat);

      // Update & create new ones
      uint curSortPos = 1;
      foreach (var cat1 in CategoryViewModel.Items)
      {
        var cat2 = _category2Category.FirstOrDefault(i => i.Value == cat1);

        if (cat2.Key != null)
        {
          // Update existing one
          cat2.Key.Name = cat1.Name;
          cat2.Key.PrettyName = cat1.PrettyName;
          cat2.Key.Synonyms = cat1.Synonyms;
          cat2.Key.SortPos = curSortPos;
        }
        else
        {
          // Create new one
          var catNew = new ParticipantCategory(cat1.Name, cat1.PrettyName, curSortPos, cat1.Synonyms);
          _dm.GetParticipantCategories().Add(catNew);
          _category2Category.Add(catNew, cat1);
        }

        curSortPos++;
      }
    }


    private void storeClasses()
    {
      // Delete removed one
      List<ParticipantClass> toDelete = new List<ParticipantClass>();
      foreach (var c2 in _dm.GetParticipantClasses())
      {
        var c1 = _class2Class[c2];

        if (ClassViewModel.Items.FirstOrDefault(i => i == c1) == null)
          toDelete.Add(c2);
      }
      foreach (var c in toDelete)
        _dm.GetParticipantClasses().Remove(c);

      // Update & create new ones
      uint curSortPos = 1;
      foreach (var c1 in ClassViewModel.Items)
      {
        var c2 = _class2Class.FirstOrDefault(i => i.Value == c1);
        var g2 = _group2Group.FirstOrDefault(i => i.Value == c1.Group);
        var cat2 = _category2Category.FirstOrDefault(i => i.Value == c1.Sex);

        if (c2.Key != null)
        {

          // Update existing one
          c2.Key.Name = c1.Name;
          c2.Key.Group = g2.Key;
          c2.Key.Sex = cat2.Key;
          c2.Key.Year = c1.Year;
          c2.Key.SortPos = curSortPos;
        }
        else
        {
          // Create new one
          var cNew = new ParticipantClass(null, g2.Key, c1.Name, cat2.Key, c1.Year, curSortPos);
          _dm.GetParticipantClasses().Add(cNew);
          _class2Class.Add(cNew, c1);
        }

        curSortPos++;
      }
    }
  }
}