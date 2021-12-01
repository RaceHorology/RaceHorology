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
  /// Interaction logic for ImportTimeUC.xaml
  /// </summary>
  public partial class ImportTimeUC : UserControl
  {

    public event EventHandler Finished;

    public ImportTimeUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      Finished?.Invoke(this, new EventArgs());
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Finished?.Invoke(this, new EventArgs());
    }
  }
}
