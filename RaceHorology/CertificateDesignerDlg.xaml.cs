using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CertificateDesignerDlg.xaml
    /// </summary>
    public partial class CertificateDesignerDlg : Window
    {
        public CertificateDesignerDlg()
        {
            InitializeComponent();
        }

        public void Init(AppDataModel dm, Race race)
        {
            ucCertDesigner.Init(dm, race);

            ucCertDesigner.Finished += UcCertDesigner_Finished;
        }


        private void UcCertDesigner_Finished(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
 
            var result = MessageBox.Show(
                "Ja = \tSchließen und Layout Speichern\r\nNein =\tSchließen\r\n",
                "Urkunden Designer",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    ucCertDesigner.SaveToLayoutToDatabase();                   
                    // allow close
                    break;

                case MessageBoxResult.No:                   
                    break;

                case MessageBoxResult.Cancel:
                    e.Cancel = true;   // keep window open
                    break;
            }

        }
    }
}
