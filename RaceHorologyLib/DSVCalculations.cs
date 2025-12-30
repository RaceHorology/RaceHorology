/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
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

namespace RaceHorologyLib
{
  public class DSVRaceCalculation
  {
    public class TopTenResult
    {
      public TopTenResult(RaceResultItem rri, double consideredDsvPoints, double racePoints)
      {
        RRI = rri;
        DSVPoints = consideredDsvPoints;
        RacePoints = racePoints;
        TopFive = false;
      }

      public RaceResultItem RRI { get; set; }
      public double DSVPoints { get; set; }
      public double RacePoints { get; set; }
      public bool TopFive { get; set; }

      public override string ToString()
      {
        return string.Format("Zeit: {0}, ListPoints: {1}, Best5Points {2}({4}), RacePoints: {3}",
          RRI.TotalTime, RRI.Participant.Points, DSVPoints, RacePoints, TopFive);
      }
    }

    private Race _race;
    private RaceResultViewProvider _vpSource;
    private char _sex;

    private double _valueF;
    private double _valueA;
    private double _valueZ;
    private double _minPenalty;
    private double _valueCutOff;

    private double _penaltyA;
    private double _penaltyB;
    private double _penaltyC;
    private double _penalty;
    private double _penaltyRounded;
    private double _penaltyWithAdder;
    bool _calculationValid;

    private double _appliedPenalty;

    TimeSpan? _bestTime;
    List<TopTenResult> _topTen;
    List<TopTenResult> _topFiveDSV;



    public double ExactCalculatedPenalty { get { return _penalty; } }
    public double CalculatedPenalty { get { return _penaltyRounded; } }
    public double CalculatedPenaltyWithAdded { get { return _penaltyWithAdder; } }
    public double AppliedPenalty { get { return _appliedPenalty; } }
    public double MinPenalty { get { return _minPenalty; } }

    public bool CalculationValid { get { return _calculationValid; } }

    public double ValueF { get { return _valueF; } }
    public double ValueA { get { return _valueA; } }
    public double ValueZ { get { return _valueZ; } }
    public double PenaltyA { get { return _penaltyA; } }
    public double PenaltyB { get { return _penaltyB; } }
    public double PenaltyC { get { return _penaltyC; } }

    public List<TopTenResult> TopTen { get { return _topTen; } }
    public List<TopTenResult> TopFiveDSV { get { return _topFiveDSV; } }


    public DSVRaceCalculation(Race race, RaceResultViewProvider vpSource, char sex)
    {
      _race = race;
      _vpSource = vpSource;
      _sex = sex;

      _valueF = race.RaceConfiguration.ValueF;
      _valueA = race.RaceConfiguration.ValueA;
      _valueZ = race.RaceConfiguration.ValueZ;

      _minPenalty = race.RaceConfiguration.MinimumPenalty;
      _valueCutOff = race.RaceConfiguration.ValueCutOff;

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
        return Math.Round(_valueF * (((TimeSpan)rri.TotalTime).TotalSeconds / ((TimeSpan)_bestTime).TotalSeconds - 1.0) + penalty, 2);

      return -1.0;
    }


    public void CalculatePenalty()
    {
      try
      {
        findTopTen();

        markBestFive();
        calculatePenaltyAC();

        findBestFiveDSV();
        calculatePenaltyB();

        calculatePenalty();

        if (_topTen.Count == 0)
          _calculationValid = false;
        else
          _calculationValid = true;
      }
      catch (Exception)
      {
        _calculationValid = false;
        throw;
      }
    }



    void findTopTen()
    {
      System.Collections.IEnumerable results = _vpSource.GetViewList();
      if (results == null)
        throw new Exception("calculation not possible");

      List<RaceResultItem> items = new List<RaceResultItem>();

      // Copy from Results
      foreach (var item in results)
      {
        if (item is RaceResultItem rri)
          if (sexMatched(rri) && hasResult(rri))
            items.Add(rri);
      }

      ResultSorter<RaceResultItem> comparer = new TotalTimeSorter();
      items.Sort(comparer);

      _topTen = new List<TopTenResult>();

      int i = 0;
      TimeSpan? lastTime10th = null;
      while (i < 10 && i < items.Count)
      {
        // Store the best time
        if (i == 0)
          _bestTime = items[i].TotalTime;

        _topTen.Add(new TopTenResult(items[i], cutOffPoints(items[i].Participant.Points), cutOffPoints(CalculatePoints(items[i], false))));

        // Remember time of 10th
        if (_topTen.Count == 10)
          lastTime10th = items[i].TotalTime;

        i++;
      }

      // Consider all participants at position 10 (have same time as the 10th)
      // (FIS Points Rules §4.4.5)
      while (i < items.Count)
      {
        if (lastTime10th != null && lastTime10th == items[i].TotalTime)
          _topTen.Add(new TopTenResult(items[i], cutOffPoints(items[i].Participant.Points), CalculatePoints(items[i], false)));
        i++;
      }
    }

    void markBestFive()
    {
      for (int i = 0; i < 5; i++)
      {
        int nextBest = int.MaxValue;
        double nextBestValue = double.MaxValue;

        // Iterate from back i.e., pick the worsest in case of same points (FIS Points Rules §4.4.4, §4.4.6)
        for (int j = _topTen.Count - 1; j >= 0; j--)
        {
          var item = _topTen[j];

          // Already marked as top five?
          if (item.TopFive)
            continue;

          if (item.DSVPoints < nextBestValue)
          {
            nextBestValue = item.DSVPoints;
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
          valueA += item.DSVPoints;
          valueC += item.RacePoints;
        }
      }

      _penaltyA = Math.Round(valueA, 2, MidpointRounding.AwayFromZero);
      _penaltyC = Math.Round(valueC, 2, MidpointRounding.AwayFromZero);
    }


    void calculatePenaltyB()
    {
      double valueB = .0;

      foreach (var rri in _topFiveDSV)
      {
        valueB += cutOffPoints(rri.DSVPoints);
      }

      _penaltyB = Math.Round(valueB, 2, MidpointRounding.AwayFromZero);
    }

    void calculatePenalty()
    {
      _penalty = (_penaltyA + _penaltyB - _penaltyC) / 10.0;
      _penaltyRounded = Math.Round(_penalty, 2, MidpointRounding.AwayFromZero);
      _penaltyWithAdder = _penaltyRounded + _valueA + _valueZ;
      _appliedPenalty = Math.Max(_minPenalty, _penaltyWithAdder);
    }


    void findBestFiveDSV()
    {
      _topFiveDSV = new List<TopTenResult>();


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
                if (!_topFiveDSV.Exists(x => x.RRI == rri))
                {
                  bestPoints = rri.Participant.Points;
                  bestRRI = rri;
                }
              }
            }
          }
        }

        if (bestRRI != null)
          _topFiveDSV.Add(new TopTenResult(bestRRI, cutOffPoints(bestRRI.Participant.Points), cutOffPoints(bestRRI.Points)));
      }
    }

    double cutOffPoints(double points)
    {
      return 0.0 <= points && points < _valueCutOff ? points : _valueCutOff;
    }

    bool sexMatched(RaceResultItem rri)
    {
      if (_sex == char.MinValue)
        return true;

      var sex = rri?.Participant?.Sex?.Name;
      return sex != null && Char.ToUpper((char)sex) == _sex;
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
