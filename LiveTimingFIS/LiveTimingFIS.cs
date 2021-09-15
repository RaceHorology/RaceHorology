using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveTimingFIS
{
  public class LiveTimingFIS : ILiveTiming
  {

    public LiveTimingFIS()
    { }

    private Race _race;
    public Race Race
    {
      set { _race = value; }
      get { return _race; }
    }

    public void Login(string fisRaceCode, string fisCategory, string fisPassword, string fisPort)
    { 
    }

    public void Start()
    {
      throw new NotImplementedException();
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }

  }
}
