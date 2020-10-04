using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class TimingData
  {
    public TimeSpan? Time { get; set; }
  }


  public class ALGETimy
  {
    ALGETdC8001LineParser _parser;
    private SerialPort _serialPort;

    public ALGETimy(string serialPortName)
    {
      _parser = new ALGETdC8001LineParser();

      _serialPort = new SerialPort(serialPortName, 9600, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;
      _serialPort.Open();

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
        string dataLine = _serialPort.ReadLine();

        ALGETdC8001LiveTimingData parsedData = null;
        try
        {
          parsedData = _parser.Parse(dataLine);
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
