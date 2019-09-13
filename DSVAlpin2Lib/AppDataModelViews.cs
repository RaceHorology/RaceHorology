using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSVAlpin2Lib
{


  public class ViewProvider
  {
    public ICollectionView GetView()
    {

    }

  }


  public class StartListViewProvider : ViewProvider
  {

    // Input: List<RaceParticipant>

    // Output: sorted List<StartListEntry> according to StartNumber

    public void SetDefaultGrouping(string propertyName) { }
    public void ChangeGrouping(string propertyName) { }
    public void ResetToDefaultGrouping(string propertyName) { }

  }


  public class FirstRunStartListViewProvider :  StartListViewProvider
  {

    // Input: List<RaceParticipant>

    // Output: sorted List<StartListEntry> according to StartNumber

  }


  // First n (15) per grouping are always kept constant
  public class DSVFirstRunStartListViewProvider : FirstRunStartListViewProvider
  {

    // Input: List<RaceParticipant>

    // Output: sorted List<StartListEntry> according to StartNumber

    // Parameter: first n

  }




  public class SecondRunStartListViewProvider : StartListViewProvider
  {
    // Input: List<StartListEntry> (1st run),
    //        List<RaceResultWithPosition> (1st run)

    // Output: sorted List<StartListEntry> according to StartNumber

  }


  // wie 1. DG, 1. DG rückwärts
  public class SimpleSecondRunStartListViewProvider : SecondRunStartListViewProvider
  {
    

  }


  // basierend auf (1. DG) Ergebnisliste: rückwärts, ersten n gelost, mit/ohne disqualifizierten vorwärts oder rückwärts
  public class BasedOnResultsSecondRunStartListViewProvider : SecondRunStartListViewProvider
  {


  }




  public class RemainingStartListViewProvider : StartListViewProvider
  {

    // Input: StartListViewProvider or List<StartListEntry>

    // Output: sorted List<StartListEntry> according to StartNumber

  }



  public class ResultViewProvider : IViewProvider
  {
    public void SetDefaultGrouping(string propertyName) { }
    public void ChangeGrouping(string propertyName) { }
    public void ResetToDefaultGrouping(string propertyName) { }

  }


  public class RaceRunResultViewProvider : ResultViewProvider
  {
    // Input: RaceRun

    // Output: List<RunResultWithPosition>


  }


  public class RaceResultViewProvider : ResultViewProvider
  {
    // Input: Race

    // Output: List<RunResultWithPosition>


  }

/* e.g. FamilienWertung
  public class SpecialRaceResultViewProvider : ResultViewProvider
  {
    // Input: Race

    // Output: List<RunResultWithPosition>


  }
  */



}
