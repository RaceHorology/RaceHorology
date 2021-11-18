/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class DSVRaceCalculation
  {
    public class TopTenResult
    {
      public TopTenResult(RaceResultItem rri, double racePoints)
      {
        RRI = rri;
        DSVPoints = rri.Participant.Points;
        RacePoints = racePoints;
        TopFive = false;
      }

      public RaceResultItem RRI { get; set; }
      public double DSVPoints { get; set; }
      public double RacePoints { get; set; }
      public bool TopFive { get; set; }
    }

    private Race _race;
    private RaceResultViewProvider _vpSource;
    private char _sex;
    
    private double _valueF;
    private double _valueA;
    private double _minPenalty;

    private double _penaltyA;
    private double _penaltyB;
    private double _penaltyC;
    private double _penalty;

    private double _appliedPenalty;

    TimeSpan? _bestTime;
    List<TopTenResult> _topTen;
    List<RaceResultItem> _topFiveDSV;



    public double ExactCalculatedPenalty { get { return _penalty; } }
    public double CalculatedPenalty { get { return Math.Round(ExactCalculatedPenalty, 2); } }
    public double AppliedPenalty { get { return _appliedPenalty; } }

    public double ValueF { get { return _valueF; } }
    public double ValueA { get { return _valueA; } }
    public double PenaltyA { get { return _penaltyA; } }
    public double PenaltyB { get { return _penaltyB; } }
    public double PenaltyC { get { return _penaltyC; } }

    public List<TopTenResult> TopTen { get { return _topTen; } }
    public List<RaceResultItem> TopFiveDSV {  get { return _topFiveDSV; } }


    public DSVRaceCalculation(Race race, RaceResultViewProvider vpSource, char sex)
    {
      _race = race;
      _vpSource = vpSource;
      _sex = sex;

      _valueF = race.RaceConfiguration.ValueF;
      _valueA = race.RaceConfiguration.ValueA;

      _minPenalty = race.RaceConfiguration.MinimumPenalty;
      _penalty = _penaltyA = _penaltyB = _penaltyC = 0.0;
      _appliedPenalty = 0.0;
      _bestTime = null;
    }


    public double CalculatePoints(RaceResultItem rri, bool withPenalty)
    {
      double penalty = 0.0;
      if (withPenalty)
        penalty = _appliedPenalty;

      if (_bestTime != null && rri.TotalTime != null)
        return Math.Round(_valueF * ((TimeSpan)rri.TotalTime).TotalSeconds / ((TimeSpan)_bestTime).TotalSeconds - _valueF + _valueA + penalty, 2);

      return -1.0;
    }


    public void CalculatePenalty()
    {
      findTopTen();

      markBestFive();
      calculatePenaltyAC();

      findBestFiveDSV();
      calculatePenaltyB();

      calculatePenalty();
    }



    void findTopTen()
    {
      System.Collections.IEnumerable results = _vpSource.GetViewList();
      if (results == null)
        throw new Exception("calculation not possible");

      List<RaceResultItem> items = new List<RaceResultItem>();

      // Copy from Results
      foreach ( var item in results)
      {
        if (item is RaceResultItem rri)
          if (sexMatched(rri) && hasResult(rri))
            items.Add(rri);
      }

      ResultSorter<RaceResultItem> comparer = new TotalTimeSorter();
      items.Sort(comparer);

      _topTen = new List<TopTenResult>();

      for (int i = 0; i < 10 && i < items.Count; i++)
      {
        // Store the best time
        if (i==0)
          _bestTime = items[i].TotalTime;

        _topTen.Add(new TopTenResult(items[i], CalculatePoints(items[i], false)));
      }
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

          // Already marked as top five?
          if (item.TopFive)
            continue;

          if (item.RRI.Participant.Points < nextBestValue)
          {
            nextBestValue = item.RRI.Participant.Points;
            nextBest = j;
          }

        }
        if (nextBest < int.MaxValue)
          _topTen[nextBest].TopFive = true;
      }
    }

    void calculatePenaltyAC()
    {
      double valueA = .0;
      double valueC = .0;

      for (int j = 0; j < _topTen.Count; j++)
      {
        var item = _topTen[j];
        if (item.TopFive == true)
        {
          valueA += item.RRI.Participant.Points;
          valueC += item.RacePoints;
        }
      }

      _penaltyA = Math.Round(valueA, 2);
      _penaltyC = Math.Round(valueC, 2);
    }


    void calculatePenaltyB()
    {
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
      _appliedPenalty = Math.Max(_minPenalty, CalculatedPenalty);
    }


    void findBestFiveDSV()
    {
      _topFiveDSV = new List<RaceResultItem>();


      for (int i = 0; i < 5; i++)
      {
        double bestPoints = double.MaxValue;
        RaceResultItem bestRRI = null;

        System.Collections.IEnumerable results = _vpSource.GetViewList();
        if (results == null)
          throw new Exception("calculation not possible");

        foreach (var item in results)
        {
          if (item is RaceResultItem rri)
          {
            if (sexMatched(rri) && didStart(rri))
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


    bool sexMatched(RaceResultItem rri)
    {
      if (_sex == char.MinValue)
        return true;

      return rri?.Participant?.Sex?.Name == _sex;
    }

    bool hasResult(RaceResultItem rri)
    {
      return rri.Position > 0;
    }

    bool didStart(RaceResultItem rri)
    {
      if (rri.SubResults.ContainsKey(1))
        return rri.SubResults[1].RunResultCode != RunResult.EResultCode.NaS;
      return false;
    }

  }
}
