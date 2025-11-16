using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static RaceHorologyLib.PrintCertificateModel;

namespace RaceHorology
{
    public partial class CertificateDesignerUC : UserControl, INotifyPropertyChanged
    {

        private AppDataModel _dm;
        private Race _race;


        public event EventHandler Finished;


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
                                                     "<Datum>",});


        // In CertificateDesigner-Klasse:
        public ObservableCollection<FontFamily> SystemFonts { get; } =
            new ObservableCollection<FontFamily>(Fonts.SystemFontFamilies.OrderBy(f => f.Source));

        public PrintCertificateModel _certificateModel { get; set; }

        public CertificateDesignerUC()
        {
            InitializeComponent();

            DataContext = this;

            ApplyA4DesignSurface();
        }


        public void Init(AppDataModel dm, Race race)
        {
            _dm = dm;
            _race = race;

            _certificateModel = _race.GetDataModel().GetDB().GetCertificateModel(_race);

            RebuildOverlay();
        }

      
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _race.GetDataModel().GetDB().SaveCertificateModel(_race, _certificateModel);
            }
            catch (Exception ex)
            {

            }
        }

   

   

        // Handler: überschreibt sofort den Freitext von SelectedField
        private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedField == null) return;
            var cb = sender as ComboBox; if (cb == null) return;
            var token = cb.SelectedItem as string; if (string.IsNullOrEmpty(token)) return;
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
                if (SelectedFieldChanged != null) 
                    SelectedFieldChanged(this, value); }
        }

        public event EventHandler<TextItem> SelectedFieldChanged;


        private double _zoom = 0.75; // Startwert für Laptops
        private void ApplyZoom(double z)
        {
            if (z < 0.25) z = 0.25; if (z > 3.0) z = 3.0;
            _zoom = z;
            if (ZoomTransform != null) { ZoomTransform.ScaleX = z; ZoomTransform.ScaleY = z; }
        }

        // Preset aus ComboBox
        private void OnZoomPresetChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox; if (cb?.SelectedValue == null) return;
            double z; if (double.TryParse(cb.SelectedValue.ToString(), System.Globalization.NumberStyles.Any,
                                          System.Globalization.CultureInfo.InvariantCulture, out z))
                ApplyZoom(z);
        }

        // Fit-to-Window (passt A4 in die ScrollViewer-Viewportgröße)
        private void FitToWindow()
        {
            // ScrollViewer ist der direkte Parent in deinem Layout
            var sv = FindParent<ScrollViewer>(DesignFrame);
            if (sv == null) return;

            // A4-DIPs (wie von dir gesetzt)
            double wDip = DesignFrame.Width;  // ≈ 793.7
            double hDip = DesignFrame.Height; // ≈ 1122.5

            // etwas Rand einplanen
            double pad = 24;
            double availW = Math.Max(0, sv.ViewportWidth - pad);
            double availH = Math.Max(0, sv.ViewportHeight - pad);
            if (availW <= 0 || availH <= 0 || wDip <= 0 || hDip <= 0) return;

            double z = Math.Min(availW / wDip, availH / hDip);
            ApplyZoom(z);
        }

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
        private void OnRootSizeChanged(object sender, SizeChangedEventArgs e) => FitToWindow();

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

        public void SaveLayout(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            var ser = new DataContractJsonSerializer(typeof(ObservableCollection<TextItem>));
            ser.WriteObject(stream, _certificateModel.TextItems);
        }

        public void LoadLayout(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            var ser = new DataContractJsonSerializer(typeof(ObservableCollection<TextItem>));
            var loaded = ser.ReadObject(stream) as ObservableCollection<TextItem>;
            if (loaded != null)
            {
                _certificateModel.TextItems.Clear();
                foreach (var f in loaded) _certificateModel.TextItems.Add(f);
                RebuildOverlay();
            }
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
            //if (vm == null || el == null) return;

            //double maxX = Math.Max(0, OverlayCanvas.Width - el.ActualWidth);
            //double maxY = Math.Max(0, OverlayCanvas.Height - el.ActualHeight);

            //vm.HPos = (int)Math.Min(Math.Max(0, vm.HPos), maxX);
            //vm.VPos = (int)Math.Min(Math.Max(0, vm.VPos), maxY);

            //Canvas.SetLeft(el, vm.HPos);
            //Canvas.SetTop(el, vm.VPos);
            if (vm == null || el == null) return;

            // compute canvas limits (DIP)
            double maxXdip = Math.Max(0, OverlayCanvas.Width - el.ActualWidth);
            double maxYdip = Math.Max(0, OverlayCanvas.Height - el.ActualHeight);

            // convert model position (tenth mm → DIP)
            double wantedX = TenthMmToDip(vm.HPos);
            double wantedY = TenthMmToDip(vm.VPos);

            // clamp DIP values
            double clampedX = Math.Min(Math.Max(0, wantedX), maxXdip);
            double clampedY = Math.Min(Math.Max(0, wantedY), maxYdip);

            // apply to UI
            Canvas.SetLeft(el, clampedX);
            Canvas.SetTop(el, clampedY);

            // convert DIP → tenths of mm (round!) and write back
            int newH = DipToTenthMm(clampedX);
            int newV = DipToTenthMm(clampedY);

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
            text.Foreground = Brushes.Black;
            text.TextAlignment = ToTextAlignment(CertificatesUtils.mapAlignmentInt(vm.Alignment));


            text.FontFamily = new FontFamily(CertificatesUtils.MapFontFamily(vm.Font));   // NEU

            // bestehende Bindings
            text.SetBinding(TextBlock.TextProperty, new Binding("Text"));      // falls Merge, weglassen
            text.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSize"));



            // auf Änderungen reagieren
            vm.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "IsBold")
                    text.FontWeight = vm.IsBold ? FontWeights.Bold : FontWeights.Normal;
                else if (e.PropertyName == "TextAlignment")
                    text.TextAlignment = ToTextAlignment(CertificatesUtils.mapAlignmentInt(vm.Alignment));
                else if (e.PropertyName == "FontFamilyName")       // NEU
                    text.FontFamily = new FontFamily(vm.FontFamilyName);
            };

            // Drag behavior on a Border container
            var border = new Border { Background = Brushes.Transparent, Child = text, Padding = new Thickness(2) };
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
                //vm.HPos = (int)Canvas.GetLeft(border);
                //vm.VPos = (int)Canvas.GetTop(border);
                vm.HPos = DipToTenthMm(Canvas.GetLeft(border));
                vm.VPos = DipToTenthMm(Canvas.GetTop(border));
            };

            border.MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs e)
            {
                if (!dragging) return;
                dragging = false;
                border.ReleaseMouseCapture();
                e.Handled = true;
            };



            vm.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "HPos" || e.PropertyName == "VPos")
                    ClampVmToCanvas(vm, border);

            };

            return border;
        }

        private static TextAlignment ToTextAlignment(int value)
        {
            if (value==1) return TextAlignment.Center;
            if (value==2) return TextAlignment.Right;
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


        private void OnSaveLayout_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Layout JSON|*.json" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                        SaveLayout(fs);
                }
                catch (Exception ex) { MessageBox.Show("Save failed: " + ex.Message); }
            }
        }

        private void OnLoadLayout_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Layout JSON|*.json" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                        LoadLayout(fs);
                }
                catch (Exception ex) { MessageBox.Show("Load failed: " + ex.Message); }
            }
        }

        private void OnAddField(object sender, RoutedEventArgs e)
        {

            var f = new TextItem { Text = "Neues Feld", HPos = 50, VPos = 50};
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
    }

    //[DataContract]
    //public class FieldVM : INotifyPropertyChanged
    //{
    //    protected bool SetField<T>(ref T storage, T value, [CallerMemberName] string prop = null)
    //    {
    //        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
    //        storage = value;
    //        OnPropertyChanged(prop);
    //        return true;
    //    }

    //    [DataMember(Order = 0)] public string Key { get; set; }
    //    [DataMember(Order = 1)] public string Label { get; set; }

    //    private string _value = string.Empty;
    //    [DataMember(Order = 2)] public string Value { get { return _value; } set { _value = value; OnPropertyChanged(); } }

    //    private double _x;
    //    [DataMember(Order = 3)] public double X { get => _x; set => SetField(ref _x, value); }

    //    private double _y;
    //    [DataMember(Order = 4)] public double Y { get => _y; set => SetField(ref _y, value); }

    //    private double _fontSize = 24.0;
    //    [DataMember(Order = 5)] public double FontSize { get { return _fontSize; } set { _fontSize = value; OnPropertyChanged(); } }

    //    private bool _isBold;
    //    [DataMember(Order = 6)] public bool IsBold { get { return _isBold; } set { _isBold = value; OnPropertyChanged(); } }

    //    private string _textAlignment = "Left"; // "Left", "Center", "Right"
    //    [DataMember(Order = 7)] public string TextAlignment { get { return _textAlignment; } set { _textAlignment = value; OnPropertyChanged(); } }

    //    // In FieldVM:
    //    [DataMember(Order = 8)]
    //    public string FontFamilyName
    //    {
    //        get { return _fontFamilyName; }
    //        set { _fontFamilyName = value; OnPropertyChanged(); }
    //    }
    //    private string _fontFamilyName = "Segoe UI"; // Default


    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void OnPropertyChanged([CallerMemberName] string n = null)
    //    { var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(n)); }
    //}

}