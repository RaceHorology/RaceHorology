using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public delegate void OnStatusChanged();

  public interface ILiveTiming
  {

    Race Race { get; set; }

    void Start();
    void Stop();

    bool Started { get; }

    event OnStatusChanged StatusChanged;

  }
}
