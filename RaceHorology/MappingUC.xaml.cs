using RaceHorologyLib;
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
  /// <summary>
  /// Interaction logic for MappingUC.xaml
  /// </summary>
  public partial class MappingUC : UserControl
  {

    Mapping _mapping;

    public MappingUC()
    {
      InitializeComponent();
    }

    public Mapping Mapping 
    { 
      set 
      { 
        _mapping = value;
        setupDataGrid();
      } 

      get 
      { 
        return _mapping; 
      } 
    }

    private void setupDataGrid()
    {
      dgMapping.ItemsSource = _mapping.MappingList;
    }
  }
}
