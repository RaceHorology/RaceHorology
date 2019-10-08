using System;

namespace LiveTimingRM
{

  public class Class1
  {

    public Class1()
    {
    }

    public void Login()
    {
      rmlt.LiveTiming lv = new rmlt.LiveTiming();
      lv.LoginLiveTiming("01122", "livetiming", "livetiming");


    }
  }
}
