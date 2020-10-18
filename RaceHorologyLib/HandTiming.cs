/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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

using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static RaceHorologyLib.HandTimingVMEntry;

namespace RaceHorologyLib
{
  public class HandTiming
  {
    public static IHandTiming CreateHandTiming(string device, string devicePort)
    {
      IHandTiming handTiming = null;

      switch (device)
      {
        case "ALGETimy":
          handTiming = new ALGETimy(devicePort);
          break;
        case "TagHeuerPPro":
          handTiming = new TagHeuer(devicePort);
          break;
        case "File":
          handTiming = new FromFileHandTiming(devicePort);
          break;
      }

      return handTiming;
    }

  }



  public class HandTimingVMEntry : INotifyPropertyChanged
  {
    public enum ETimeModus { EStartTime, EFinishTime };


    internal RunResult OriginalRunResult { get { return _runResult; } }

    public uint? StartNumber 
    { 
      get { return _startNumber; }
      set { if (_startNumber != value) { _startNumber = value; notifyPropertyChanged(); } }
    }
    public TimeSpan? StartTime 
    { 
      get { return _startTime; }
      set { if (_startTime != value) { _startTime = value; notifyPropertyChanged(); } }
    }
    public TimeSpan? FinishTime 
    { 
      get { return _finishTime; }
      set { if (_finishTime != value) { _finishTime = value; notifyPropertyChanged(); } }
    }
    public TimeSpan? RunTime 
    { 
      get { return _runTime; }
      set { if (_runTime != value) { _runTime = value; notifyPropertyChanged(); } }
    }

    public bool ManuallyAdjustedStartTime
    {
      get { return _manuallyAdjustedStartTime; }
      set { if (_manuallyAdjustedStartTime != value) { _manuallyAdjustedStartTime = value; notifyPropertyChanged(); } }
    }

    public bool ManuallyAdjustedFinishTime
    {
      get { return _manuallyAdjustedFinishTime; }
      set { if (_manuallyAdjustedFinishTime != value) { _manuallyAdjustedFinishTime = value; notifyPropertyChanged(); } }
    }

    /// Returns either StartTime or FinishTime depending on timeModus
    public TimeSpan? ATime { get { return _timeModus == ETimeModus.EStartTime ? StartTime : FinishTime; } }

    public TimeSpan? HandTime { get { return _handTime; } }
    public TimeSpan? HandTimeDiff
    {
      get { return _handTimeDiff; }
      private set
      {
        if (_handTimeDiff != value)
        {
          _handTimeDiff = value;
          notifyPropertyChanged();
        }
      }
    }

    public ETimeModus TimeModus
    {
      get { return _timeModus; }
      set 
      {
        if (_timeModus != value)
        {
          _timeModus = value;
          notifyPropertyChanged();
          updateInternal();
        }
      }
    }

    uint? _startNumber;
    RunResult _runResult;
    TimeSpan? _startTime;
    TimeSpan? _finishTime;
    TimeSpan? _runTime;
    TimeSpan? _handTime;
    TimeSpan? _handTimeDiff;
    ETimeModus _timeModus;
    bool _manuallyAdjustedStartTime;
    bool _manuallyAdjustedFinishTime;

    public HandTimingVMEntry(ETimeModus timeModus, RunResult runResult, TimeSpan? handTime)
    {
      _timeModus = timeModus;
      _manuallyAdjustedStartTime = false;
      _manuallyAdjustedFinishTime = false;

      _handTime = handTime;

      _runResult = runResult;
      copyFromRunResult(runResult);
    }


    public HandTimingVMEntry ShallowCopy()
    {
      return (HandTimingVMEntry)this.MemberwiseClone();
    }

    public void SetRunResult(RunResult runResult)
    {
      _runResult = runResult;
      copyFromRunResult(runResult);

      notifyPropertyChanged_RunResult();
      updateInternal();
    }

    public void SetHandTime(TimeSpan? handTime)
    {
      _handTime = handTime;

      notifyPropertyChanged_HandTime();
      updateInternal();
    }

    public void SetCalulatedHandTime(TimeSpan? calcTime)
    {
      if (_timeModus == ETimeModus.EStartTime)
      {
        StartTime = calcTime;
        ManuallyAdjustedStartTime = true;
      }
      else
      {
        FinishTime = calcTime;
        ManuallyAdjustedFinishTime = true;
      }

      updateInternal();
    }

    private void copyFromRunResult(RunResult runResult)
    {
      _startNumber = runResult?.StartNumber;
      _startTime = runResult?.GetStartTime();
      _finishTime = runResult?.GetFinishTime();
      _runTime = runResult?.GetRunTime(true, false);
    }

    private void updateInternal()
    {
      if (_handTime != null && ATime != null)
      {
        var t1 = new RoundedTimeSpan((TimeSpan)_handTime, 2, RoundedTimeSpan.ERoundType.Floor);
        var t2 = new RoundedTimeSpan((TimeSpan)ATime, 2, RoundedTimeSpan.ERoundType.Floor);

        HandTimeDiff = t1.TimeSpan.Subtract(t2.TimeSpan);
      }
      else
        HandTimeDiff = null;
    }

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void notifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void notifyPropertyChanged_RunResult()
    {
      foreach (var p in new string[] { "StartNumber", "StartTime", "FinishTime", "RunTime" })
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    private void notifyPropertyChanged_HandTime()
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HandTime"));
    }

    #endregion
  }


  internal class HandTimingVMEntrySorter : IComparer<HandTimingVMEntry>
  {
    NullEnabledComparer _nullComparer = new NullEnabledComparer();

    public int Compare(HandTimingVMEntry x, HandTimingVMEntry y)
    {
      TimeSpan? tX = x.ATime != null ? x.ATime : x.HandTime;
      TimeSpan? tY = y.ATime != null ? y.ATime : y.HandTime;

      int s = _nullComparer.Compare(tX, tY);
      if (s != 0)
        return s;

      return _nullComparer.Compare(x.StartNumber, y.StartNumber);
    }
  }

  public class HandTimingVM
  {
    HandTimingVMEntry.ETimeModus _timeModus;
    ObservableCollection<HandTimingVMEntry> _handTimings;
    HandTimingVMEntrySorter _handTimingsSorter;
    int _finEntryMaxDifferenceMS = 1000;

    public ObservableCollection<HandTimingVMEntry> Items { get { return _handTimings; } }

    public HandTimingVM(HandTimingVMEntry.ETimeModus timeModus)
    {
      _timeModus = timeModus;
      _handTimings = new ObservableCollection<HandTimingVMEntry>();
      _handTimingsSorter = new HandTimingVMEntrySorter();
    }


    public HandTimingVMEntry.ETimeModus TimeModus
    {
      get { return _timeModus; }
      set 
      { 
        if (_timeModus != value)
        {
          _timeModus = value;
          foreach (var v in _handTimings)
            v.TimeModus = _timeModus;

          _handTimings.Sort(_handTimingsSorter);
        }
      }
    }


    public void Dissolve(HandTimingVMEntry entry)
    {
      // Only split if there is something to split
      if (entry.HandTime != null && entry.StartNumber != null)
      {
        // Create entry for handtime
        HandTimingVMEntry entryHT = new HandTimingVMEntry(entry.TimeModus, null, entry.HandTime);
        entry.SetHandTime(null);

        _handTimings.InsertSorted(entryHT, _handTimingsSorter);
      }
    }

    public void AssignStartNumber(HandTimingVMEntry entry, uint startNumber)
    {
      // Find entry with correct startnumber
      HandTimingVMEntry entrySN = _handTimings.FirstOrDefault(x => x.StartNumber == startNumber);

      if (entrySN!=null)
      {
        // Merge entrys
        entrySN.SetHandTime(entry.HandTime);
        _handTimings.Remove(entry);
      }
      else
      {
        entry.StartNumber = startNumber;
      }

      _handTimings.Sort(_handTimingsSorter);
    }


    public void AddRunResults(IEnumerable<RunResult> runResults)
    { 
      foreach(var rr in runResults)
      {
        HandTimingVMEntry e = findEntry(rr);
        if (e != null && e.OriginalRunResult == rr)
        {
          // Do nothing
        }
        else if (e != null && e.ATime == null)
        {
          e.SetRunResult(rr);
        }
        else
        {
          _handTimings.InsertSorted(new HandTimingVMEntry(_timeModus, rr, null), _handTimingsSorter);
        }
      }
      _handTimings.Sort(_handTimingsSorter);
    }

    public void AddHandTimings(IEnumerable<TimingData> handTimings)
    {
      foreach (var ht in handTimings)
      {
        HandTimingVMEntry e = findEntry(ht);
        if (e != null && e.HandTime == ht.Time)
        {
          // Do nothing
        }
        else if (e != null && e.HandTime == null)
        {
          e.SetHandTime(ht.Time);
        }
        else
        {
          _handTimings.InsertSorted(new HandTimingVMEntry(_timeModus, null, ht.Time), _handTimingsSorter);
        }
      }
      _handTimings.Sort(_handTimingsSorter);
    }

    public void DeleteHandTimings()
    {
      var ht = _handTimings.ToList();
      foreach (var t in ht)
      {
        if (t.HandTime != null)
        {
          if (t.StartTime != null || t.FinishTime != null)
            t.SetHandTime(null);
          else
            _handTimings.Remove(t);
        }
      }
    }


    private TimeSpan? getATime(RunResult rr)
    {
      return _timeModus == HandTimingVMEntry.ETimeModus.EStartTime ? rr.GetStartTime() : rr.GetFinishTime();
    }

    public HandTimingVMEntry findEntry(RunResult rr)
    {
      foreach (var e in _handTimings)
      {
        if (e.OriginalRunResult == rr)
          return e;

        if (e.HandTime == null)
          continue;

        TimeSpan? aTime = getATime(rr);
        if (aTime == null)
          continue;

        if ( Math.Abs(((TimeSpan)aTime).Subtract((TimeSpan)e.HandTime).TotalMilliseconds) < _finEntryMaxDifferenceMS)
        {
          return e;
        }
      }
      return null;
    }


    public HandTimingVMEntry findEntry(TimingData td)
    {
      foreach (var e in _handTimings)
      {
        if (e.HandTime == td.Time)
          return e;

        if (e.ATime == null)
          continue;

        TimeSpan? hTime = td.Time;
        if (hTime == null)
          continue;

        if (Math.Abs(((TimeSpan)hTime).Subtract((TimeSpan)e.ATime).TotalMilliseconds) < _finEntryMaxDifferenceMS)
        {
          return e;
        }
      }
      return null;
    }

  }



  public class HandTimingCalc
  {
    int _numberOfEntriesToUse = 10;

    HandTimingVMEntry _entryToCalculate;
    List<HandTimingVMEntry> _usedEntries;
    List<HandTimingVMEntry> _calculatedEntries;
    TimeSpan? _calculatedTime;

    public TimeSpan? CalculatedTime { get { return _calculatedTime; } }
    public HandTimingVMEntry EntryToCalculate { get { return _entryToCalculate; } }

    public IEnumerable<HandTimingVMEntry> UsedEntries { get { return _usedEntries; } }
    public IEnumerable<HandTimingVMEntry> CalculatedEntries { get { return _calculatedEntries; } }


    public HandTimingCalc(HandTimingVMEntry entry, IEnumerable<HandTimingVMEntry> sortedEntries)
    {
      _usedEntries = new List<HandTimingVMEntry>();

      _entryToCalculate = entry;

      findAndStoreEntriesToUse(new List<HandTimingVMEntry>(sortedEntries));
      calculateHandTime();
    }

    private void findAndStoreEntriesToUse(List<HandTimingVMEntry> sortedEntries)
    {
      bool entryCanBeUsed(HandTimingVMEntry entry) { return entry.ATime != null && entry.HandTime != null && entry.HandTimeDiff != null; }

      int indexCalc = sortedEntries.IndexOf(_entryToCalculate);
      if (indexCalc < 0)
        throw new Exception("entry to calculate cannot be found, internal error");

      // move backwards as far as possible and copy entries to use
      int index = indexCalc-1;
      while(index >= 0 && _usedEntries.Count < _numberOfEntriesToUse)
      {
        if (entryCanBeUsed(sortedEntries[index]))
          _usedEntries.Insert(0, sortedEntries[index].ShallowCopy()); // make copy to avoid inferrences with upcoming operations

        index--;
      }

      // move forwards as far as possible and copy entries to use
      index = indexCalc + 1;
      while (index < sortedEntries.Count && _usedEntries.Count < _numberOfEntriesToUse)
      {
        if (entryCanBeUsed(sortedEntries[index]))
          _usedEntries.Add(sortedEntries[index].ShallowCopy()); // make copy to avoid inferrences with upcoming operations

        index++;
      }
    }

    private void calculateHandTime()
    {
      _calculatedTime = null;

      if (_usedEntries.Count > 0 && _entryToCalculate?.HandTime != null)
      {
        TimeSpan correctionValue = new TimeSpan(0);
        foreach (var e in _usedEntries)
        {
          correctionValue = correctionValue.Add((TimeSpan)e.HandTimeDiff);
        }

        correctionValue = new TimeSpan(correctionValue.Ticks / _usedEntries.Count);
        // Round (real rounding)
        correctionValue = new RoundedTimeSpan(correctionValue, 2, RoundedTimeSpan.ERoundType.Round).TimeSpan;

        TimeSpan handTime = (TimeSpan)_entryToCalculate.HandTime;
        _calculatedTime = handTime.Subtract(correctionValue);

        _calculatedEntries = new List<HandTimingVMEntry>();
        // Make a clean list of all entries
        foreach (var item in _usedEntries)
        {
          _calculatedEntries.Add(item.ShallowCopy());
        }
        var calculatedItem = _entryToCalculate.ShallowCopy();
        calculatedItem.SetCalulatedHandTime(_calculatedTime);
        _calculatedEntries.Add(calculatedItem);
        _calculatedEntries.Sort(new HandTimingVMEntrySorter());
      }
    }
  }


  public class HandTimingVMManager
  {
    AppDataModel _dm;
    Dictionary<string, HandTimingVM> _handTimingVM;

    public HandTimingVMManager(AppDataModel dm)
    {
      _dm = dm;
      _handTimingVM = new Dictionary<string, HandTimingVM>();
    }


    public void LoadHandTimingFromFile()
    {
      string handTimingFilePath = System.IO.Path.Combine(
        _dm.GetDB().GetDBPathDirectory(),
        _dm.GetDB().GetDBFileName() + ".handtiming");

      if (File.Exists(handTimingFilePath))
      {
        Dictionary<string, List<TimeSpan>> handTimingData = new Dictionary<string, List<TimeSpan>>();
        try
        {
          string configJSON = System.IO.File.ReadAllText(handTimingFilePath);
          Newtonsoft.Json.JsonConvert.PopulateObject(configJSON, handTimingData);
        }
        catch (Exception e)
        {
        }

        foreach(var a in handTimingData)
        {
          Race race = null;
          RaceRun run = null;
          ETimeModus timeModus;

          if (parseTimingVMKey(a.Key, out race, out run, out timeModus))
          {
            var vm = GetHandTimingVM(race, run, timeModus);

            List<TimingData> handTime = new List<TimingData>();

            foreach (var t in a.Value)
              handTime.Add(new TimingData { Time = t });

            vm.AddHandTimings(handTime);
          }
        }
      }
    }


    public void SaveHandTimingToFile()
    {
      Dictionary<string, List<TimeSpan>> handTimingData = new Dictionary<string, List<TimeSpan>>();
      foreach (var vm in _handTimingVM)
      {
        handTimingData[vm.Key] = new List<TimeSpan>();
        foreach (var t in vm.Value.Items)
        {
          if (t.HandTime != null)
            handTimingData[vm.Key].Add((TimeSpan)t.HandTime);
        }
      }

      string handTimingFilePath = System.IO.Path.Combine(
        _dm.GetDB().GetDBPathDirectory(),
        _dm.GetDB().GetDBFileName() + ".handtiming");
      using (StreamWriter file = File.CreateText(handTimingFilePath))
      {
        using (JsonWriter writer = new JsonTextWriter(file))
        {
          JsonSerializer serializer = new JsonSerializer();
          serializer.Formatting = Formatting.Indented;
          serializer.Serialize(writer, handTimingData);
        }
      }
    }


    public void SaveToDataModel()
    {
      foreach(var vm in _handTimingVM)
      {
        var htVM = vm.Value;
        foreach(var i in htVM.Items)
        {
          if (i.ManuallyAdjustedStartTime)
          {
            i.OriginalRunResult.SetStartTime(i.StartTime);
            // Clear any stored runtime to get it calculated
            i.OriginalRunResult.SetRunTime(null);
          }
          if (i.ManuallyAdjustedFinishTime)
          {
            i.OriginalRunResult.SetFinishTime(i.FinishTime);
            // Clear any stored runtime to get it calculated
            i.OriginalRunResult.SetRunTime(null);
          }
        }
      }
    }


    public HandTimingVM GetHandTimingVM(Race race, RaceRun run, ETimeModus timeModus)
    {
      HandTimingVM vm;

      if (!_handTimingVM.TryGetValue(handTimingVMKey(race, run, timeModus), out vm))
      {
        vm = new HandTimingVM(timeModus);
        _handTimingVM.Add(handTimingVMKey(race, run, timeModus), vm);
      }

      vm.AddRunResults(run.GetResultList());

      return vm;
    }

    private string handTimingVMKey(Race race, RaceRun run, ETimeModus timeModus)
    {
      return string.Format("{0}_{1}_{2}", race.RaceType, run.Run, timeModus);
    }

    private bool parseTimingVMKey(string key, out Race race, out RaceRun run, out ETimeModus timeModus)
    {
      race = null;
      run = null;
      timeModus = ETimeModus.EStartTime;

      var keyParts = key.Split('_');
      if (keyParts.Length != 3)
        return false;
      
      foreach(var r in _dm.GetRaces())
      {
        if (r.RaceType.ToString() == keyParts[0])
        {
          race = r;
          break;
        }
      }

      if (race == null)
        return false;

      foreach(var r in race.GetRuns())
      {
        if (r.Run.ToString() == keyParts[1])
        {
          run = r;
          break;
        }
      }

      if (ETimeModus.EStartTime.ToString() == keyParts[2])
        timeModus = ETimeModus.EStartTime;
      else if (ETimeModus.EFinishTime.ToString() == keyParts[2])
        timeModus = ETimeModus.EFinishTime;
      else
        return false;

      return true;
    }

  }

  public class HandTimingCalcReport : PDFRaceReport
  {
    HandTimingCalc _calculation;

    public HandTimingCalcReport(HandTimingCalc calculation, Race race) : base(race)
    {
      _calculation = calculation;

    }

    protected override string getReportName()
    {
      return string.Format("Handzeitberechnung - StNr {0} - {1}", 
        _calculation.EntryToCalculate.StartNumber, _calculation.EntryToCalculate.TimeModus);
    }


    protected override string getTitle()
    {
      return string.Format("Handzeitberechnung\nfür Startnummer {0} ({1})",
        _calculation.EntryToCalculate.StartNumber, _calculation.EntryToCalculate.TimeModus==ETimeModus.EStartTime?"Start":"Ziel");
    }


    protected override void addContent(PdfDocument pdf, Document document)
    {

      Table table = generateHandTimingTable();
      addHeaderToTable(table);

      int i = 0;
      foreach(var item in _calculation.CalculatedEntries)
      {
        addLineToTable(table, item, i++);
      }

      document.Add(table);
    }

    protected Table generateHandTimingTable()
    {

      float[] cols = { 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F };
      var table = new Table(cols);

      table.SetWidth(UnitValue.CreatePercentValue(100));
      table.SetBorder(Border.NO_BORDER);

      return table;
    }

    protected void addHeaderToTable(Table table)
    {
      table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Start-Nr.")));

      table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Startzeit")));
      
      table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Zielzeit")));
      
      table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Laufzeit")));
      
      table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Handzeit")));
      
      table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Differenz")));
    }

    protected void addLineToTable(Table table, HandTimingVMEntry data, int i)
    {
      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      bool bManuallyAdjustedEntry = data.ManuallyAdjustedFinishTime || data.ManuallyAdjustedStartTime;

      // Startnumber
      var pStNr = createCellParagraphForTable(formatStartNumber((uint)data.StartNumber));
      if (bManuallyAdjustedEntry)
        pStNr.SetItalic();
      table.AddCell(createCellForTable(TextAlignment.CENTER).SetBackgroundColor(bgColor).Add(pStNr));

      // StartTime
      var pStartTime = createCellParagraphForTable(data.StartTime.ToRaceTimeString());
      if (bManuallyAdjustedEntry)
        pStartTime.SetItalic();
      if (data.ManuallyAdjustedStartTime)
        pStartTime.SetBold();
      table.AddCell(createCellForTable(TextAlignment.CENTER).SetBackgroundColor(bgColor).Add(pStartTime));

      // FinishTime
      var pFinishTime = createCellParagraphForTable(data.FinishTime.ToRaceTimeString());
      if (bManuallyAdjustedEntry)
        pFinishTime.SetItalic();
      if (data.ManuallyAdjustedFinishTime)
        pFinishTime.SetBold();
      table.AddCell(createCellForTable(TextAlignment.CENTER).SetBackgroundColor(bgColor).Add(pFinishTime));

      // RunTime
      var pRunTime = createCellParagraphForTable(data.RunTime.ToRaceTimeString());
      if (bManuallyAdjustedEntry)
        pRunTime.SetItalic().SetBold();
      table.AddCell(createCellForTable(TextAlignment.CENTER).SetBackgroundColor(bgColor).Add(pRunTime));

      // HandTime
      var pHandTime = createCellParagraphForTable(data.HandTime.ToRaceTimeString());
      if (bManuallyAdjustedEntry)
        pHandTime.SetItalic();
      table.AddCell(createCellForTable(TextAlignment.CENTER).SetBackgroundColor(bgColor).Add(pHandTime));

      // Difference
      var pHandDiff = createCellParagraphForTable(data.HandTimeDiff.ToRaceTimeString());
      if (bManuallyAdjustedEntry)
        pHandDiff.SetItalic().SetBold();
      table.AddCell(createCellForTable(TextAlignment.CENTER).SetBackgroundColor(bgColor).Add(pHandDiff));
    }
  }
}
