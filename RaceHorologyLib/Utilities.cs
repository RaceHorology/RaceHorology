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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class ItemsChangedNotifier : INotifyCollectionChanged, IDisposable
  {
    INotifyCollectionChanged _source;
    List<INotifyPropertyChanged> _observedItems;

    public bool CollectionResetIfItemChanged { get; set; } = false;

    public ItemsChangedNotifier(INotifyCollectionChanged source)
    {
      _observedItems = new List<INotifyPropertyChanged>();

      _source = source;

      _source.CollectionChanged += OnCollectionChanged;

      // Populate initially
      Initialize();
    }

    ~ItemsChangedNotifier()
    {
      Dispose(false);
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
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          _observedItems.Remove(item);
          item.PropertyChanged -= ItemPropertyChanged;
        }
      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          _observedItems.Add(item);
          item.PropertyChanged += ItemPropertyChanged;

          ItemChangedEventHandler handler2 = ItemChanged;
          handler2?.Invoke(item, null);//e = {System.ComponentModel.PropertyChangedEventArgs}
        }
      NotifyCollectionChangedEventHandler handler = CollectionChanged;
      handler?.Invoke(sender, e);
    }

    private void Initialize()
    {
      System.Collections.IEnumerable items = _source as System.Collections.IEnumerable;
      if (items != null)
        foreach (var item in items)
        {
          INotifyPropertyChanged item2 = (INotifyPropertyChanged)item;
          _observedItems.Add(item2);
          item2.PropertyChanged += ItemPropertyChanged;
        }
    }


    private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      ItemChangedEventHandler handler = ItemChanged;
      handler?.Invoke(sender, e);

      if (CollectionResetIfItemChanged)
      {
        NotifyCollectionChangedEventHandler handlerCC = CollectionChanged;
        handlerCC?.Invoke(
          sender, 
          new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
      }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _source.CollectionChanged -= OnCollectionChanged;
          _source = null;

          foreach (var it in _observedItems)
            it.PropertyChanged -= ItemPropertyChanged;

        }

        disposedValue = true;
      }
    }


    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion

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



  public class CopyObservableCollection<TC, T> : ObservableCollection<TC> where T : class where TC : class
  {
    protected ObservableCollection<T> _source;
    protected Cloner<TC,T> _cloner;
    protected bool _cloneOnPropertyChanged;

    public delegate TClone Cloner<TClone,TSource>(TSource source);
    public CopyObservableCollection(ObservableCollection<T> source, Cloner<TC,T> cloner, bool cloneOnPropertyChanged)
    {
      _source = source;
      _cloner = cloner;
      _cloneOnPropertyChanged = cloneOnPropertyChanged;

      _source.CollectionChanged += OnCollectionChanged;

      FillInitially();
    }

    ~CopyObservableCollection()
    {
      _source.CollectionChanged -= OnCollectionChanged;
    }


    protected void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (_cloneOnPropertyChanged && (sender is T sourceItem))
      {
        int index = _source.IndexOf(sourceItem);
        var clonedItem = _cloner(sourceItem);
        SetItem(index, clonedItem);
      }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      // Unhook from PropertyChanged
      if (e.OldItems != null)
        foreach (T item in e.OldItems)
        {
          if (item is INotifyPropertyChanged item2)
            item2.PropertyChanged -= ItemPropertyChanged;
        }

      // Sync Lists
      int i,j;
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          i = 0;
          foreach (T item in e.NewItems)
          {
            Insert(e.NewStartingIndex+i, _cloner(item));
            i++;
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          for (i = 0; i < e.OldItems.Count; i++)
            RemoveAt(e.OldStartingIndex + i);
          break;
        case NotifyCollectionChangedAction.Replace:
          throw new NotImplementedException();
        case NotifyCollectionChangedAction.Reset:
          Clear();

          List<TC> toInsert = new List<TC>();
          foreach (T item in _source)
            toInsert.Add(_cloner(item));
          this.InsertRange(toInsert);

          break;
        case NotifyCollectionChangedAction.Move:
          for (i = e.OldStartingIndex, j = e.NewStartingIndex; i < e.OldItems.Count; i++, j++)
            Move(i, j);
          break;
      }

      // Hook to PropertyChanged
      if (e.NewItems != null)
        foreach (T item in e.NewItems)
        {
          if (item is INotifyPropertyChanged item2)
            item2.PropertyChanged += ItemPropertyChanged;
        }
    }

    private void FillInitially()
    {
      for(int i=0; i<_source.Count(); i++)
        Add(_cloner(_source[i]));
    }

  }


  public class FilterObservableCollection<T> : ObservableCollection<T>
  {
    protected Func<T, bool> _predicate;
    protected ObservableCollection<T> _source;
    protected IComparer<T> _compare;

    public FilterObservableCollection(ObservableCollection<T> source, Func<T, bool> predicate, IComparer<T> compare)
    {
      _source = source;
      _predicate = predicate;
      _compare = compare;

      _source.CollectionChanged += onSource_CollectionChanged;
      
      copyItems();
    }

    private void copyItems()
    {
      if (_compare != null)
        this.InsertRange(_source.Where(_predicate).OrderBy(v => v, _compare));
      else
        this.InsertRange(_source.Where(_predicate));
    }

    ~FilterObservableCollection()
    {
      _source.CollectionChanged -= onSource_CollectionChanged;
    }

    protected void onSource_ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is T sourceItem)
      {
        if (_predicate(sourceItem))
        {
          if (IndexOf(sourceItem) == -1)
            if (_compare != null)
              this.InsertSorted<T>(sourceItem, _compare);
            else
              this.Add(sourceItem);
        }
        else
        {
          if (IndexOf(sourceItem) != -1)
            Remove(sourceItem);
        }
      }
    }


    private void onSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      // Unhook from PropertyChanged
      if (e.OldItems != null)
        foreach (T item in e.OldItems)
        {
          if (item is INotifyPropertyChanged item2)
            item2.PropertyChanged -= onSource_ItemPropertyChanged;
        }

      // Sync Lists
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Move:
          ClearItems();
          copyItems();
          break;
      }

      // Hook to PropertyChanged
      if (e.NewItems != null)
        foreach (T item in e.NewItems)
        {
          if (item is INotifyPropertyChanged item2)
            item2.PropertyChanged += onSource_ItemPropertyChanged;
        }
    }
  }

  public static class ObservableCollectionExtensions
  {
    /// <summary>
    /// Inserts a new element in a sorted collection
    /// </summary>
    /// <typeparam name="TC"></typeparam>
    /// <typeparam name="TI"></typeparam>
    /// <param name="col"></param>
    /// <param name="item"></param>
    /// <param name="comparer"></param>
    public static void InsertSorted<TC>(this Collection<TC> collection, TC item, System.Collections.Generic.IComparer<TC> comparer)
    {
      // Find right position and insert
      int i = 0;
      for (; i < collection.Count(); ++i)
        if (comparer.Compare(item, collection.ElementAt(i)) < 0)
          break;

      // Not yet inserted, insert at the end
      collection.Insert(i, item);
    }

    public static void InsertSorted<TC>(this Collection<TC> collection, TC[] items, IComparer<TC> comparer)
    {
      foreach (TC item in items)
        collection.InsertSorted(item, comparer);

    }

    /// <summary>
    /// Sorts the collection in-place
    /// </summary>
    /// <typeparam name="TC"></typeparam>
    /// <param name="collection">The collection to sort.</param>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="first">The first element to start the sort process. Specifiy 0 if from start.</param>
    /// <param name="last">The last element to include into the sort. Specify -1 for the last element.</param>
    public static void Sort<TC>(this Collection<TC> collection, IComparer<TC> comparer, int first = 0, int last = -1)
    {
      int firstElement = 0;
      int lastElement= collection.Count - 1;

      firstElement = Math.Min(first, collection.Count - 1);

      if (last >= 0)
        lastElement = Math.Min(last, collection.Count - 1);

      int n = lastElement;
      bool swapped;
      do
      {
        swapped = false;
        for (int i = firstElement; i < n; ++i)
        {
          if (comparer.Compare(collection.ElementAt(i), collection.ElementAt(i + 1)) > 0)
          {
            TC temp = collection.ElementAt(i);
            collection.RemoveAt(i);
            collection.Insert(i + 1, temp);
            swapped = true;
          }
        }
        n--;
      }
      while (swapped);
    }


    public static void InsertRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
      var enumerable = items as List<T> ?? items.ToList();
      if (collection == null || items == null || !enumerable.Any())
      {
        return;
      }

      Type type = collection.GetType();

      type.InvokeMember("CheckReentrancy", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, collection, null);
      var itemsProp = type.BaseType.GetProperty("Items", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
      var privateItems = itemsProp.GetValue(collection) as IList<T>;
      foreach (var item in enumerable)
      {
        privateItems.Add(item);
      }

      type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
        collection, new object[] { new PropertyChangedEventArgs("Count") });

      type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
        collection, new object[] { new PropertyChangedEventArgs("Item[]") });

      type.InvokeMember("OnCollectionChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
        collection, new object[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) });
    }

  }



  public static class ViewUtilities
  {

    /// <summary>
    /// Extension method to convert a ICollectionView to a List
    /// </summary>
    public static IList<T> ViewToList<T>(this System.ComponentModel.ICollectionView view)
    {
      IList<T> resList = new List<T>();

      var lr = view as System.Windows.Data.ListCollectionView;
      if (view.Groups != null)
      {
        foreach (var group in view.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;
          // Group(Name) would be: cvGroup.Name.ToString()

          foreach (var item in cvGroup.Items)
            resList.Add((T)item);
        }
      }
      else
        foreach (var item in view.SourceCollection)
          resList.Add((T)item);

      return resList;
    }
  }

  public static class PropertyUtilities
  {
    public static object GetPropertyValue(object obj, string propertyName)
    {
      if (propertyName == null || obj == null)
        return null;

      foreach (string part in propertyName.Split('.'))
      {
        if (obj == null) { return null; }

        Type type = obj.GetType();
        System.Reflection.PropertyInfo info = type.GetProperty(part);
        if (info == null) { return null; }

        obj = info.GetValue(obj, null);
      }
      return obj;
    }

    public static bool SetPropertyValue(object obj, string propertyName, object value)
    {
      if (propertyName == null || obj == null)
        return false;

      System.Reflection.PropertyInfo info = null;
      foreach (string part in propertyName.Split('.'))
      {
        if (obj == null) { return false; }

        Type type = obj.GetType();
        info = type.GetProperty(part);
        if (info == null) { return false; }
      }

      if (info == null) 
        return false;
  
      info.SetValue(obj, Convert.ChangeType(value, info.PropertyType));
      return true;
    }

    public static object GetPropertyValue(object obj, string propertyName, object defaultValue)
    {
      object val = GetPropertyValue(obj, propertyName);
      if (val == null)
        val = defaultValue;

      return val;
    }

  }



  public struct RoundedTimeSpan
  {
    public enum ERoundType { Round, Floor };

    private const int TIMESPAN_SIZE = 7; // it always has seven digits

    private TimeSpan roundedTimeSpan;
    private int precision;
    private ERoundType roundType;

    public RoundedTimeSpan(TimeSpan time, int precision, ERoundType roundType)
    {
      long ticks = time.Ticks;

      if (precision < 0) { throw new ArgumentException("precision must be non-negative"); }
      this.precision = precision;
      this.roundType = roundType;
      int factor = (int)System.Math.Pow(10, (TIMESPAN_SIZE - precision));

      // This is only valid for rounding milliseconds-will *not* work on secs/mins/hrs!
      // Note: FIS TimeSpan is cut-off not rounded
      if (this.roundType == ERoundType.Floor)
        roundedTimeSpan = new TimeSpan(((long)System.Math.Floor((1.0 * ticks / factor)) * factor));
      else
        roundedTimeSpan = new TimeSpan(((long)System.Math.Round((1.0 * ticks / factor)) * factor));
    }

    public TimeSpan TimeSpan { get { return roundedTimeSpan; } }

    public override string ToString()
    {
      return ToString(precision);
    }

    public string ToString(int length)
    { // this method revised 2010.01.31
      int digitsToStrip = TIMESPAN_SIZE - length;
      string s = roundedTimeSpan.ToString();
      if (!s.Contains(".") && length == 0) { return s; }
      if (!s.Contains(".")) { s += "." + new string('0', TIMESPAN_SIZE); }
      int subLength = s.Length - digitsToStrip;
      return subLength < 0 ? "" : subLength > s.Length ? s : s.Substring(0, subLength);
    }
  }



  // Define other methods and classes here
  public static class TimeSpanExtensions
  {

    public static DateTime AddMicroseconds(this DateTime datetime, Int32 value)
    {
      return new DateTime(datetime.Ticks + value * 10, datetime.Kind);
    }


    public static TimeSpan AddMicroseconds(this TimeSpan timespan, Int32 value)
    {
      return new TimeSpan(timespan.Ticks + value * 10);
    }


    public static TimeSpan? ParseTimeSpan(string text)
    {
      TimeSpan? time = null;
      try
      {
        string[] formats = {
            @"ss\.ffff", @"ss\.fff", @"ss\.ff", @"ss\.f",
            @"mm\:ss\.ffff", @"mm\:ss\.fff", @"mm\:ss\.ff", @"mm\:ss\.f",
            @"m\:ss\.ffff", @"m\:ss\.fff", @"m\:ss\.ff", @"m\:ss\.f",
            @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f",
            @"h\:mm\:ss\.ffff", @"h\:mm\:ss\.fff", @"h\:mm\:ss\.ff", @"h\:mm\:ss\.f",
            @"ss\,ffff", @"ss\,fff", @"ss\,ff", @"ss\,f",
            @"mm\:ss\,ffff", @"mm\:ss\,fff", @"mm\:ss\,ff", @"mm\:ss\,f",
            @"m\:ss\,ffff", @"m\:ss\,fff", @"m\:ss\,ff", @"m\:ss\,f",
            @"hh\:mm\:ss\,ffff", @"hh\:mm\:ss\,fff", @"hh\:mm\:ss\,ff", @"hh\:mm\:ss\,f",
            @"h\:mm\:ss\,ffff", @"h\:mm\:ss\,fff", @"h\:mm\:ss\,ff", @"h\:mm\:ss\,f"
          };
        text = text.TrimEnd(' ');
        time = TimeSpan.ParseExact(text, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException)
      { }


      if (time == null) // treat as seconds
      {
        try
        {
          double value = 0.0;
          text = text.Replace(',', '.');
          value = double.Parse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
          long ticks = (long)(value * 10000000);
          time = new TimeSpan(ticks);
        }
        catch (Exception)
        { }
      }

      return time;
    }


    public static string ToRaceTimeString(this TimeSpan? time, RoundedTimeSpan.ERoundType roundType = RoundedTimeSpan.ERoundType.Floor, string formatString = null)
    {
      if (time == null)
        return "";

      TimeSpan time2 = (TimeSpan)time;

      bool bNegative = time2 < TimeSpan.Zero;
      if (bNegative)
        time2 = new TimeSpan(time2.Ticks * -1);

      RoundedTimeSpan roundedTimeSpan = new RoundedTimeSpan(time2, 2, roundType);

      string str = string.Empty;
      if (formatString == "s")
        str = roundedTimeSpan.TimeSpan.ToString(@"s\,ff");
      else if (formatString == "m")
        str = roundedTimeSpan.TimeSpan.ToString(@"m\:ss\,ff");
      else if (formatString == "mm")
        str = roundedTimeSpan.TimeSpan.ToString(@"mm\:ss\,ff");
      
      else if (roundedTimeSpan.TimeSpan < new TimeSpan(0,1,0))
        str = roundedTimeSpan.TimeSpan.ToString(@"s\,ff");
      else if (roundedTimeSpan.TimeSpan < new TimeSpan(1, 0, 0))
        str = roundedTimeSpan.TimeSpan.ToString(@"m\:ss\,ff");
      else
        str = roundedTimeSpan.TimeSpan.ToString(@"hh\:mm\:ss\,ff");

      if (bNegative)
        return "-"  + str;
      else
        return str;
    }
  }


  public class NullEnabledComparer : System.Collections.Generic.IComparer<IComparable>
  {
    public int Compare(IComparable x, IComparable y)
    {
      if (x == null && y == null)
        return 0;

      if (x == null && y != null)
        return 1;

      if (x != null && y == null)
        return -1;

      return x.CompareTo(y);
    }
  }


  public class StdComparer : System.Collections.Generic.IComparer<IComparable>
  {
    public int Compare(IComparable x, IComparable y)
    {
      return x.CompareTo(y);
    }
  }


  public static class CollectionViewGroupUtils
  {
    public static string GetName(this System.Windows.Data.CollectionViewGroup cvGroup)
    {
      string groupName = string.Empty;
      if (cvGroup?.Name != null)
        groupName = cvGroup.Name.ToString();

      return groupName;
    }

  }


  public static class StringUtils
  {
    public static string ToStringOrEmpty(this object o)
    {
      if (o == null)
        return "";
      return o.ToString();
    }
  }



  public class Singleton<T> where T : class, new()
  {
    private Singleton() { }

    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());

    public static T Instance { get { return instance.Value; } }
  }
}
