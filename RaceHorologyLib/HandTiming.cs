using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class HandTiming
  {
    public static IHandTiming CreateHandTiming(string device, string devicePort)
    {
      IHandTiming handTiming = null;

      switch (device)
      {
        case "ALGETimy":
          handTiming = new ALGETimy(devicePort);
          break;
        case "TagHeuerPPro":
          handTiming = new TagHeuer(devicePort);
          break;
        case "File":
          handTiming = new FromFileHandTiming(devicePort);
          break;
      }

      return handTiming;
    }

  }




}
