using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class COMPortViewModel
  {
    public class COMPort
    {
      public string Port { get; set; }
      public string Text { get; set; }
      public override string ToString()
      {
        return Text;
      }
    }

    public ObservableCollection<COMPort> Items { get { return _comPorts; } }

    public COMPortViewModel()
    {
      _comPorts = new ObservableCollection<COMPort>();
      fillInitially();

      _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

      WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

      _watcher = new ManagementEventWatcher(query);
      _watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);
      _watcher.Start();
    }


    #region internal
    ObservableCollection<COMPort> _comPorts;

    internal class COMPortComparer : IComparer<COMPort>
    {
      public int Compare(COMPort a, COMPort b)
      {
        return a.Port.CompareTo(b.Port);
      }
    }

    void fillInitially()
    {
      IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

      foreach (var port in ports)
        addPort(port);
    }

    private void addPort(string port)
    {
      _comPorts.InsertSorted(new COMPort { Text = getPrettyName(port), Port = port }, new COMPortComparer());
    }

    #endregion

    #region CheckForUpdates
    private ManagementEventWatcher _watcher;
    private TaskScheduler _taskScheduler;

    private void CheckForNewPorts(EventArrivedEventArgs args)
    {
      // do it async so it is performed in the UI thread if this class has been created in the UI thread
      Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
    }

    private void CheckForNewPortsAsync()
    {
      IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

      foreach (var comPort in _comPorts)
        if (!ports.Contains(comPort.Port))
        {
          _comPorts.Remove(comPort);
          break;
        }

      foreach (var port in ports)
      {
        if (_comPorts.FirstOrDefault(x => x.Port == port) == null)
          addPort(port);
      }
    }

    #endregion


    #region Pretty Names
    string getPrettyName(string port)
    {
      string prettyName = port; // Fallback

      buildPrettyNameCache(); // Ensure the pretty names are cached
      _prettyNameCache.TryGetValue(port, out prettyName);

      return prettyName;
    }

    Dictionary<string, string> _prettyNameCache;
    void buildPrettyNameCache(bool bForce = false)
    {
      if (bForce)
        _prettyNameCache = null;

      if (_prettyNameCache != null)
        return;

      _prettyNameCache = new Dictionary<string, string>();

      ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
      ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");
      ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
      ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);
      using (comPortSearcher)
      {
        string caption = null;
        int i = 0;
        foreach (ManagementObject obj in comPortSearcher.Get())
        {
          i++;
          if (obj != null)
          {
            object captionObj = obj["Caption"];
            object deviceIdObj = obj["DeviceID"];
            if (captionObj != null)
            {
              caption = captionObj.ToString();
              if (caption.Contains("(COM"))
              {
                string name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")",
                                                     string.Empty);

                try {
                  _prettyNameCache.Add(name, caption);
                } catch { }
              }
            }
          }
        }
      }
    }

    internal class ProcessConnection
    {
      public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)
      {
        ManagementScope connectScope = new ManagementScope();
        connectScope.Path = new ManagementPath(@"\\" + machineName + path);
        connectScope.Options = options;
        connectScope.Connect();
        return connectScope;
      }

      public static ConnectionOptions ProcessConnectionOptions()
      {
        ConnectionOptions options = new ConnectionOptions();
        options.Impersonation = ImpersonationLevel.Impersonate;
        options.Authentication = AuthenticationLevel.Default;
        options.EnablePrivileges = true;
        return options;
      }
    }

    #endregion


    #region IDisposable

    public void Dispose()
    {
      _watcher.Stop();
    }

    #endregion


  }


}
