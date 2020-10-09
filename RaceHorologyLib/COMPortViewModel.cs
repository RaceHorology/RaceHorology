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


    class COMPortComparer: IComparer<COMPort>
    {
      public int Compare(COMPort a, COMPort b)
      {
        return a.Port.CompareTo(a.Port);
      }

    }




    ObservableCollection<COMPort> _comPorts;

    public COMPortViewModel()
    {
      _comPorts = new ObservableCollection<COMPort>();
      fillInitially();

      _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      ComPorts = new ObservableCollection<string>(SerialPort.GetPortNames().OrderBy(s => s));

      WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

      _watcher = new ManagementEventWatcher(query);
      _watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);
      _watcher.Start();
    }

    void fillInitially()
    {
      IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

      foreach (var port in ports)
        AddPort(port);
    }



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
          AddPort(port);
      }
    }

    private void AddPort(string port)
    {
      _comPorts.InsertSorted(new COMPort { Text = getPrettyName(port), Port = port }, new COMPortComparer());
    }


    string getPrettyName(string port)
    {
      string prettyName = port;

      ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
      ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");
      ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
      ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);
      using (comPortSearcher)
      {
        string caption = null;
        foreach (ManagementObject obj in comPortSearcher.Get())
        {
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

                if (name == port)
                {
                  prettyName = caption;
                  break;
                }
              }
            }
          }
        }
      }

      return prettyName;
    }

    public ObservableCollection<string> ComPorts { get; private set; }

    #region IDisposable Members

    public void Dispose()
    {
      _watcher.Stop();
    }

    #endregion

    private ManagementEventWatcher _watcher;
    private TaskScheduler _taskScheduler;

    public ObservableCollection<COMPort> Items { get { return _comPorts; } }


    internal class ProcessConnection
    {
      #region Methods

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

      #endregion Methods
    }
  }


}
