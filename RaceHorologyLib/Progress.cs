using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class StdProgress
  {
    public string CurrentStatus { get; set; } = string.Empty;
    public int Percentage { get; set; } = -1;

    public bool Finished { get; set; } = false;
  }

  public interface IHasProgress<T>
  {
    void DoProgressReport(IProgress<T> progress);
  }

}
