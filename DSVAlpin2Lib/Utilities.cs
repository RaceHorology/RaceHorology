using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{

  public class ItemsChangedNotifier : INotifyCollectionChanged
  {
    INotifyCollectionChanged _source;

    public bool CollectionResetIfItemChanged { get; set; } = false;

    public ItemsChangedNotifier(INotifyCollectionChanged source)
    {
      _source = source;

      _source.CollectionChanged += OnCollectionChanged;
    }

    ~ItemsChangedNotifier()
    {
      _source.CollectionChanged -= OnCollectionChanged;
    }


    public INotifyCollectionChanged Source { get { return _source; } }



    public delegate void ItemChangedEventHandler(object sender, PropertyChangedEventArgs e);
    /// <summary>
    /// If an item of the collection changed its properties this event triggers
    /// </summary>
    public event ItemChangedEventHandler ItemChanged;



    public event NotifyCollectionChangedEventHandler CollectionChanged;



    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems!=null)
        foreach (INotifyPropertyChanged item in e.OldItems)
          item.PropertyChanged -= ItemPropertyChanged;

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
          item.PropertyChanged += ItemPropertyChanged;

      NotifyCollectionChangedEventHandler handler = CollectionChanged;
      handler?.Invoke(sender, e);
    }


    private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      ItemChangedEventHandler handler = ItemChanged;
      handler?.Invoke(sender, e);

      if (CollectionResetIfItemChanged)
      {
        NotifyCollectionChangedEventHandler handlerCC = CollectionChanged;
        handlerCC?.Invoke(sender, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
      }
    }

  }


  public class ItemsChangeObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
  {
    /// <summary>
    /// Specifies whether the collection itself shall be reset and trigger a CollectionChanged event in case an item's property changed
    /// </summary>
    public bool CollectionResetIfItemChanged { get; set; } = false;


    public delegate void ItemChangedEventHandler(object sender, PropertyChangedEventArgs e);
    /// <summary>
    /// If an item of the collection changed its properties this event triggers
    /// </summary>
    public event ItemChangedEventHandler ItemChanged;

    protected override void ClearItems()
    {
      foreach (var item in Items) item.PropertyChanged -= ItemPropertyChanged;
      base.ClearItems();
    }

    protected override void InsertItem(int index, T item)
    {
      item.PropertyChanged += ItemPropertyChanged;
      base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
      this[index].PropertyChanged -= ItemPropertyChanged;
      base.RemoveItem(index);
    }

    protected override void SetItem(int index, T item)
    {
      this[index].PropertyChanged -= ItemPropertyChanged;
      item.PropertyChanged += ItemPropertyChanged;
      base.SetItem(index, item);
    }

    void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      ItemChangedEventHandler handler = ItemChanged;
      handler?.Invoke(sender, e);

      if (CollectionResetIfItemChanged)
        OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
    }
  }


  // Define other methods and classes here
  public static class Extensions
  {
    public static DateTime AddMicroseconds(this DateTime datetime, Int32 value)
    {
      return new DateTime(datetime.Ticks + value*10, datetime.Kind);
    }


    public static TimeSpan AddMicroseconds(this TimeSpan timespan, Int32 value)
    {
      return new TimeSpan(timespan.Ticks + value*10);
    }
  }
}
