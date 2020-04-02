﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class StartNumberAsignment
  {
    public uint StartNumber;
    public RaceParticipant Participant;
  }

  /// <summary>
  /// Represents the startnumber asignments and its working space
  /// </summary>
  public class StartNumberAssignment
  {
    ObservableCollection<StartNumberAsignment> _snAssignment;

    public StartNumberAssignment()
    {
      _snAssignment = new ObservableCollection<StartNumberAsignment>();
    }

    /// <summary>
    /// Returns the workng space containing current StartNumber and Particpant
    /// </summary>
    public ObservableCollection<StartNumberAsignment> ParticipantList
    { get { return _snAssignment; } }

    /// <summary>
    /// Assigns next free startnumber to the participant
    /// </summary>
    /// <param name="participant">The particpant the startnumber to assign</param>
    /// <returns></returns>
    public uint AssignNextFree(RaceParticipant participant)
    {
      uint sn = GetNextFreeStartNumber();
      Assign(sn, participant);
      return sn;
    }

    /// <summary>
    /// Assignes the specified startnumber to the specified participant
    /// </summary>
    /// <param name="sn">The start number</param>
    /// <param name="participant">The particpant the startnumber to assign</param>
    public void Assign(uint sn, RaceParticipant participant)
    {
      if ((int)sn - 1 < _snAssignment.Count)
        _snAssignment[(int)sn - 1] = new StartNumberAsignment { Participant = participant };
      else
      {
        // Fill up with empty space
        while (_snAssignment.Count < (int)sn - 1)
          _snAssignment.Add(new StartNumberAsignment());

        // Put participant at the correct place
        _snAssignment.Add(new StartNumberAsignment { Participant = participant });
      }

      updateStartNumbers((int)sn - 1);
    }

    /// <summary>
    /// Inserts a new startnumber slot at the given position. All existing start numbers higher that sn will be increased accordingly.
    /// </summary>
    /// <param name="sn"></param>
    public void InsertAndShift(uint sn)
    {
      _snAssignment.Insert((int)sn-1, new StartNumberAsignment());

      updateStartNumbers((int)sn - 1);
    }

    /// <summary>
    /// Removes a new startnumber slot at the given position. All existing start numbers higher that sn will be descreased accordingly.
    /// </summary>
    /// <param name="sn"></param>
    public void Delete(uint sn)
    {
      if ( sn - 1 < _snAssignment.Count)
        _snAssignment.RemoveAt((int)sn - 1);

      updateStartNumbers((int)sn - 1);
    }

    /// <summary>
    /// Returns the next free startnumber (number of assigned startnumber slots + 1)
    /// </summary>
    /// <returns></returns>
    public uint GetNextFreeStartNumber()
    {
      return (uint)_snAssignment.Count+1;
    }

    /// <summary>
    /// Internal function to update the startnumber for each slot correctly starting from from.
    /// </summary>
    /// <param name="from">Update starts at this startnumber (optional, just an optimization)</param>
    protected void updateStartNumbers(int from = 0)
    {
      for (int i = from; i < _snAssignment.Count; i++)
        if (_snAssignment != null && _snAssignment[i] != null)
          _snAssignment[i].StartNumber = (uint)i + 1;
    }
  }


  public class ParticpantSelector
  {
    Race _race;
    StartNumberAssignment _snAssignment;

    public ParticpantSelector(Race race, StartNumberAssignment snAssignment)
    {
      _race = race;
      _snAssignment = snAssignment;
    }


    public RaceParticipant PickNextParticipant()
    {
      return null;
    }

  }
}
