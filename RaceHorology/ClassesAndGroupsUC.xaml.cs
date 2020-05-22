using GongSolutions.Wpf.DragDrop;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for ClassesAndGroupsUC.xaml
  /// </summary>
  public partial class ClassesAndGroupsUC : UserControl
  {
    AppDataModel _dm;
    ClassesAndGroupsEditViewModel _cgVM;

    public ClassesAndGroupsUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm)
    {
      _dm = dm;

      connectDataGrids();
    }

    protected void connectDataGrids()
    {
      _cgVM = new ClassesAndGroupsEditViewModel(_dm);
      DataContext = _cgVM; 
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
      _cgVM.Reset();
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {
      _cgVM.Store();
    }
  }



  public class ClassesAndGroupsEditViewModel
  {
    private AppDataModel _dm;
    Dictionary<ParticipantGroup, ParticipantGroup> _group2Group;
    Dictionary<ParticipantClass, ParticipantClass> _class2Class;


    public GroupViewModel GroupViewModel { get; }
    public ClassViewModel ClassViewModel { get; }


    public ClassesAndGroupsEditViewModel(AppDataModel dm)
    {
      _dm = dm;
      _group2Group = new Dictionary<ParticipantGroup, ParticipantGroup>();
      _class2Class = new Dictionary<ParticipantClass, ParticipantClass>();

      GroupViewModel = new GroupViewModel();
      ClassViewModel = new ClassViewModel();

      initialize();
    }

    private void initialize()
    {
      _group2Group.Clear();
      _class2Class.Clear();

      var srcGroups = _dm.GetParticipantGroups();
      var srcClasses = _dm.GetParticipantClasses();

      List<ParticipantGroup> dstGroups = new List<ParticipantGroup>();
      List<ParticipantClass> dstClasses = new List<ParticipantClass>();

      foreach (var g1 in srcGroups)
      {
        var g2 = new ParticipantGroup(g1.Id, g1.Name, g1.SortPos);
        dstGroups.Add(g2);
        _group2Group.Add(g1, g2);
      }

      foreach (var c1 in srcClasses)
      {
        var c2 = new ParticipantClass(c1.Id, c1.Group == null ? null : _group2Group[c1.Group], c1.Name, c1.Sex, c1.Year, c1.SortPos);
        dstClasses.Add(c2);
        _class2Class.Add(c1, c2);
      }

      GroupViewModel.Assign(dstGroups);
      ClassViewModel.Assign(dstClasses);
    }


    public void Reset()
    {
      initialize();
    }


    public void Store()
    {
      storeGroups();
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

        if (c2.Key != null)
        {

          // Update existing one
          c2.Key.Name = c1.Name;
          c2.Key.Group = g2.Key;
          c2.Key.Sex = c1.Sex;
          c2.Key.Year = c1.Year;
          c2.Key.SortPos = curSortPos;
        }
        else
        {
          // Create new one
          var cNew = new ParticipantClass(null, g2.Key, c1.Name, c1.Sex, c1.Year, curSortPos);
          _dm.GetParticipantClasses().Add(cNew);
          _class2Class.Add(cNew, c1);
        }

        curSortPos++;
      }
    }
  }


  public class GroupViewModel : IDropTarget
  {
    
    public ObservableCollection<ParticipantGroup> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public GroupViewModel()
    {
      Items = new ObservableCollection<ParticipantGroup>();

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
    }

    public void Assign(IList<ParticipantGroup> groups)
    {
      Items.Clear();
      Items.InsertRange(groups);
    }

    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
      ParticipantGroup source = dropInfo.Data as ParticipantGroup;
      ParticipantGroup target = dropInfo.TargetItem as ParticipantGroup;
      if (source != null && target != null)
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    void IDropTarget.Drop(IDropInfo dropInfo)
    {
      ParticipantGroup source = dropInfo.Data as ParticipantGroup;
      ParticipantGroup target = dropInfo.TargetItem as ParticipantGroup;

      if (source != null && target != null)
      {
        var iSource = Items.IndexOf(source);
        var iTarget = Items.IndexOf(target);

        if (iSource != iTarget)
          Items.Move(iSource, iTarget);
      }
    }
  }


  public class ClassViewModel : IDropTarget
  {
    public ObservableCollection<ParticipantClass> Items { get; }

    public ClassViewModel()
    {
      Items = new ObservableCollection<ParticipantClass>();
    }

    public void Assign(IList<ParticipantClass> classes)
    {
      Items.Clear();
      Items.InsertRange(classes);
    }

    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
      ParticipantClass source = dropInfo.Data as ParticipantClass;
      ParticipantClass target = dropInfo.TargetItem as ParticipantClass;
      if (source != null && target != null)
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    void IDropTarget.Drop(IDropInfo dropInfo)
    {
      ParticipantClass source = dropInfo.Data as ParticipantClass;
      ParticipantClass target = dropInfo.TargetItem as ParticipantClass;

      if (source != null && target != null)
      {
        var iSource = Items.IndexOf(source);
        var iTarget = Items.IndexOf(target);

        if (iSource != iTarget)
          Items.Move(iSource, iTarget);
      }
    }
  }





}
