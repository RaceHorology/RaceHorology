using Microsoft.Win32;
using RaceHorologyLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static RaceHorologyLib.PrintCertificateModel;

namespace RaceHorology
{
  public partial class CertificateDesignerUC : UserControl, INotifyPropertyChanged
  {
    private AppDataModel _dm;
    private Race _race;

    private string _feedBackText = string.Empty;
    public string FeedBackText { get { return _feedBackText; } set { _feedBackText = value; OnPropertyChanged(); } }

    // in der Klasse CertificateDesigner hinzufügen:
    public ObservableCollection<string> Presets { get; } =
      new ObservableCollection<string>(new[] { "<Vorname Name>",
                                                "<Vorname>",
                                                "<Nachname>",
                                                "<Verein>",
                                                "<Klasse>",
                                                "<Disziplin>",
                                                "<Platz>",
                                                "<Zeit>",
                                                "<Berwerbsdatum>",
                                                "<Datum>",
      });


    // In CertificateDesigner-Klasse:
    public ObservableCollection<FontFamily> SystemFonts { get; } =
      new ObservableCollection<FontFamily>(Fonts.SystemFontFamilies.OrderBy(f => f.Source));

    private bool _showHelperLines = false;


    public PrintCertificateModel _certificateModel { get; set; }

    public CertificateDesignerUC()
    {
      InitializeComponent();

      DataContext = this;

      ApplyA4DesignSurface();

      ucSaveOrReset.Init("Urkunde", null, null, HasChanges, StoreCertificateDesign, RestoreCertificateDesign, true);

      DesignFrame.SizeChanged += (_, __) => DrawHelperLines();
    }


    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;

      _certificateModel = _race.GetDataModel().GetDB().GetCertificateModel(_race);


      RebuildOverlay();

      ApplyZoom(_zoom);
    }


    public void StoreCertificateDesign()
    {
      _race.GetDataModel().GetDB().SaveCertificateModel(_race, _certificateModel);
    }
    public void RestoreCertificateDesign()
    {
      _certificateModel = _race.GetDataModel().GetDB().GetCertificateModel(_race);
      RebuildOverlay();
    }

    public bool HasChanges()
    {
      var orgModel = _race.GetDataModel().GetDB().GetCertificateModel(_race);
      return !PrintCertificateModel.AreEqual(orgModel, _certificateModel);
    }

    public void SaveOrResetNow()
    {
      ucSaveOrReset.SaveOrResetNow();
    }

    // Handler: überschreibt sofort den Freitext von SelectedField
    private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (SelectedField == null)
        return;
      var cb = sender as ComboBox;
      if (cb == null)
        return;

      var token = cb.SelectedItem as string;

      if (string.IsNullOrEmpty(token))
        return;

      SelectedField.Text = token; // z.B. "<Date>"
    }


    private TextItem _selectedField;
    public TextItem SelectedField
    {
      get { return _selectedField; }
      set
      {
        _selectedField = value;
        OnPropertyChanged();
        UpdateSelectionBorders();

        if (SelectedFieldChanged != null)
          SelectedFieldChanged(this, value);
      }
    }

    public event EventHandler<TextItem> SelectedFieldChanged;


    private double _zoom = 0.5; // Startwert für Laptops
    private void ApplyZoom(double z)
    {
      if (z < 0.25) z = 0.25; if (z > 3.0) z = 3.0;
      _zoom = z;
      if (ZoomTransform != null) { ZoomTransform.ScaleX = z; ZoomTransform.ScaleY = z; }
    }

    // Preset aus ComboBox
    private void OnZoomPresetChanged(object sender, SelectionChangedEventArgs e)
    {
      var cb = sender as ComboBox;

      if (cb?.SelectedValue == null)
        return;

      double z;

      if (double.TryParse(cb.SelectedValue.ToString(), System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out z))
        ApplyZoom(z);

      DrawHelperLines();
    }

    // Fit-to-Window (passt A4 in die ScrollViewer-Viewportgröße)
    //private void FitToWindow()
    //{
    //    // ScrollViewer ist der direkte Parent in deinem Layout
    //    var sv = FindParent<ScrollViewer>(DesignFrame);
    //    if (sv == null) return;

    //    // A4-DIPs (wie von dir gesetzt)
    //    double wDip = DesignFrame.Width;  // ≈ 793.7
    //    double hDip = DesignFrame.Height; // ≈ 1122.5

    //    // etwas Rand einplanen
    //    double pad = 24;
    //    double availW = Math.Max(0, sv.ViewportWidth - pad);
    //    double availH = Math.Max(0, sv.ViewportHeight - pad);
    //    if (availW <= 0 || availH <= 0 || wDip <= 0 || hDip <= 0) return;

    //    double z = Math.Min(availW / wDip, availH / hDip);
    //    ApplyZoom(z);
    //}

    // Helper: Parent suchen
    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
      DependencyObject p = VisualTreeHelper.GetParent(child);
      while (p != null && !(p is T)) p = VisualTreeHelper.GetParent(p);
      return p as T;
    }
    public static double TenthMmToDip(int tenthMm)
    {
      return (tenthMm / 10.0) * (DpiDip / 25.4);
    }

    public static int DipToTenthMm(double dip)
    {
      return (int)Math.Round((dip * 25.4 / DpiDip) * 10.0);
    }

    // Bei Größenänderung automatisch anpassen (optional)
    //private void OnRootSizeChanged(object sender, SizeChangedEventArgs e) => FitToWindow();

    // Konstanten: DIN A4 in DIPs (1 DIP = 1/96")
    const double DpiDip = 96.0;
    const double MM_TO_DIP = DpiDip / 25.4;        // 1 mm
    const double TENTH_MM_TO_DIP = MM_TO_DIP / 10; // 0.1 mm
    const double A4WidthIn = 210.0 / 25.4;  // 210 mm
    const double A4HeightIn = 297.0 / 25.4;  // 297 mm
    static readonly double A4WidthDip = A4WidthIn * DpiDip; // ≈ 793.7
    static readonly double A4HeightDip = A4HeightIn * DpiDip; // ≈ 1122.5

    private void ApplyA4DesignSurface()
    {
      // Rahmen/Bereich
      DesignFrame.Width = A4WidthDip;
      DesignFrame.Height = A4HeightDip;

      // Innenleben exakt gleich groß halten
      BackgroundImage.Width = A4WidthDip;
      BackgroundImage.Height = A4HeightDip;

      OverlayCanvas.Width = A4WidthDip;
      OverlayCanvas.Height = A4HeightDip;

      // ... Width/Height für BackgroundImage & OverlayCanvas setzen ...
      OverlayCanvas.Clip = new RectangleGeometry(new Rect(0, 0, OverlayCanvas.Width, OverlayCanvas.Height));
    }



    // ===================== Public API =====================
    public void SetBackground(BitmapSource bmp)
    {
      if (bmp == null) throw new ArgumentNullException("bmp");
      BackgroundImage.Source = bmp;
      ApplyA4DesignSurface();
      RebuildOverlay();
    }

    public void LoadBackground(string filePath)
    {
      if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("filePath is null or empty");
      var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

      if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
      {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new Uri(filePath);
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();
        SetBackground(bmp);
        return;
      }

      throw new NotSupportedException("Unsupported background format: " + ext);
    }

    public RenderTargetBitmap RenderComposite(double dpi)
    {
      if (BackgroundImage.Source == null) throw new InvalidOperationException("Background not set");
      double w = OverlayCanvas.Width;
      double h = OverlayCanvas.Height;

      var rtb = new RenderTargetBitmap(
          (int)(w * dpi / 96.0),
          (int)(h * dpi / 96.0),
          dpi, dpi,
          PixelFormats.Pbgra32);

      var grid = new Grid { Width = w, Height = h };
      grid.Children.Add(new Image { Source = BackgroundImage.Source, Width = w, Height = h });
      grid.Children.Add(new Rectangle { Width = w, Height = h, Fill = new VisualBrush(OverlayCanvas), IsHitTestVisible = false });
      grid.Measure(new Size(w, h));
      grid.Arrange(new Rect(0, 0, w, h));
      rtb.Render(grid);
      rtb.Freeze();
      return rtb;
    }
    // =================== End Public API ===================

    private void RebuildOverlay()
    {
      OverlayCanvas.Children.Clear();
      for (int i = 0; i < _certificateModel.TextItems.Count; i++)
        OverlayCanvas.Children.Add(CreateDraggable(_certificateModel.TextItems[i]));
    }

    private static double GetAnchorOffsetDip(TextItem vm, double widthDip)
    {
      // vm.Alignment: Left = 0, Right = 1, Center = 2 (bei dir)
      if (vm.Alignment == TextItemAlignment.Center) return widthDip / 2.0;
      if (vm.Alignment == TextItemAlignment.Right) return widthDip;
      return 0.0; // Left
    }

    private static double MeasureWidthDip(FrameworkElement el)
    {
      var w = el.ActualWidth;
      if (w <= 0)
      {
        el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        w = el.DesiredSize.Width;
      }
      return w;
    }
    private void SetClampedPosition(FrameworkElement el, double x, double y)
    {
      if (OverlayCanvas == null) return;

      double maxX = Math.Max(0, OverlayCanvas.Width - el.ActualWidth);
      double maxY = Math.Max(0, OverlayCanvas.Height - el.ActualHeight);

      x = Math.Min(Math.Max(0, x), maxX);
      y = Math.Min(Math.Max(0, y), maxY);

      Canvas.SetLeft(el, x);
      Canvas.SetTop(el, y);
    }

    private void ClampVmToCanvas(TextItem vm, FrameworkElement el)
    {
      if (vm == null || el == null || OverlayCanvas == null) return;

      double wDip = MeasureWidthDip(el);
      double hDip = el.ActualHeight > 0 ? el.ActualHeight : el.DesiredSize.Height;

      // Anker in DIP
      double ax = TenthMmToDip(vm.HPos);
      double ay = TenthMmToDip(vm.VPos);

      // je nach Alignment hat der Anker andere erlaubte Grenzen
      double minAx = 0, maxAx = OverlayCanvas.Width;
      if (vm.Alignment == TextItemAlignment.Left)
      {
        minAx = 0;
        maxAx = Math.Max(0, OverlayCanvas.Width - wDip);
      }
      else if (vm.Alignment == TextItemAlignment.Center)
      {
        minAx = wDip / 2.0;
        maxAx = Math.Max(minAx, OverlayCanvas.Width - wDip / 2.0);
      }
      else if (vm.Alignment == TextItemAlignment.Right)
      {
        minAx = wDip;
        maxAx = Math.Max(minAx, OverlayCanvas.Width);
      }

      // Y-Anker ist bei dir TOP (VerticalAlignment.TOP): top = ay
      double maxAy = Math.Max(0, OverlayCanvas.Height - hDip);
      ay = Math.Min(Math.Max(0, ay), maxAy);

      ax = Math.Min(Math.Max(minAx, ax), maxAx);

      // Canvas.Left aus Anker berechnen
      double left = ax - GetAnchorOffsetDip(vm, wDip);

      Canvas.SetLeft(el, left);
      Canvas.SetTop(el, ay);

      // zurückschreiben (nur wenn geändert)
      int newH = DipToTenthMm(ax);
      int newV = DipToTenthMm(ay);
      if (newH != vm.HPos) vm.HPos = newH;
      if (newV != vm.VPos) vm.VPos = newV;
    }

    private FrameworkElement CreateDraggable(TextItem vm)
    {
      var text = new TextBlock();
      text.DataContext = vm;
      text.Text = vm.Text;                 // oder ResolveEffectiveText(vm) falls du Merge hast
      text.FontSize = vm.FontSize;
      text.FontWeight = vm.IsBold ? FontWeights.Bold : FontWeights.Normal;
      text.FontStyle = vm.IsItalic ? FontStyles.Italic : FontStyles.Normal;
      text.Foreground = Brushes.Black;
      text.TextAlignment = ToTextAlignment(CertificatesUtils.mapAlignmentInt(vm.Alignment));

      text.FontFamily = new FontFamily(CertificatesUtils.MapFontFamily(vm.Font));   // NEU

      // bestehende Bindings
      text.SetBinding(TextBlock.TextProperty, new Binding("Text"));      // falls Merge, weglassen
      text.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSizeDip"));


      // auf Änderungen reagieren
      vm.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
      {
        if (e.PropertyName == "IsBold")
          text.FontWeight = vm.IsBold ? FontWeights.Bold : FontWeights.Normal;
        else if (e.PropertyName == "IsItalic")
          text.FontStyle = vm.IsItalic ? FontStyles.Italic : FontStyles.Normal;
        else if (e.PropertyName == "TextAlignment")
          text.TextAlignment = ToTextAlignment(CertificatesUtils.mapAlignmentInt(vm.Alignment));
        else if (e.PropertyName == "FontFamilyName")       // NEU
          text.FontFamily = new FontFamily(vm.FontFamilyName);
      };

      // Drag behavior on a Border container  
      var border = new Border
      {
        Background = Brushes.Transparent,
        Child = text,
        Padding = new Thickness(2),
        BorderBrush = Brushes.Transparent,   // default = no visible border
        BorderThickness = new Thickness(1)
      };

      border.Tag = vm;

      // initial setzen (nachdem border geladen ist, damit ActualWidth/ActualHeight stimmen)
      border.Loaded += (s, e) => ClampVmToCanvas(vm, border);
      // bei Größenänderung (FontSize, FontFamily etc.) neu clampen
      border.SizeChanged += (s, e) => ClampVmToCanvas(vm, border);

      // Startposition
      //Canvas.SetLeft(border, vm.HPos);
      //Canvas.SetTop(border, vm.VPos);
      Canvas.SetLeft(border, TenthMmToDip(vm.HPos));
      Canvas.SetTop(border, TenthMmToDip(vm.VPos));

      Point dragStart = new Point();
      bool dragging = false;
      double startLeft = 0.0, startTop = 0.0;

      border.MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs e)
      {
        SelectedField = vm;
        UpdateSelectionBorders();
        dragging = true;
        dragStart = e.GetPosition(OverlayCanvas);
        startLeft = Canvas.GetLeft(border); if (double.IsNaN(startLeft)) startLeft = 0.0;
        startTop = Canvas.GetTop(border); if (double.IsNaN(startTop)) startTop = 0.0;
        border.CaptureMouse();
        e.Handled = true;
      };

      // Drag
      border.MouseMove += delegate (object s, MouseEventArgs e)
      {
        if (!dragging) return;
        Point pos = e.GetPosition(OverlayCanvas);
        double x = startLeft + (pos.X - dragStart.X);
        double y = startTop + (pos.Y - dragStart.Y);
        SetClampedPosition(border, x, y);

        double wDip = MeasureWidthDip(border);
        double left = Canvas.GetLeft(border);
        double top = Canvas.GetTop(border);

        double ax = left + GetAnchorOffsetDip(vm, wDip); // Anker = left + offset
        double ay = top;

        vm.HPos = DipToTenthMm(ax);
        vm.VPos = DipToTenthMm(ay);
      };

      border.MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs e)
      {
        if (!dragging) return;
        dragging = false;
        border.ReleaseMouseCapture();
        e.Handled = true;
      };

      border.MouseEnter += (_, __) =>
      {
        if (vm != SelectedField)
          border.BorderBrush = Brushes.LightBlue;
      };

      border.MouseLeave += (_, __) =>
      {
        if (vm != SelectedField)
          border.BorderBrush = Brushes.Transparent;
      };


      vm.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
      {

        if (e.PropertyName == "HPos" || e.PropertyName == "VPos" || e.PropertyName == "Alignment")
          ClampVmToCanvas(vm, border);

      };


      return border;
    }

    private void UpdateSelectionBorders()
    {
      foreach (var child in OverlayCanvas.Children.OfType<Border>())
      {
        if (child.Tag is TextItem item)
        {
          bool selected = item == SelectedField;

          child.BorderBrush = selected ? Brushes.DodgerBlue : Brushes.Transparent;
          child.BorderThickness = selected ? new Thickness(1.5) : new Thickness(1);
        }
      }
    }
    public Array AlignmentValues => Enum.GetValues(typeof(PrintCertificateModel.TextItemAlignment));

    private static TextAlignment ToTextAlignment(int value)
    {
      if (value == 1) return TextAlignment.Center;
      if (value == 2) return TextAlignment.Right;
      return TextAlignment.Left;
    }

    // ============ OPTIONAL: handlers for toolbar/buttons ============
    private void OnLoadImageBackground_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg" };
      if (dlg.ShowDialog() == true)
      {
        try { LoadBackground(dlg.FileName); }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
      }
    }

    private void OnAddField(object sender, RoutedEventArgs e)
    {

      var f = new TextItem { Text = "Neues Feld", HPos = 50, VPos = 50 };
      _certificateModel.TextItems.Add(f);
      SelectedField = f;
      RebuildOverlay();
    }

    private void OnDeleteField(object sender, RoutedEventArgs e)
    {
      if (SelectedField == null)
      {
        MessageBox.Show("Kein Feld ausgewählt.");
        return;
      }

      // Optional: Sicherheitsabfrage
      var ok = MessageBox.Show($"Feld \"{SelectedField.Text}\" löschen?",
          "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
      if (ok != MessageBoxResult.Yes) return;

      var victim = SelectedField;
      _certificateModel.TextItems.Remove(victim);
      SelectedField = null;
      RebuildOverlay();
    }


    private void OnHelperLinesToggled(object sender, RoutedEventArgs e)
    {
      _showHelperLines = cbHelperLines.IsChecked == true;
      DrawHelperLines();
    }

    private void DrawHelperLines()
    {
      if (HelperLinesCanvas == null || DesignFrame == null)
        return;

      HelperLinesCanvas.Children.Clear();

      if (!_showHelperLines)
        return;

      // Your logical grid step in tenth-mm
      const int GRID_TENTH_MM = 25;   // or 5 or 25 as you prefer

      // Convert to DIP for drawing
      double spacingDip = TenthMmToDip(GRID_TENTH_MM);

      double width = DesignFrame.ActualWidth;
      double height = DesignFrame.ActualHeight;

      if (width <= 0 || height <= 0)
        return;

      // Vertical lines
      for (double x = spacingDip; x < width; x += spacingDip)
      {
        HelperLinesCanvas.Children.Add(new Line
        {
          X1 = x,
          Y1 = 0,
          X2 = x,
          Y2 = height,
          Stroke = Brushes.DarkGray,
          StrokeThickness = 1,
          Opacity = 0.5
        });
      }

      // Horizontal lines
      for (double y = spacingDip; y < height; y += spacingDip)
      {
        HelperLinesCanvas.Children.Add(new Line
        {
          X1 = 0,
          Y1 = y,
          X2 = width,
          Y2 = y,
          Stroke = Brushes.DarkGray,
          StrokeThickness = 1,
          Opacity = 0.5
        });
      }

      // Center lines
      HelperLinesCanvas.Children.Add(new Line
      {
        X1 = width / 2,
        Y1 = 0,
        X2 = width / 2,
        Y2 = height,
        Stroke = Brushes.Red,
        StrokeThickness = 1.5,
        StrokeDashArray = new DoubleCollection { 4, 4 }
      });

      HelperLinesCanvas.Children.Add(new Line
      {
        X1 = 0,
        Y1 = height / 2,
        X2 = width,
        Y2 = height / 2,
        Stroke = Brushes.Red,
        StrokeThickness = 1.5,
        StrokeDashArray = new DoubleCollection { 4, 4 }
      });
    }

    const int GRID = 5;   // or 10 — set what you prefer
    const int SMALL_MOVE = GRID;   // or 10 — set what you prefer
    const int LARGE_MOVE = GRID * 5;   // or 10 — set what you prefer
    private int SnapNearest(int value, int grid)
    {
      return (int)(Math.Round(value / (double)grid) * grid);
    }

    private void MoveField(double dx, double dy, int step)
    {
      if (SelectedField == null)
        return;

      const int GRID = 5;  // or 10, depending on your grid spacing

      // Snap to nearest grid (in tenth-mm)
      SelectedField.HPos = SnapNearest(SelectedField.HPos, GRID);
      SelectedField.VPos = SnapNearest(SelectedField.VPos, GRID);

      // Move directly in tenth-mm
      SelectedField.HPos += (int)(dx * step);
      SelectedField.VPos += (int)(dy * step);
    }

    private void MoveUpSmall(object s, RoutedEventArgs e) => MoveField(0, -1, SMALL_MOVE);
    private void MoveDownSmall(object s, RoutedEventArgs e) => MoveField(0, 1, SMALL_MOVE);
    private void MoveLeftSmall(object s, RoutedEventArgs e) => MoveField(-1, 0, SMALL_MOVE);
    private void MoveRightSmall(object s, RoutedEventArgs e) => MoveField(1, 0, SMALL_MOVE);

    private void MoveUpLeftSmall(object s, RoutedEventArgs e) => MoveField(-1, -1, SMALL_MOVE);
    private void MoveUpRightSmall(object s, RoutedEventArgs e) => MoveField(1, -1, SMALL_MOVE);
    private void MoveDownLeftSmall(object s, RoutedEventArgs e) => MoveField(-1, 1, SMALL_MOVE);
    private void MoveDownRightSmall(object s, RoutedEventArgs e) => MoveField(1, 1, SMALL_MOVE);

    private void MoveUpLarge(object s, RoutedEventArgs e) => MoveField(0, -1, LARGE_MOVE);
    private void MoveDownLarge(object s, RoutedEventArgs e) => MoveField(0, 1, LARGE_MOVE);
    private void MoveLeftLarge(object s, RoutedEventArgs e) => MoveField(-1, 0, LARGE_MOVE);
    private void MoveRightLarge(object s, RoutedEventArgs e) => MoveField(1, 0, LARGE_MOVE);

    private void MoveUpLeftLarge(object s, RoutedEventArgs e) => MoveField(-1, -1, LARGE_MOVE);
    private void MoveUpRightLarge(object s, RoutedEventArgs e) => MoveField(1, -1, LARGE_MOVE);
    private void MoveDownLeftLarge(object s, RoutedEventArgs e) => MoveField(-1, 1, LARGE_MOVE);
    private void MoveDownRightLarge(object s, RoutedEventArgs e) => MoveField(1, 1, LARGE_MOVE);

    private void CenterHorizontally(object s, RoutedEventArgs e)
    {
      if (SelectedField == null) return;

      // Alignment setzen
      SelectedField.Alignment = PrintCertificateModel.TextItemAlignment.Center;
      // Seitenmitte in DIP -> in 1/10 mm (HPos)
      double midDip = (DesignFrame.ActualWidth > 0 ? DesignFrame.ActualWidth : A4WidthDip) / 2.0;
      SelectedField.HPos = DipToTenthMm(midDip);

    }

    private void AllignLeft(object s, RoutedEventArgs e)
    {
      if (SelectedField == null) return;

      // Alignment setzen
      SelectedField.Alignment = PrintCertificateModel.TextItemAlignment.Left;

      // Seitenmitte in DIP -> in 1/10 mm (HPos)
      double midDip = ((DesignFrame.ActualWidth > 0 ? DesignFrame.ActualWidth : A4WidthDip) * 5.0) / 100.0;
      SelectedField.HPos = DipToTenthMm(midDip);

    }

    private void AllignRight(object s, RoutedEventArgs e)
    {
      if (SelectedField == null) return;

      // Alignment setzen
      SelectedField.Alignment = PrintCertificateModel.TextItemAlignment.Right;

      // Seitenmitte in DIP -> in 1/10 mm (HPos)
      double midDip = ((DesignFrame.ActualWidth > 0 ? DesignFrame.ActualWidth : A4WidthDip) * 95.0) / 100.0;
      SelectedField.HPos = DipToTenthMm(midDip);

    }

    private double GetFieldWidth(TextItem vm)
    {
      foreach (var child in OverlayCanvas.Children.OfType<Border>())
        if (child.Tag == vm)
          return child.ActualWidth;

      return 0;
    }

    // ================================================================

#if PDFIUM
        private static BitmapImage ConvertDrawingBitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
        }
#endif


    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string n = null)
    { var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(n)); }

    private void btnImport_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter =
        "Race Horology Daten|*.mdb|DSValpin Daten|*.mdb";
      if (openFileDialog.ShowDialog() == true)
      {
        Database importDB = new Database();
        importDB.Connect(openFileDialog.FileName);
        AppDataModel importModel = new AppDataModel(importDB);
        var races = importModel.GetRaces();

        var dlg = new CertificateDesignerImportDlg(importModel);
        dlg.Owner = Window.GetWindow(this);
        dlg.ShowDialog();
      }
    }
  }
}
