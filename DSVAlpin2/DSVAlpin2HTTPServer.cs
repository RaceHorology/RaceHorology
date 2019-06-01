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

namespace DSVAlpin2
{
  class DSVAlpin2HTTPServer
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

    public DSVAlpin2HTTPServer(UInt16 port, AppDataModel dm)
    {
      _dataModel = dm;
      // AppFolder + /webroot
      _baseFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"webroot");
      _baseFolder = @"C:\src\DSVAlpin2\work\DSVAlpin2\DSVAlpin2\webroot";

      _httpServer = new HttpServer(port);
      _httpServer.Log.Level = LogLevel.Trace;
      _httpServer.DocumentRootPath = _baseFolder;

      // Set the HTTP GET request event.
      _httpServer.OnGet += OnGetHandler;
    }

    public void Start()
    {
      _httpServer.AddWebSocketService<StartListBehavior>("/StartList", (connection) => { connection.SetupThis(_dataModel); });

      _httpServer.Start();
    }

    public void Stop()
    {
      _httpServer.Stop();
    }


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
    public override void SetupThis(AppDataModel dm)
    {
      base.SetupThis(dm);

      _dm.GetRun(0).GetStartList().CollectionChanged += StartListChanged;
    }

    protected override void TearDown()
    {
      if (_dm != null)
        _dm.GetRun(0).GetStartList().CollectionChanged -= StartListChanged;

      base.TearDown();
    }

    
    private void StartListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      SendStartList();
    }

    void SendStartList()
    {
      string output = JsonConvert.SerializeObject(_dm.GetRun(0).GetStartList());
      Send(output);
    }


    protected override void OnMessage(MessageEventArgs e)
    {
      SendStartList();
      //var name = Context.QueryString["name"];
      //Send(!name.IsNullOrEmpty() ? String.Format("\"{0}\" to {1}", e.Data, name) : e.Data);
    }
  }

}
