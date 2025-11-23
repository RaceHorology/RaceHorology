//Trigger
namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for HandTimingDlg.xaml
  /// </summary>
  public partial class HandTimingDlg : Window
  {
    public HandTimingDlg()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
      ucHandTiming.Init(dm, race);

      ucHandTiming.Finished += UcHandTiming_Finished;
    }

    private void UcHandTiming_Finished(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}
