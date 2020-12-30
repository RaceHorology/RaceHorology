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
        _cgVM.Clear();
        _cgVM.Import(importModel);
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
        _cgVM.Import(importModel);
      }
    }
  }



  public class ClassesAndGroupsEditViewModel : ClassesGroupsCategoriesEditVM
  {

    public GroupViewModelDD GroupViewModelDD { get; }
    public ClassViewModelDD ClassViewModelDD { get; }
    public CategoryViewModelDD CategoryViewModelDD { get; }


    public ClassesAndGroupsEditViewModel(AppDataModel dm) : base(dm)
    {
      GroupViewModelDD = new GroupViewModelDD(GroupViewModel);
      ClassViewModelDD = new ClassViewModelDD(ClassViewModel);
      CategoryViewModelDD = new CategoryViewModelDD(CategoryViewModel);
    }
  }


  public class GroupViewModelDD : IDropTarget
  {
    GroupVM _viewModel;
    public GroupViewModelDD(GroupVM viewModel)
    {
      _viewModel = viewModel;
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
        var iSource = _viewModel.Items.IndexOf(source);
        var iTarget = _viewModel.Items.IndexOf(target);

        if (iSource != iTarget)
          _viewModel.Items.Move(iSource, iTarget);
      }
    }
  }


  public class CategoryViewModelDD : IDropTarget
  {
    CategoryVM _viewModel;
    public CategoryViewModelDD(CategoryVM viewModel) : base()
    {
      _viewModel = viewModel;
    }

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
        var iSource = _viewModel.Items.IndexOf(source);
        var iTarget = _viewModel.Items.IndexOf(target);

        if (iSource != iTarget)
          _viewModel.Items.Move(iSource, iTarget);
      }
    }
  }



  public class ClassViewModelDD : IDropTarget
  {
    ClassVM _viewModel;
    public ClassViewModelDD(ClassVM viewModel) : base()
    {
      _viewModel = viewModel;
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
        var iSource = _viewModel.Items.IndexOf(source);
        var iTarget = _viewModel.Items.IndexOf(target);

        if (iSource != iTarget)
          _viewModel.Items.Move(iSource, iTarget);
      }
    }
  }





}
