using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace DSVAlpin2
{
  class DSVAlpin2HTTPServer
  {
    private HttpServer _httpServer;
    private string _baseFolder;

    public DSVAlpin2HTTPServer(UInt16 port)
    {
      // AppFolder + /webroot
      _baseFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"webroot");

      _httpServer = new HttpServer(port);
      _httpServer.Log.Level = LogLevel.Trace;
      _httpServer.DocumentRootPath = _baseFolder;

      // Set the HTTP GET request event.
      _httpServer.OnGet += OnGetHandler;
    }

    public void Start()
    {
      //_httpServer.AddWebSocketService<TestWebSocket>("/TestWebSocket");
      //_httpServer.AddWebSocketService<Echo>("/Echo");
      //_httpServer.AddWebSocketService<Chat>("/Chat");

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
}
