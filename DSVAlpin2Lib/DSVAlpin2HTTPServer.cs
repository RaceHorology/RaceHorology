using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

using DSVAlpin2Lib;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;

namespace DSVAlpin2Lib
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
    public DSVAlpin2HTTPServer(UInt16 port, AppDataModel dm)
    {
      _dataModel = dm;

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

    /// <summary>
    /// Actually starts the server
    /// </summary>
    public void Start()
    {
      _httpServer.AddWebSocketService<LiveDataBehavior>("/LiveData", (connection) => { connection.SetupThis(_dataModel); });

      _httpServer.Start();
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
  }


  public class DSVAlpinBaseBehavior : WebSocketBehavior
  {
    protected AppDataModel _dm;

    ~DSVAlpinBaseBehavior()
    {
      TearDown();
    }

    public virtual void SetupThis(AppDataModel dm)
    {
      _dm = dm;
    }

    protected virtual void TearDown()
    {
      _dm = null;
    }


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

    public override void SetupThis(AppDataModel dm)
    {
      base.SetupThis(dm);

      _liveProvider = new List<LiveDataProvider>();

      Add(new StartListDataProvider(_dm));
      Add(new RemainingStartListDataProvider(_dm));
      Add(new RaceRunDataProvider(_dm));
      Add(new RaceDataProvider(_dm));
      Add(new RaceResultDataProvider(_dm));
      Add(new OnTrackDataProvider(_dm));
    }


    protected override void TearDown()
    {
      while (_liveProvider.Count() > 0)
        Remove(_liveProvider[_liveProvider.Count() - 1]);
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
      Send(eventData.Data);
    }


    protected override void OnMessage(MessageEventArgs e)
    {
      foreach (var p in _liveProvider)
        p.SendInitial();
      //var name = Context.QueryString["name"];
      //Send(!name.IsNullOrEmpty() ? String.Format("\"{0}\" to {1}", e.Data, name) : e.Data);
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
    AppDataModel _dm;
    RaceRun _currentRace;
    RemainingStartListViewProvider _rslVP;
    ItemsChangedNotifier _notifier;
    System.Timers.Timer _timer;

    public RemainingStartListDataProvider(AppDataModel dm)
    {
      _dm = dm;

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
        output = JsonConversion.ConvertOnStartList(_rslVP.GetView());
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
    //System.Timers.Timer _timer;

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
        output = JsonConversion.ConvertRaceResults(_dm.GetCurrentRace().GetResultViewProvider().GetView());
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
      data["type"] = r.RaceType.ToString();
      data["run"] = rr.Run.ToString();

      string output = null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertCurrrentRaceRun(data);
      });

      OnNewDataToSend(this, new NewDataEventArgs { Data = output });
    }
  }


}
