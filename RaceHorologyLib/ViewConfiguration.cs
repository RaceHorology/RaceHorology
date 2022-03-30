/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
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


  public class ViewFactory
  {
    protected Dictionary<string, ViewProvider> _prototypes;

    public ViewFactory()
    {
      _prototypes = new Dictionary<string, ViewProvider>();

      _prototypes["Startlist_1stRun_StartnumberAscending"] = new FirstRunStartListViewProvider();
      _prototypes["Startlist_1stRun_Points_0"] = new DSVFirstRunStartListViewProvider(0);
      _prototypes["Startlist_1stRun_Points_15"] = new DSVFirstRunStartListViewProvider(15);
      _prototypes["Startlist_1stRun_Points_30"] = new DSVFirstRunStartListViewProvider(30);

      _prototypes["Startlist_2nd_StartnumberAscending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);
      //_prototypes["Startlist_2nd_StartnumberAscending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);
      _prototypes["Startlist_2nd_StartnumberDescending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      //_prototypes["Startlist_2nd_StartnumberDescending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      _prototypes["Startlist_2nd_PreviousRun_0_OnlyWithResults"] = new BasedOnResultsFirstRunStartListViewProvider(0, false);
      _prototypes["Startlist_2nd_PreviousRun_0_AlsoWithoutResults"] = new BasedOnResultsFirstRunStartListViewProvider(0, true);
      _prototypes["Startlist_2nd_PreviousRun_15_OnlyWithResults"] = new BasedOnResultsFirstRunStartListViewProvider(15, false);
      _prototypes["Startlist_2nd_PreviousRun_15_AlsoWithoutResults"] = new BasedOnResultsFirstRunStartListViewProvider(15, true);
      _prototypes["Startlist_2nd_PreviousRun_30_OnlyWithResults"] = new BasedOnResultsFirstRunStartListViewProvider(30, false);
      _prototypes["Startlist_2nd_PreviousRun_30_AlsoWithoutResults"] = new BasedOnResultsFirstRunStartListViewProvider(30, true);
      _prototypes["Startlist_2nd_PreviousRun_all_OnlyWithResults"] = new BasedOnResultsFirstRunStartListViewProvider(int.MaxValue, false);
      _prototypes["Startlist_2nd_PreviousRun_all_AlsoWithoutResults"] = new BasedOnResultsFirstRunStartListViewProvider(int.MaxValue, true);

      _prototypes["RaceResult_BestOfTwo"] = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);
      _prototypes["RaceResult_Sum"] = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.Sum);
      _prototypes["RaceResult_SumBest2"] = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.SumBest2);
      _prototypes["RaceResult_SumDSVPointsSchool"] = new DSVSchoolRaceResultViewProvider();
      _prototypes["RaceResult_SumPointsViaTable"] = new PointsViaTableRaceResultViewProvider();

      _prototypes["RaceRunResult"] = new RaceRunResultViewProvider();


    }


    public ViewProvider Create(string viewKey)
    {
      if (viewKey == null)
        return null;

      ViewProvider prototype;
      if (_prototypes.TryGetValue(viewKey, out prototype))
      {
        return prototype.Clone();
      }

      return null;
    }

    public T Create<T>(string viewKey) where T:ViewProvider
    {
      T instance = Create(viewKey) as T;
      return instance;
    }



  }


  public class ViewConfigurator
  {
    protected Race _race;
    protected AppDataModel _dataModel;
    protected RaceConfiguration _config;

    public ViewConfigurator(Race race)
    {
      _race = race;
      _dataModel = race.GetDataModel();
      _config = race.RaceConfiguration.Copy();
    }


    public StartListViewProvider GetStartlistViewProvider(RaceRun rr, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      StartListViewProvider slVP;

      // First Run
      if (1 == rr.Run)
      {
        FirstRunStartListViewProvider frslVP = factory.Create<FirstRunStartListViewProvider>(_config.Run1_StartistView);

        // Backup if nothing has been created
        if (frslVP == null)
          frslVP = new FirstRunStartListViewProvider();

        frslVP.SetDefaultGrouping(_config.Run1_StartistViewGrouping);

        frslVP.Init(rr.GetRace().GetParticipants());
        slVP = frslVP;
      }
      else
      // Second or later run
      {
        // Figure out previous run
        RaceRun rrPrevious = rr.GetRace().GetRuns().Where( r => r.Run == (rr.Run-  1U)).First();

        SecondRunStartListViewProvider srslVP = factory.Create<SecondRunStartListViewProvider>(_config.Run2_StartistView);

        if (srslVP == null)
          srslVP = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);

        srslVP.SetDefaultGrouping(_config.Run2_StartistViewGrouping);

        srslVP.Init(rrPrevious);
        slVP = srslVP;
      }

      return slVP;
    }


    public RemainingStartListViewProvider GetRemainingStartersViewProvider(RaceRun rr, string context = null)
    {
      var slVP = GetStartlistViewProvider(rr, context);

      if (slVP == null)
        return null;

      var rslVP = new RemainingStartListViewProvider();
      rslVP.Init(slVP, rr);

      return rslVP;
    }


    public RaceRunResultViewProvider GetRaceRunResultViewProvider(RaceRun rr, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      RaceRunResultViewProvider rVP;

      rVP = factory.Create<RaceRunResultViewProvider>("RaceRunResult");

      if (rVP == null)
        rVP = new RaceRunResultViewProvider();

      rVP.SetDefaultGrouping(_config.DefaultGrouping);

      rVP.Init(rr, _dataModel);

      return rVP;
    }


    public RaceResultViewProvider GetRaceResultViewProvider(Race race, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      RaceResultViewProvider rVP;

      rVP = factory.Create<RaceResultViewProvider>(_config.RaceResultView);

      if (rVP == null)
        rVP = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);
      
      rVP.SetDefaultGrouping(_config.DefaultGrouping);

      rVP.Init(race, _dataModel);

      return rVP;
    }


    public void ConfigureRace(Race race)
    {
      for(int i=0; i<race.GetMaxRun(); i++)
      {
        RaceRun rr = race.GetRun(i);

        StartListViewProvider slVP = GetStartlistViewProvider(rr);
        rr.SetStartListProvider(slVP);

        RaceRunResultViewProvider rrVP = GetRaceRunResultViewProvider(rr);
        rr.SetResultViewProvider(rrVP);
      }

      RaceResultViewProvider raceVP = GetRaceResultViewProvider(race);
      race.SetResultViewProvider(raceVP);
    }

  }
}
