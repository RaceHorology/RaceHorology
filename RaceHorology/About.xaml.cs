using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
  /// Interaction logic for About.xaml
  /// </summary>
  public partial class AboutDlg : Window
  {
    public AboutDlg()
    {
      InitializeComponent();

      Assembly assembly = Assembly.GetEntryAssembly();
      if (assembly == null)
        assembly = Assembly.GetExecutingAssembly();

      if (assembly != null)
      {
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

        var companyName = fvi.CompanyName;
        var productName = fvi.ProductName;
        var copyrightYear = fvi.LegalCopyright;

        var productVersion = fvi.ProductVersion;

        lblVersion.Content = productVersion;
        lblCopyright.Content = string.Format("{0} by {1}", copyrightYear, companyName);
      }
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }


  }
}
