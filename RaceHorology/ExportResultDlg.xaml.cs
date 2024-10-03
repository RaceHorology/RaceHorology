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
  /// Interaction logic for ExportResultDlg.xaml
  /// </summary>
  public partial class ExportResultDlg : Window
  {
    string title;
    string exportedFile;
    string message;

    public ExportResultDlg(string title, string exportedFile, string message)
    {
      InitializeComponent();

      this.title = title;
      this.exportedFile = exportedFile;
      this.message = message;

      lblStatus.Content = message;
      Title = title;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e)
    {
      openExplorer(exportedFile);
      DialogResult = true;
    }

    private void openExplorer(string filePath)
    {
      if (!System.IO.File.Exists(filePath))
        return;

      // combine the arguments together
      // it doesn't matter if there is a space after ','
      string argument = "/select, \"" + filePath + "\"";

      System.Diagnostics.Process.Start("explorer.exe", argument);
    }
  }
}
