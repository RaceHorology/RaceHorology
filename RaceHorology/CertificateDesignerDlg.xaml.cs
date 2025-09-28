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
using RaceHorologyLib;

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
    }
}
