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
      using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
      {
        socket.Connect("8.8.8.8", 65530);
        System.Net.IPEndPoint endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
        localIP = endPoint.Address.ToString();
      }
      return "http://" + localIP + ":" + _httpServer.Port + "/";
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
      _httpServer.AddWebSocketService<StartListBehavior>("/StartList", (connection) => { connection.SetupThis(_dataModel); });
      _httpServer.AddWebSocketService<ResultListBehavior>("/ResultList", (connection) => { connection.SetupThis(_dataModel); });


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


  public class StartListBehavior : DSVAlpinBaseBehavior
  {
    ItemsChangedNotifier _notifier;

    public override void SetupThis(AppDataModel dm)
    {
      base.SetupThis(dm);

      Application.Current.Dispatcher.Invoke(() => 
      {
        _notifier = new ItemsChangedNotifier(_dm.GetRace().GetRun(0).GetStartList());
        _notifier.CollectionChanged += StartListChanged;
        _notifier.ItemChanged += StartListItemChanged;
      });
    }

    protected override void TearDown()
    {
      if (_dm != null)
      {
        _notifier.CollectionChanged -= StartListChanged;
        _notifier.ItemChanged -= StartListItemChanged;
      }

      base.TearDown();
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
      string output=null;
      Application.Current.Dispatcher.Invoke(() =>
      {
        output = JsonConversion.ConvertStartList(_dm.GetRace().GetRun(0).GetStartList());
      });

      Send(output);
    }


    protected override void OnMessage(MessageEventArgs e)
    {
      SendStartList();
      //var name = Context.QueryString["name"];
      //Send(!name.IsNullOrEmpty() ? String.Format("\"{0}\" to {1}", e.Data, name) : e.Data);
    }
  }

  public class ResultListBehavior : DSVAlpinBaseBehavior
  {
    ItemsChangedNotifier _notifier;

    public override void SetupThis(AppDataModel dm)
    {
      base.SetupThis(dm);

      Application.Current.Dispatcher.Invoke(() =>
      {
        _notifier = new ItemsChangedNotifier(_dm.GetRace().GetRun(0).GetResultView());
        _notifier.CollectionChanged += ResultListChanged;
        _notifier.ItemChanged += ResultListItemChanged;
      });


    }

    protected override void TearDown()
    {
      if (_dm != null)
      {
        _notifier.CollectionChanged -= ResultListChanged;
        _notifier.ItemChanged -= ResultListItemChanged;
      }

      base.TearDown();
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
        output = JsonConversion.ConvertRunResults(_dm.GetRace().GetRun(0).GetResultView());
      });

      Send(output);
    }


    protected override void OnMessage(MessageEventArgs e)
    {
      SendResultList();
      //var name = Context.QueryString["name"];
      //Send(!name.IsNullOrEmpty() ? String.Format("\"{0}\" to {1}", e.Data, name) : e.Data);
    }
  }


}
