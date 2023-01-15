/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

using RaceHorologyLib;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;

namespace RaceHorologyLib
{
  /// <summary>
  /// Provides the web server backend for the mobile clients
  /// </summary>
  /// 
  /// It starts a web server and user the provided DataModel in order to serve the data.
  /// 
  /// Updates are propoageted immediately via WebSockets to the client.
  public class DSVAlpin2HTTPServer
  {
    private HttpServer _httpServer;
    private string _baseFolder;
    AppDataModel _dataModel;

    /// <summary>
    /// Returns the URL the Application WebPage is available
    /// </summary>
    /// <returns> The URL</returns>
    /// 
    /// <remarks>
    /// Connect on a UDP socket has the following effect: 
    /// it sets the destination for Send/Recv, discards all packets from other addresses, 
    /// and - which is what we use - transfers the socket into "connected" state, settings 
    /// its appropriate fields.This includes checking the existence of the route to the 
    /// destination according to the system's routing table and setting the local endpoint accordingly. 
    /// The last part seems to be undocumented officially but it looks like an integral trait 
    /// of Berkeley sockets API (a side effect of UDP "connected" state) that works reliably 
    /// in both Windows and Linux across versions and distributions.
    /// 
    /// So, this method will give the local address that would be used to connect to the 
    /// specified remote host. There is no real connection established, hence the specified 
    /// remote ip can be unreachable.
    /// </remarks>
    public string GetUrl()
    {
      string localIP;
      try
      {
        using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
        {
          socket.Connect("8.8.8.8", 65530);
          System.Net.IPEndPoint endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
          localIP = endPoint.Address.ToString();
        }
        return "http://" + localIP + ":" + _httpServer.Port + "/";
      }
      catch(System.Net.Sockets.SocketException)
      {
        return null;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="port">The port number the web service shall be available</param>
    /// <param name="dm">The DataModel to use</param>
    public DSVAlpin2HTTPServer(UInt16 port)
    {
      // AppFolder + /webroot
      _baseFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"webroot");

      // Use the source folder in case the debugger is attached for easier development
      if (System.Diagnostics.Debugger.IsAttached)
      {
        // This will get the current WORKING directory (i.e. \bin\Debug)
        string workingDirectory = Environment.CurrentDirectory;         // or: Directory.GetCurrentDirectory() gives the same result

        // This will get the current PROJECT directory
        string projectDirectory = System.IO.Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        _baseFolder = System.IO.Path.Combine(projectDirectory, @"webroot");
      }

      // Configure the server
      _httpServer = new HttpServer(port);
      _httpServer.Log.Level = LogLevel.Trace;
      _httpServer.DocumentRootPath = _baseFolder;

      // Set the HTTP GET request event.
      _httpServer.OnGet += OnGetHandler;
    }


    public delegate void DataModelChangedHandler(AppDataModel dm);
    public event DataModelChangedHandler DataModelChanged;
    public void UseDataModel(AppDataModel dm)
    {
      _dataModel = dm;

      var handler = DataModelChanged;
      handler?.Invoke(dm);
    }

    public AppDataModel DataModel { get { return _dataModel; } }


    /// <summary>
    /// Actually starts the server
    /// </summary>
    public void Start()
    {
      try
      {
        _httpServer.AddWebSocketService<LiveDataBehavior>("/api/LiveData", (connection) => { connection.SetupThis(this); });

        _httpServer.Start();
      }
      catch (Exception)
      { }
    }

    /// <summary>
    /// Stops the server
    /// </summary>
    public void Stop()
    {
      _httpServer.Stop();
    }

    /// <summary>
    /// Get-Handler - Simple method to provide the web pages (e.g. html,javascript, ...)
    /// </summary>
    private void OnGetHandler(object sender, HttpRequestEventArgs e)
    {
      if (e.Request.RawUrl.StartsWith("/api/"))
      {
        HandleAPIRequest(e);
      }
      else
      {
        HandleStaticContentRequest(e);
      }
    }


    protected void HandleStaticContentRequest(HttpRequestEventArgs e)
    {
      var req = e.Request;
      var res = e.Response;

      var path = req.RawUrl;
      if (path == "/")
        path += "index.html";

      byte[] contents;
      if (!e.TryReadFile(path, out contents))
      {
        res.StatusCode = (int)HttpStatusCode.NotFound;
        return;
      }

      if (path.EndsWith(".html"))
      {
        res.ContentType = "text/html";
        res.ContentEncoding = Encoding.UTF8;
      }
      else if (path.EndsWith(".js"))
      {
        res.ContentType = "application/javascript";
        res.ContentEncoding = Encoding.UTF8;
      }
      else if (path.EndsWith(".css"))
      {
        res.ContentType = "text/css";
        res.ContentEncoding = Encoding.UTF8;
      }
      else if (path.EndsWith(".svg"))
      {
        res.ContentType = "image/svg+xml";
        res.ContentEncoding = Encoding.UTF8;
      }

      res.WriteContent(contents);
    }

    protected void HandleAPIRequest(HttpRequestEventArgs e)
    {
      var req = e.Request;
      var res = e.Response;

      string apiVersion = null, listName = null;
      int raceNo = -1, runNo = -1;
      parseUrlPath(req.RawUrl, out apiVersion, out listName, out raceNo, out runNo);

      
      var r = req.QueryString;


      Race race = null;
      if (raceNo < 0)
        race = _dataModel?.GetCurrentRace();
      else
        race = _dataModel?.GetRace(raceNo);

      RaceRun raceRun = null;
      if (runNo < 0)
        raceRun = _dataModel?.GetCurrentRaceRun();
      else
        raceRun = race?.GetRun(runNo);

      string output = "";
      if (listName == "startlist")
      {
        if (raceRun != null)
          output = getStartList(raceRun, getGrouping(req.QueryString));
      }
      else if (listName == "nextstarters")
      {
        if (raceRun != null)
          output = getRemainingStartersList(raceRun, getGrouping(req.QueryString), getLimit(req.QueryString));
      }
      else if (listName == "resultlist")
      {
        if (raceRun != null)
          output = getResultList(raceRun, getGrouping(req.QueryString));
        else if (race != null)
          output = getResultList(race, getGrouping(req.QueryString));
      }
      else if (listName == "metadata")
      {
        if (race != null)
        {
          var classes = _dataModel.GetParticipantClasses().ToArray();
          var groups = _dataModel.GetParticipantGroups().ToArray();
          var sex = new string[] { "M", "W" };
          var grouping = new string[] { "Class", "Group", "Sex" };
          var runs = race.GetMaxRun();

          output = JsonConversion.ConvertMetaData(classes, groups, sex, grouping, runs);
        }
      }


      res.ContentType = "application/vnd.api+json";
      res.ContentEncoding = Encoding.UTF8;
      res.WriteContent(Encoding.UTF8.GetBytes(output));
    }


    void parseUrlPath(string url, out string apiVersion, out string listName, out int raceNo, out int runNo)
    {
      var urlParts = url.Split('?');

      var urlPathParts = urlParts[0].Split('/');
      apiVersion = null;
      listName = null;
      raceNo = runNo = int.MinValue;

      int i = 0;
      while (i < urlPathParts.Length)
      {
        if (string.Equals(urlPathParts[i], "api", System.StringComparison.OrdinalIgnoreCase))
        {
          i++;
          apiVersion = urlPathParts[i];
        }
        else if (string.Equals(urlPathParts[i], "races", System.StringComparison.OrdinalIgnoreCase))
        {
          i++;
          try { raceNo = int.Parse(urlPathParts[i]); } catch (Exception) { raceNo = -1; }
        }
        else if (string.Equals(urlPathParts[i], "runs", System.StringComparison.OrdinalIgnoreCase))
        {
          i++;
          try { runNo = int.Parse(urlPathParts[i]); } catch (Exception) { runNo = -1; }
        }
        else 
        {
          listName = urlPathParts[i];
        }

        i++;
      }
    }

    string getGrouping(NameValueCollection queryString)
    {
      string grouping;
      string groupby = queryString.Get("groupby");

      switch (groupby)
      {
        case "class":
        case "Class":
          grouping = "Participant.Class";
          break;
        case "group":
        case "Group":
          grouping = "Participant.Group";
          break;
        case "sex":
        case "Sex":
          grouping = "Participant.Sex";
          break;
        default:
          grouping = null;
          break;
      }

      return grouping;
    }

    string getSorting(NameValueCollection queryString)
    {
      string sorting;
      string sortby = queryString.Get("sortby");

      switch (sortby)
      {
        case "class":
          sorting = "Participant.Class";
          break;
        case "group":
          sorting = "Participant.Group";
          break;
        case "sex":
          sorting = "Participant.Sex";
          break;
        default:
          sorting = null;
          break;
      }

      return sorting;
    }


    int getLimit(NameValueCollection queryString)
    {
      try
      {
        if (queryString.Get("limit")!=null)
          return int.Parse(queryString.Get("limit"));
      }
      catch(Exception)
      {}
      return -1;
    }


    string getStartList(RaceRun raceRun, string grouping)
    {
      string output = "";

      Application.Current.Dispatcher.Invoke(() =>
      {
        ViewConfigurator viewConfigurator = new ViewConfigurator(raceRun.GetRace());
        StartListViewProvider vp = viewConfigurator.GetStartlistViewProvider(raceRun);
        if (grouping != null)
          vp.ChangeGrouping(grouping);

        output = JsonConversion.ConvertStartList(vp.GetView());
      });

      return output;
    }


    string getRemainingStartersList(RaceRun raceRun, string grouping, int limit)
    {
      string output = "";

      Application.Current.Dispatcher.Invoke(() =>
      {
        ViewConfigurator viewConfigurator = new ViewConfigurator(raceRun.GetRace());
        var vp = viewConfigurator.GetRemainingStartersViewProvider(raceRun);
        if (grouping != null)
          vp.ChangeGrouping(grouping);

        List<object> remaingStarters = new List<object>();
        int c = 0;
        foreach (var item in vp.GetView())
        {
          if (limit >= 0 && c >= limit)
            break;

          remaingStarters.Add(item);
          c++;
        }

        output = JsonConversion.ConvertOnStartList(remaingStarters);
      });

      return output;
    }


    string getResultList(RaceRun raceRun, string grouping)
    {
      string output = "";

      Application.Current.Dispatcher.Invoke(() =>
      {
        ViewConfigurator viewConfigurator = new ViewConfigurator(raceRun.GetRace());
        RaceRunResultViewProvider vp = viewConfigurator.GetRaceRunResultViewProvider(raceRun);
        if (grouping != null)
          vp.ChangeGrouping(grouping);

        output = JsonConversion.ConvertRunResults(vp.GetView());
      });

      return output;
    }


    string getResultList(Race race, string grouping)
    {
        string output = "";

        Application.Current.Dispatcher.Invoke(() =>
        {
          ViewConfigurator viewConfigurator = new ViewConfigurator(race);
          RaceResultViewProvider vp = viewConfigurator.GetRaceResultViewProvider(race);
          if (grouping != null)
            vp.ChangeGrouping(grouping);

          output = JsonConversion.ConvertRaceResults(vp.GetView(), (uint)race.GetMaxRun());
        });

      return output;
    }
  }


  public abstract class DSVAlpinBaseBehavior : WebSocketBehavior
  {
    protected DSVAlpin2HTTPServer _server;

    ~DSVAlpinBaseBehavior()
    {
      TearDown();
    }

    public virtual void SetupThis(DSVAlpin2HTTPServer server)
    {
      _server = server;
      _server.DataModelChanged += OnDataModelChanged;

      // Populate initially
      OnDataModelChanged(_server.DataModel);
    }

    protected virtual void TearDown()
    {
      _server.DataModelChanged -= OnDataModelChanged;
      _server = null;
    }

    protected abstract void OnDataModelChanged(AppDataModel dm);


    protected override void OnClose(CloseEventArgs e)
    {
      Log.Debug("Closing websocket:" + ToString() + ", " + e.Reason);
      TearDown();
    }

    protected override void OnError(ErrorEventArgs e)
    {
      Log.Error("Error on websocket:" + ToString() + ", " + e.ToString());
      TearDown();
    }
  }


  public abstract class LiveDataProvider : IDisposable
  {

    public class NewDataEventArgs : EventArgs
    {
      public string Data { get; set; }
    }

    public delegate void NewDataToSendHandler(object source, NewDataEventArgs eventData);
    public event NewDataToSendHandler NewDataToSend;
    protected void OnNewDataToSend(object source, NewDataEventArgs eventData)
    {
      NewDataToSendHandler handler = NewDataToSend;
      handler?.Invoke(source, eventData);
    }

    public abstract void SendInitial();

    public abstract void Dispose();
  }


  public class LiveDataBehavior : DSVAlpinBaseBehavior
  {
    List<LiveDataProvider> _liveProvider;

    public LiveDataBehavior()
    {
      _liveProvider = new List<LiveDataProvider>();
    }

    protected override void TearDown()
    {
      while (_liveProvider.Count() > 0)
        Remove(_liveProvider[_liveProvider.Count() - 1]);
    }


    protected override void OnDataModelChanged(AppDataModel dm)
    {

      if (_liveProvider != null)
        while (_liveProvider.Count() > 0)
          Remove(_liveProvider[_liveProvider.Count() - 1]);

      if (dm != null)
      {
        //Add(new StartListDataProvider(dm));
        Add(new RaceDataProvider(dm));
        Add(new RemainingStartListDataProvider(dm, 3));
        Add(new OnTrackDataProvider(dm));
        Add(new OnTrackEventsProvider(dm));
        //Add(new RaceRunDataProvider(dm));
        Add(new RaceResultDataProvider(dm));
      }
    }



    public void Add(LiveDataProvider provider)
    {
      _liveProvider.Add(provider);
      provider.NewDataToSend += OnSendNewData;
    }

    public void Remove(LiveDataProvider provider)
    {
      provider.NewDataToSend -= OnSendNewData;
      _liveProvider.Remove(provider);
      provider.Dispose();
    }


    protected void OnSendNewData(object source, LiveDataProvider.NewDataEventArgs eventData)
    {
      try
      {
        Send(eventData.Data);
      }
      catch (Exception)
      { }
    }


    protected override void OnMessage(MessageEventArgs e)
    {
      foreach (var p in _liveProvider)
        p.SendInitial();
    }
  }


  public class StartListDataProvider : LiveDataProvider
  {
    AppDataModel _dm;
    ItemsChangedNotifier _notifier;
    System.Timers.Timer _timer;

    public StartListDataProvider(AppDataModel dm)
    {
      _dm = dm;
      Application.Current.Dispatcher.Invoke(() =>
      {
        _notifier = new ItemsChangedNotifier(_dm.GetCurrentRace().GetRun(0).GetStartList());
        _notifier.CollectionChanged += StartListChanged;
        _notifier.ItemChanged += StartListItemChanged;
      });
    }

    public override void Dispose()
    {
      _notifier.CollectionChanged -= StartListChanged;
      _notifier.ItemChanged -= StartListItemChanged;
      _notifier = null;
      _timer = null;
    }

    public override void SendInitial()
    {
      SendStartList();
    }

    private void StartListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      SendStartList();
    }
    private void StartListItemChanged(object sender, PropertyChangedEventArgs args)
    {
      SendStartList();
    }

    void SendStartList()
    {
      if (_timer != null)
        _timer.Stop();
      else
      {
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += DoSendStartList;
        _timer.AutoReset = false;
        _timer.Enabled = true;
      }

      _timer.Start();
    }

    void DoSendStartList(object sender, System.Timers.ElapsedEventArgs e)
    {
      string output=null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertStartList(_dm.GetCurrentRace().GetRun(0).GetStartList());
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }



  public class RemainingStartListDataProvider : LiveDataProvider
  {
    int _limit;
    AppDataModel _dm;
    RaceRun _currentRace;
    RemainingStartListViewProvider _rslVP;
    ItemsChangedNotifier _notifier;
    System.Timers.Timer _timer;


    public RemainingStartListDataProvider(AppDataModel dm, int limit)
    {
      _limit = limit;
      _dm = dm;
      _dm.CurrentRaceChanged += OnCurrentRaceChanged;

      ListenToCurrentRaceRun();
    }

    private void OnCurrentRaceChanged(object sender, AppDataModel.CurrentRaceEventArgs args)
    {
      ListenToCurrentRaceRun();
    }

    private void ListenToCurrentRaceRun()
    {
      if (_currentRace != _dm.GetCurrentRaceRun())
      {
        
        if (_notifier != null)
        {
          _notifier.CollectionChanged -= StartListChanged;
          _notifier.ItemChanged -= StartListItemChanged;
          _notifier = null;
        }
        _rslVP = null;



        Application.Current.Dispatcher.Invoke(() =>
        {
          RaceRun raceRun = _dm.GetCurrentRaceRun();
          _rslVP = new RemainingStartListViewProvider();
          _rslVP.Init(raceRun.GetStartListProvider(), raceRun);

          _notifier = new ItemsChangedNotifier(_rslVP.GetView());
          _notifier.CollectionChanged += StartListChanged;
          _notifier.ItemChanged += StartListItemChanged;
        });

        _currentRace = _dm.GetCurrentRaceRun();

        SendStartList();
      }
    }

    public override void Dispose()
    {
      _dm.CurrentRaceChanged -= OnCurrentRaceChanged;

      _notifier.CollectionChanged -= StartListChanged;
      _notifier.ItemChanged -= StartListItemChanged;
      _rslVP = null;
      _notifier = null;
      _timer = null;
    }

    public override void SendInitial()
    {
      SendStartList();
    }

    private void StartListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      SendStartList();
    }
    private void StartListItemChanged(object sender, PropertyChangedEventArgs args)
    {
      SendStartList();
    }

    void SendStartList()
    {
      if (_timer != null)
        _timer.Stop();
      else
      {
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += DoSendStartList;
        _timer.AutoReset = false;
        _timer.Enabled = true;
      }

      _timer.Start();
    }

    void DoSendStartList(object sender, System.Timers.ElapsedEventArgs e)
    {
      string output = null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        List<object> remaingStarters = new List<object>();
        int c = 0;
        foreach (var item in _rslVP.GetView())
        {
          if (_limit >= 0 && c >= _limit)
            break;

          remaingStarters.Add(item);
          c++;
        }
               
        output = JsonConversion.ConvertOnStartList(remaingStarters);
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }





  public class RaceRunDataProvider : LiveDataProvider
  {
    AppDataModel _dm;
    RaceRun _currentRace;
    ItemsChangedNotifier _notifier;
    System.Timers.Timer _timer;

    public RaceRunDataProvider(AppDataModel dm)
    {
      _dm = dm;
      _dm.CurrentRaceChanged += OnCurrentRaceChanged;

      ListenToCurrentRaceRun();
    }

    public override void Dispose()
    {
      _dm.CurrentRaceChanged -= OnCurrentRaceChanged;

      _notifier.CollectionChanged -= ResultListChanged;
      _notifier.ItemChanged -= ResultListItemChanged;
      _notifier = null;
      _timer = null;
    }

    private void OnCurrentRaceChanged(object sender, AppDataModel.CurrentRaceEventArgs args)
    {
      ListenToCurrentRaceRun();
    }

    private void ListenToCurrentRaceRun()
    {
      if (_currentRace != _dm.GetCurrentRaceRun())
      {

        if (_notifier != null)
        {
          _notifier.CollectionChanged -= ResultListChanged;
          _notifier.ItemChanged -= ResultListItemChanged;
          _notifier = null;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
          _notifier = new ItemsChangedNotifier(_dm.GetCurrentRaceRun().GetResultView());
          _notifier.CollectionChanged += ResultListChanged;
          _notifier.ItemChanged += ResultListItemChanged;
        });

        _currentRace = _dm.GetCurrentRaceRun();

        SendResultList();
      }
    }

    public override void SendInitial()
    {
      SendResultList();
    }


    private void ResultListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      SendResultList();
    }

    private void ResultListItemChanged(object sender, PropertyChangedEventArgs args)
    {
      SendResultList();
    }


    void SendResultList()
    {
      if (_timer != null)
        _timer.Stop();
      else
      {
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += DoSendResultList;
        _timer.AutoReset = false;
        _timer.Enabled = true;
      }

      _timer.Start();
    }

    void DoSendResultList(object sender, System.Timers.ElapsedEventArgs e)
    {
      string output = null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertRunResults(_dm.GetCurrentRaceRun().GetResultView());
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }


  public class OnTrackDataProvider : LiveDataProvider
  {
    AppDataModel _dm;
    RaceRun _currentRace;
    ItemsChangedNotifier _notifier;

    public OnTrackDataProvider(AppDataModel dm)
    {
      _dm = dm;
      _dm.CurrentRaceChanged += OnCurrentRaceChanged;

      ListenToCurrentRaceRun();
    }

    public override void Dispose()
    {
      _dm.CurrentRaceChanged -= OnCurrentRaceChanged;

      _notifier.CollectionChanged -= ResultListChanged;
      _notifier.ItemChanged -= ResultListItemChanged;
      _notifier = null;
    }

    private void OnCurrentRaceChanged(object sender, AppDataModel.CurrentRaceEventArgs args)
    {
      ListenToCurrentRaceRun();
    }

    private void ListenToCurrentRaceRun()
    {
      if (_currentRace != _dm.GetCurrentRaceRun())
      {

        if (_notifier != null)
        {
          _notifier.CollectionChanged -= ResultListChanged;
          _notifier.ItemChanged -= ResultListItemChanged;
          _notifier = null;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
          _notifier = new ItemsChangedNotifier(_dm.GetCurrentRaceRun().GetOnTrackList());
          _notifier.CollectionChanged += ResultListChanged;
          _notifier.ItemChanged += ResultListItemChanged;
        });

        _currentRace = _dm.GetCurrentRaceRun();

        SendResultList();
      }
    }

    public override void SendInitial()
    {
      SendResultList();
    }


    private void ResultListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      SendResultList();
    }

    private void ResultListItemChanged(object sender, PropertyChangedEventArgs args)
    {
      SendResultList();
    }


    void SendResultList()
    {
      string output = null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertOnTrack(_dm.GetCurrentRaceRun().GetOnTrackList());
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }





  public class OnTrackEventsProvider : LiveDataProvider
  {
    AppDataModel _dm;
    RaceRun _currentRace;

    public OnTrackEventsProvider(AppDataModel dm)
    {
      _dm = dm;
      _dm.CurrentRaceChanged += OnCurrentRaceChanged;

      ListenToCurrentRaceRun();
    }

    public override void Dispose()
    {
      _dm.CurrentRaceChanged -= OnCurrentRaceChanged;
      _currentRace.OnTrackChanged -= OnSomethingChanged;
    }

    private void OnCurrentRaceChanged(object sender, AppDataModel.CurrentRaceEventArgs args)
    {
      ListenToCurrentRaceRun();
    }

    private void ListenToCurrentRaceRun()
    {
      if (_currentRace != _dm.GetCurrentRaceRun())
      {

        if (_currentRace != null)
          _currentRace.OnTrackChanged -= OnSomethingChanged;

        _currentRace = _dm.GetCurrentRaceRun();

        if (_currentRace != null)
          _currentRace.OnTrackChanged += OnSomethingChanged;
      }
    }

    public override void SendInitial()
    {
    }


    private void OnSomethingChanged(object sender, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
    {
      RaceParticipant particpant = null;

      string eventType = null;
      if (participantEnteredTrack != null)
      {
        eventType = "Started";
        particpant = participantEnteredTrack;
      }
      if (participantLeftTrack != null)
      {
        eventType = "Finished";
        particpant = participantLeftTrack;
      }

      string output = JsonConversion.ConvertEvent(particpant, eventType, currentRunResult);

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }

  }





  public class RaceResultDataProvider : LiveDataProvider
  {
    AppDataModel _dm;
    ItemsChangedNotifier _notifier;
    System.Timers.Timer _timer;

    public RaceResultDataProvider(AppDataModel dm)
    {
      _dm = dm;
      Application.Current.Dispatcher.Invoke(() =>
      {
        _notifier = new ItemsChangedNotifier(_dm.GetCurrentRace().GetResultViewProvider().GetView());
        _notifier.CollectionChanged += DataListChanged;
        _notifier.ItemChanged += DataListItemChanged;
      });
    }

    public override void Dispose()
    {
      _notifier.CollectionChanged -= DataListChanged;
      _notifier.ItemChanged -= DataListItemChanged;
      _notifier = null;
      _timer = null;
    }

    public override void SendInitial()
    {
      SendResultList();
    }

    private void DataListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      SendResultList();
    }
    private void DataListItemChanged(object sender, PropertyChangedEventArgs args)
    {
      SendResultList();
    }

    void SendResultList()
    {
      if (_timer != null)
        _timer.Stop();
      else
      {
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += DoSendResultList;
        _timer.AutoReset = false;
        _timer.Enabled = true;
      }

      _timer.Start();
    }

    void DoSendResultList(object sender, System.Timers.ElapsedEventArgs e)
    {
      string output = null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertRaceResults(_dm.GetCurrentRace().GetResultViewProvider().GetView(), (uint)_dm.GetCurrentRace().GetMaxRun());
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }


  public class RaceDataProvider : LiveDataProvider
  {
    AppDataModel _dm;

    public RaceDataProvider(AppDataModel dm)
    {
      _dm = dm;

      _dm.CurrentRaceChanged += OnCurrentRaceChanged;
    }

    public override void Dispose()
    {
      _dm.CurrentRaceChanged -= OnCurrentRaceChanged;
    }

    public override void SendInitial()
    {
      SendCurrentRaceData();
    }

    private void OnCurrentRaceChanged(object sender, AppDataModel.CurrentRaceEventArgs args)
    {
      SendCurrentRaceData();
    }

    void SendCurrentRaceData()
    {
      Race r = _dm.GetCurrentRace();
      RaceRun rr = _dm.GetCurrentRaceRun();

      Dictionary<string, string> data = new Dictionary<string, string>();
      data["type"] = r?.RaceType.ToString();
      data["run"] = rr?.Run.ToString();

      string output = null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertCurrrentRaceRun(data);
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }


}
