using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class ALGETimy : IHandTiming
  {
    ALGETdC8001LineParser _parser;
    private SerialPort _serialPort;
    private string _serialPortName;

    public ALGETimy(string serialPortName)
    {
      _parser = new ALGETdC8001LineParser();
      _serialPortName = serialPortName;
    }


    public void Connect()
    {
      _serialPort = new SerialPort(_serialPortName, 9600, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;
      _serialPort.Open();
    }

    public void Disconnect()
    {
      _serialPort.Close();
      _serialPort = null;
    }

    public void Dispose()
    {
      Disconnect();
    }


    public void StartGetTimingData()
    {
      _serialPort.WriteLine("RSM");
      string response = _serialPort.ReadLine();
    }


    public IEnumerable<TimingData> TimingData()
    {
      do
      {

        ALGETdC8001LiveTimingData parsedData = null;
        try
        {
          string dataLine = _serialPort.ReadLine();
          if (dataLine.StartsWith("  ALGE-TIMING"))
          {
            // End of data => read two more lines
            _serialPort.ReadLine(); // "  TIMY V 1982"
            _serialPort.ReadLine(); // "20-10-04  16:54"
            break;
          }
          parsedData = _parser.Parse(dataLine);
        }
        catch (TimeoutException)
        {
          break; // no new data
        }
        catch (Exception)
        { }

        if (parsedData != null)
        {
          TimingData td = new TimingData
          {
            Time = parsedData.Time
          };

          yield return td;
        }
      } while (true);
    }
  }

}
