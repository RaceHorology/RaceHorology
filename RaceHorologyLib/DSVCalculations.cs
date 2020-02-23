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
    private string _sex;
    
    private double _valueF;
    private double _valueA;
    private double _minPenalty;

    private double _penaltyA;
    private double _penaltyB;
    private double _penaltyC;
    private double _penalty;

    private double _appliedPenalty;

    List<Tuple<RaceResultItem, bool>> _topTen;
    List<RaceResultItem> _topFiveDSV;



    public double ExactCalculatedPenalty { get { return _penalty; } }
    public double CalculatedPenalty { get { return Math.Round(ExactCalculatedPenalty); } }
    public double AppliedPenalty { get { return _appliedPenalty; } }



    public DSVRaceCalculation(Race race, string sex)
    {
      _race = race;
      _sex = sex;

      _valueF = race.RaceConfiguration.ValueF;
      _valueA = race.RaceConfiguration.ValueA;

      _minPenalty = race.RaceConfiguration.MinimumPenalty;
      _penalty = _penaltyA = _penaltyB = _penaltyC = 0.0;
      _appliedPenalty = 0.0;
    }


    public double CalculatePoints(RaceResultItem rri, TimeSpan bestTime)
    {
      return Math.Round(_valueF * ((TimeSpan)rri.TotalTime).TotalSeconds / bestTime.TotalSeconds - _valueF + _valueA + _appliedPenalty, 2);
    }

    public void CalculatePenalty()
    {
      calculatePenaltyAC();

      calculatePenaltyB();

      calculatePenalty();
    }



    void findTopTen()
    {
      List<RaceResultItem> items = new List<RaceResultItem>();

      // Copy from Results
      foreach( var item in _race.GetResultViewProvider().GetView().SourceCollection)
      {
        if (item is RaceResultItem rri)
          if (includeResult(rri))
            items.Add(rri);
      }

      ResultSorter<RaceResultItem> comparer = new TotalTimeSorter();
      items.Sort(comparer);

      _topTen = new List<Tuple<RaceResultItem, bool>>();

      for (int i = 0; i < 10 && i < items.Count; i++)
        _topTen.Add(new Tuple<RaceResultItem,bool>(items[i], false));
    }

    void markBestFive()
    {
      for(int i=0; i<5; i++)
      {
        int nextBest = int.MaxValue;
        double nextBestValue = double.MaxValue;

        for( int j=0; j<_topTen.Count; j++)
        {
          var item = _topTen[j];

          if (item.Item2 == true)
            continue;

          if (item.Item1.Participant.Points < nextBestValue)
          {
            nextBestValue = item.Item1.Participant.Points;
            nextBest = j;
          }

        }
        if (nextBest < int.MaxValue)
          _topTen[nextBest] = new Tuple<RaceResultItem, bool>(_topTen[nextBest].Item1, true);
      }
    }

    void calculatePenaltyAC()
    {
      findTopTen();
      markBestFive();

      double valueA = .0;
      double valueC = .0;

      for (int j = 0; j < _topTen.Count; j++)
      {
        var item = _topTen[j];
        if (item.Item2 == true)
        {
          valueA += item.Item1.Participant.Points;
          valueC += item.Item1.Points;
        }
      }

      _penaltyA = Math.Round(valueA, 2);
      _penaltyC = Math.Round(valueC, 2);
    }


    void calculatePenaltyB()
    {
      findBestFiveDSV();

      double valueB = .0;

      foreach(var rri in _topFiveDSV)
      {
        valueB += rri.Participant.Points;
      }

      _penaltyB = valueB;
    }

    void calculatePenalty()
    {
      _penalty = (_penaltyA + _penaltyB - _penaltyC) / 10.0;
      _appliedPenalty = Math.Max(_minPenalty, _penalty);
    }


    void findBestFiveDSV()
    {
      _topFiveDSV = new List<RaceResultItem>();


      for (int i = 0; i < 5; i++)
      {
        double bestPoints = double.MaxValue;
        RaceResultItem bestRRI = null;

        foreach (var item in _race.GetResultViewProvider().GetView().SourceCollection)
        {
          if (item is RaceResultItem rri)
          {
            if (includeResult(rri) && didStart(rri))
            {
              if (rri.Participant.Points < bestPoints)
              {
                // Esnure not yet added
                if (!_topFiveDSV.Exists(x => x == rri))
                {
                  bestPoints = rri.Participant.Points;
                  bestRRI = rri;
                }
              }
            }
          }
        }

        if (bestRRI != null)
          _topFiveDSV.Add(bestRRI);
      }
    }


    bool includeResult(RaceResultItem rri)
    {
      if (string.IsNullOrEmpty(_sex))
        return true;

      return rri.Participant.Sex == _sex;
    }

    bool didStart(RaceResultItem rri)
    {
      return rri.SubResults[1].RunResultCode != RunResult.EResultCode.NaS;
    }

  }
}
