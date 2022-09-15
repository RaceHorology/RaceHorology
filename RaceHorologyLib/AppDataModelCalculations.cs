using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  /// <summary>
  /// ClassAssignment can:
  /// (a) Assign the classes to a list of participants (<see cref="Assign(IList{Participant})"/>)
  /// (b) Determine the default class for a specific participants (<see cref="DetermineClass(Participant)"/>)
  /// </summary>
  public class ClassAssignment
  {
    List<ParticipantClass> _classesByYear; // Classes by year descending

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="classes">The classes to use/assign</param>
    public ClassAssignment(IList<ParticipantClass> classes)
    {
      _classesByYear = new List<ParticipantClass>(classes);
      _classesByYear.Sort(Comparer<ParticipantClass>.Create((c1, c2) => c2.Year.CompareTo(c1.Year)));

    }

    /// <summary>
    /// Assigns all participants the default class based on Sex and Year
    /// </summary>
    /// <param name="participants">The participants to assign the class</param>
    public void Assign(IList<Participant> participants)
    {
      foreach (var p in participants)
        Assign(p);
    }

    public void Assign(Participant participant)
    {
      var c = DetermineClass(participant);
      participant.Class = c;
    }

    /// <summary>
    /// Determines the default class based on Year and Sex of the participant
    /// </summary>
    /// <param name="p">The participant</param>
    /// <returns>The default class</returns>
    public ParticipantClass DetermineClass(Participant p)
    {
      ParticipantClass cFound = null;

      foreach (var c in _classesByYear)
      {
        if (c.Sex == p.Sex)
        {
          if (c.Year < p.Year)
            break;

          cFound = c;
        }
      }

      return cFound;
    }

  }
}
