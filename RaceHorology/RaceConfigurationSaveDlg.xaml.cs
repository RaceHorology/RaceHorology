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
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for RaceConfigurationSaveDlg.xaml
  /// </summary>
  public partial class RaceConfigurationSaveDlg : Window
  {
    public string TemplateName { get; private set; }

    public RaceConfigurationSaveDlg(string initTemplateName)
    {
      InitializeComponent();

      if (!string.IsNullOrEmpty(initTemplateName))
        tbName.Text = initTemplateName;

      tbName.SelectAll();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      TemplateName = null;
      DialogResult = true;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      TemplateName = tbName.Text;
      DialogResult = true;
    }
  }
}
