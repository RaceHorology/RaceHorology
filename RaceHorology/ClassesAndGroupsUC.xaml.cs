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
  /// Interaction logic for ClassesAndGroupsUC.xaml
  /// </summary>
  public partial class ClassesAndGroupsUC : UserControl
  {
    AppDataModel _dm;

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
      dgClasses.ItemsSource = _dm.GetParticipantClasses();

      dgGroups.ItemsSource = _dm.GetParticipantGroups();

    }

  }
}
