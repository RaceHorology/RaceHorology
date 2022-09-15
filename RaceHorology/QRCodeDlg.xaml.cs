using QRCoder;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for QRCodeDlg.xaml
  /// </summary>
  public partial class QRCodeDlg : Window
  {
    DSVAlpin2HTTPServer _server;

    public QRCodeDlg(DSVAlpin2HTTPServer server)
    {
      _server = server;
      InitializeComponent();

      imgQRCode.Source = QRCodeUtils.GetUrlQR(server);
    }

    private void onClickQRCode(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Process.Start(_server.GetUrl());
    }
  }


  public static class QRCodeUtils
  {
    static public BitmapImage GetUrlQR(DSVAlpin2HTTPServer alpinServer)
    {
      if (alpinServer != null)
      {
        string url = alpinServer.GetUrl();

        if (!string.IsNullOrEmpty(url))
        {
          QRCodeGenerator qrGenerator = new QRCodeGenerator();
          QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
          QRCode qrCode = new QRCode(qrCodeData);
          System.Drawing.Bitmap bitmap = qrCode.GetGraphic(10);

          BitmapImage bitmapimage = new BitmapImage();
          using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
          {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
          }

          return bitmapimage;
        }
      }
      return null;
    }
  }
}
