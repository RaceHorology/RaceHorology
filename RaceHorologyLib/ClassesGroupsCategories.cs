﻿using System;
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
}