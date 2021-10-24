using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceHorology
{
  public interface ISaveOrReset
  {
    bool IsSaveNeeded();

    void Save();
    void Reset();
  }


  public class SaveOrReset : ISaveOrReset
  {
    public bool IsSaveNeeded()
    {
      return true;
    }

    public void Reset()
    {
    }

    public void Save()
    {
    }
  }

  /// <summary>
  /// Interaction logic for SaveOrResetUC.xaml
  /// </summary>
  public partial class SaveOrResetUC : UserControl
  {
    private ISaveOrReset _saveOrReset;
    private TabControl _tabControl;
    private TabItem _thisTabItem;
    private bool _active = false;

    public SaveOrResetUC()
    {
      InitializeComponent();
    }

    public void Init(ISaveOrReset saveOrReset, TabControl parent, TabItem thisTabItem)
    {
      _saveOrReset = saveOrReset;
      _tabControl = parent;
      _thisTabItem = thisTabItem;

      var p = thisTabItem.Parent;


      _tabControl.SelectionChanged += parent_SelectionChanged;

      _tabControl.IsVisibleChanged += parent_IsVisibleChanged;
    }


    bool switchAllowed = false;
    private void parent_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_tabControl.SelectedItem == _thisTabItem)
      {
        // Just switched to here
        _active = true;
      }
      else
      {
        // Move away
        if (!switchAllowed)
        {
          e.Handled = true;
          _tabControl.SelectedItem= _thisTabItem;
          MessageBox.Show("Please Save or Cancel your work first.", "Error",
              MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
          _active = false;
      }
    }

    private void parent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      //if (_parent.IsVisible == true)
      //  return;

      //bool saveNeeded = _saveOrReset.IsSaveNeeded();

      //if (saveNeeded)
      //{
      //  var result = MessageBox.Show("Sollen die Änderungen gespeichert werden?", "Speichern?", MessageBoxButton.YesNo, MessageBoxImage.Question);
      //  if (result == MessageBoxResult.Yes)
      //    _saveOrReset.Save();
      //}
    }

    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
      _saveOrReset.Reset();
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
      _saveOrReset.Save();
    }
  }
}
