/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */


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

        string licenseMain = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), "COPYING_MAIN.txt"));
        string licenseGPLv3 = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), "COPYING"));
        string licenseThirdParty = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), "LICENSES_THIRD_PARTY.txt"));
        string credits = File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), "CREDITS.txt"));

        txtLicense.Text = licenseMain + "\n\n\n" + licenseGPLv3;

        txtLicense3rdParty.Text = licenseThirdParty;

        txtCredits.Text = credits;
      }


    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }


  }
}
