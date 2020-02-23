using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class DSVRaceCalculation
  {
    private Race _race;

    public DSVRaceCalculation(Race race)
    {
      _race = race;
    }


    public double CalculatedPenaltyMen { get; }
    public double CalculatedPenaltyWomen { get; }

    public double AppliedPenaltyMen { get; set; }
    public double AppliedPenaltyWomen { get; set; }

  }
}
