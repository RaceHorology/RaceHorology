using RaceHorologyLib;
using System;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for TeamResultsPrintUC.xaml
  /// </summary>
  public partial class TeamResultsPrintUC : UserControl, IReportSubUC
  {
    public TeamResultsPrintUC()
    {
      InitializeComponent();
    }

    public void HandleReportGenerator(IPDFReport reportGenerator)
    {
      var certificates = reportGenerator as TeamRaceResultReport;
      chkPrintNonConsideredTeamParticipants.IsChecked = certificates.PrintNonConsideredTeamParticipants;
    }
    public void Apply(IPDFReport reportGenerator)
    {
      var teamReport = reportGenerator as TeamRaceResultReport;
      if (teamReport != null)
      {
        try
        {
          teamReport.PrintNonConsideredTeamParticipants = chkPrintNonConsideredTeamParticipants.IsChecked == true;
        }
        catch (Exception)
        {
        }
      }
    }
  }
}
