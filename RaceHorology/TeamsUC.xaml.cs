/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
  public partial class TeamsUC : UserControl
  {
    AppDataModel _dm;
    TeamsEditViewModel _cgVM;

    public TeamsUC()
    {
      InitializeComponent();
      IsVisibleChanged += TeamsUC_IsVisibleChanged;
    }

    public void Init(AppDataModel dm)
    {
      _dm = dm;

      ucSaveOrReset.Init( "Team- und Teamänderungen", null, null, changed, save, reset);
    }


    private void TeamsUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.OldValue && (bool)e.NewValue)
      {
        // Become visible
        connectDataGrids();
      }
      else if ((bool)e.OldValue && !(bool)e.NewValue)
      {
        // Become invisible
        if (_cgVM != null && !_cgVM.DifferentToDataModel()) {
          _cgVM = null;
        }
      }
    }


    protected void connectDataGrids()
    {
      if (_cgVM == null)
      {
        _cgVM = new TeamsEditViewModel(_dm);
        DataContext = _cgVM; 
      }
    }

    private void reset()
    {
      _cgVM?.Reset();
    }

    private void save()
    {
      _cgVM?.Store();
    }

    private bool changed()
    {
      return _cgVM != null && _cgVM.DifferentToDataModel();
    }

  }



  public class TeamsEditViewModel : TeamsEditVM
  {

    public TeamViewModelDD TeamViewModelDD { get; }
    public TeamGroupViewModelDD TeamGroupViewModelDD { get; }

    public TeamsEditViewModel(AppDataModel dm) : base(dm)
    {
      TeamViewModelDD = new TeamViewModelDD(TeamViewModel);
      TeamGroupViewModelDD = new TeamGroupViewModelDD(TeamGroupViewModel);
    }
  }


  public class TeamGroupViewModelDD : IDropTarget
  {
    TeamGroupVM _viewModel;
    public TeamGroupViewModelDD(TeamGroupVM viewModel)
    {
      _viewModel = viewModel;
    }

    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
      var source = dropInfo.Data as TeamGroup;
      var target = dropInfo.TargetItem as TeamGroup;
      if (source != null && target != null)
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    void IDropTarget.Drop(IDropInfo dropInfo)
    {
      var source = dropInfo.Data as TeamGroup;
      var target = dropInfo.TargetItem as TeamGroup;
      if (source != null && target != null)
      {
        var iSource = _viewModel.Items.IndexOf(source);
        var iTarget = _viewModel.Items.IndexOf(target);

        if (iSource != iTarget)
          _viewModel.Items.Move(iSource, iTarget);
      }
    }
  }


  public class TeamViewModelDD : IDropTarget
  {
    TeamsVM _viewModel;
    public TeamViewModelDD(TeamsVM viewModel) : base()
    {
      _viewModel = viewModel;
    }

    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
      var source = dropInfo.Data as Team;
      var target = dropInfo.TargetItem as Team;
      if (source != null && target != null)
      {
        dropInfo.Effects = DragDropEffects.Move;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
      }
    }

    void IDropTarget.Drop(IDropInfo dropInfo)
    {
      var source = dropInfo.Data as Team;
      var target = dropInfo.TargetItem as Team;

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
