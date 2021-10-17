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


  /// <summary>
  /// Interaction logic for SaveOrResetUC.xaml
  /// </summary>
  public partial class SaveOrResetUC : UserControl
  {
    private ISaveOrReset _saveOrReset;
    private Control _parent;

    public SaveOrResetUC()
    {
      InitializeComponent();
    }

    public void Init(ISaveOrReset saveOrReset, Control parent)
    {
      _saveOrReset = saveOrReset;
      _parent = parent;

      _parent.IsVisibleChanged += parent_IsVisibleChanged;
    }

    private void parent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      bool saveNeeded = _saveOrReset.IsSaveNeeded();

      if (saveNeeded)
      {
        var result = MessageBox.Show("Sollen die Änderungen gespeichert werden?", "Speichern?", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
          _saveOrReset.Save();
      }
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
