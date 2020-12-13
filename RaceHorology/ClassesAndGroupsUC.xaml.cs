/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
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

    private void BtnImport_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter =
        "Race Horology Daten|*.mdb|DSValpin Daten|*.mdb";
      if (openFileDialog.ShowDialog() == true)
      {
        Database importDB = new Database();
        importDB.Connect(openFileDialog.FileName);
        AppDataModel importModel = new AppDataModel(importDB);
        _cgVM.Import(importModel, true);
      }
    }

    private void BtnImportAdd_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter =
        "Race Horology Daten|*.mdb|DSValpin Daten|*.mdb";
      if (openFileDialog.ShowDialog() == true)
      {
        Database importDB = new Database();
        importDB.Connect(openFileDialog.FileName);
        AppDataModel importModel = new AppDataModel(importDB);
        _cgVM.Import(importModel, false);
      }
    }
  }



  public class ClassesAndGroupsEditViewModel
  {
    private AppDataModel _dm;
    Dictionary<ParticipantGroup, ParticipantGroup> _group2Group;
    Dictionary<ParticipantClass, ParticipantClass> _class2Class;
    Dictionary<ParticipantCategory, ParticipantCategory> _category2Category;


    public GroupViewModel GroupViewModel { get; }
    public ClassViewModel ClassViewModel { get; }
    public CategoryViewModel CategoryViewModel { get; }


    public ClassesAndGroupsEditViewModel(AppDataModel dm)
    {
      _dm = dm;
      _group2Group = new Dictionary<ParticipantGroup, ParticipantGroup>();
      _class2Class = new Dictionary<ParticipantClass, ParticipantClass>();
      _category2Category = new Dictionary<ParticipantCategory, ParticipantCategory>();

      GroupViewModel = new GroupViewModel();
      ClassViewModel = new ClassViewModel();
      CategoryViewModel = new CategoryViewModel();

      initialize();
    }

    private void initialize()
    {
      _group2Group.Clear();
      _class2Class.Clear();
      _category2Category.Clear();

      var srcGroups = _dm.GetParticipantGroups();
      var srcClasses = _dm.GetParticipantClasses();
      var srcCategories = _dm.GetParticipantCategories();

      List<ParticipantGroup> dstGroups = new List<ParticipantGroup>();
      List<ParticipantClass> dstClasses = new List<ParticipantClass>();
      List<ParticipantCategory> dstCategories = new List<ParticipantCategory>();

      foreach (var g1 in srcGroups)
      {
        var g2 = new ParticipantGroup(g1.Id, g1.Name, g1.SortPos);
        dstGroups.Add(g2);
        _group2Group.Add(g1, g2);
      }

      foreach (var cat1 in srcCategories)
      {
        var cat2 = new ParticipantCategory(cat1.Name, cat1.PrettyName, cat1.SortPos, cat1.Synonyms);
        dstCategories.Add(cat2);
        _category2Category.Add(cat1, cat2);
      }

      foreach (var c1 in srcClasses)
      {
        var c2 = new ParticipantClass(
          c1.Id, 
          c1.Group == null ? null : _group2Group[c1.Group], c1.Name,
          c1.Sex == null   ? null   : _category2Category[c1.Sex], 
          c1.Year, 
          c1.SortPos);
        dstClasses.Add(c2);
        _class2Class.Add(c1, c2);
      }

      GroupViewModel.Assign(dstGroups, true);
      ClassViewModel.Assign(dstClasses, true);
      CategoryViewModel.Assign(dstCategories, true);
    }


    public void Import(AppDataModel srcModel, bool replace)
    {
      var srcGroups = srcModel.GetParticipantGroups();
      var srcClasses = srcModel.GetParticipantClasses();
      var srcCategories = srcModel.GetParticipantCategories();

      List<ParticipantGroup> dstGroups = new List<ParticipantGroup>();
      List<ParticipantClass> dstClasses = new List<ParticipantClass>();
      List<ParticipantCategory> dstCategories = new List<ParticipantCategory>();
      Dictionary<ParticipantGroup, ParticipantGroup> group2Group = new Dictionary<ParticipantGroup, ParticipantGroup>();
      Dictionary<ParticipantCategory, ParticipantCategory> category2Category = new Dictionary<ParticipantCategory, ParticipantCategory>();


      foreach (var g1 in srcGroups)
      {
        var g2 = new ParticipantGroup(g1.Id, g1.Name, g1.SortPos);
        dstGroups.Add(g2);
        group2Group.Add(g1, g2);
      }

      foreach (var cat1 in srcCategories)
      {
        var cat2 = new ParticipantCategory(cat1.Name, cat1.PrettyName, cat1.SortPos, cat1.Synonyms);
        dstCategories.Add(cat2);
        category2Category.Add(cat1, cat2);
      }

      foreach (var c1 in srcClasses)
      {
        var c2 = new ParticipantClass(
          c1.Id, 
          c1.Group == null ? null : group2Group[c1.Group], 
          c1.Name,
          c1.Sex == null ? null : category2Category[c1.Sex], 
          c1.Year, 
          c1.SortPos);
        dstClasses.Add(c2);
      }

      GroupViewModel.Assign(dstGroups, replace);
      ClassViewModel.Assign(dstClasses, replace);
      CategoryViewModel.Assign(dstCategories, replace);
    }


    public void Reset()
    {
      initialize();
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

    public void Assign(IList<ParticipantGroup> groups, bool delete)
    {
      if (delete)
        Items.Clear();
      Items.InsertRange(groups);
      Items.Sort(new StdComparer());
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


  public class CategoryViewModel : CategoryVM, IDropTarget
  {
    public CategoryViewModel() : base()
    {}

    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
      ParticipantCategory source = dropInfo.Data as ParticipantCategory;
      ParticipantCategory target = dropInfo.TargetItem as ParticipantCategory;
      if (source != null && target != null)
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    void IDropTarget.Drop(IDropInfo dropInfo)
    {
      ParticipantCategory source = dropInfo.Data as ParticipantCategory;
      ParticipantCategory target = dropInfo.TargetItem as ParticipantCategory;

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

    public void Assign(IList<ParticipantClass> classes, bool delete)
    {
      if (delete)
        Items.Clear();
      Items.InsertRange(classes);
      Items.Sort(new StdComparer());
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
