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
    ClassesAndGroupsViewModel _cgVM;

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
      _cgVM = new ClassesAndGroupsViewModel(_dm);
      DataContext = _cgVM; 
    }
  }



  public class ClassesAndGroupsViewModel
  {
    public GroupViewModel GroupViewModel { get; }
    public ClassViewModel ClassViewModel { get; }


    public ClassesAndGroupsViewModel(AppDataModel dm)
    {
      GroupViewModel = new GroupViewModel(dm.GetParticipantGroups());
      ClassViewModel = new ClassViewModel(dm.GetParticipantClasses());
    }

  }


  public class GroupViewModel : IDropTarget
  {
    
    public ObservableCollection<ParticipantGroup> Items { get; }

    CollectionViewSource _itemsWONewItem; //!< Just there to fill the comboboxes in the DataGrid for the classes, otherwise the "new items placeholder" will appear
    public System.ComponentModel.ICollectionView FilteredItems { get { return _itemsWONewItem.View; } }

    public GroupViewModel(IList<ParticipantGroup> groups)
    {
      Items = new ObservableCollection<ParticipantGroup>(groups);

      _itemsWONewItem = new CollectionViewSource();
      _itemsWONewItem.Source = Items;
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

    public ClassViewModel(IList<ParticipantClass> classes)
    {
      Items = new ObservableCollection<ParticipantClass>(classes);
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
