using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public interface ILiveTiming
  {

    Race Race { get; set; }

    void Start();
    void Stop();

  }
}
