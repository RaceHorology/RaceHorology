using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
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
using RaceHorologyLib;
using System.ComponentModel.Design;

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


        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Finished?.Invoke(this, new EventArgs());
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Finished?.Invoke(this, new EventArgs());
        }

        // (Optional) eigene Liste setzen:
        public void SetFontFamilies(IEnumerable<FontFamily> fonts)
        {
            SystemFonts.Clear();
            if (fonts == null) return;
            foreach (var f in fonts) SystemFonts.Add(f);
        }
        public void SetPresets(IEnumerable<string> tokens)
        {
            Presets.Clear();
            if (tokens == null) return;
            foreach (var t in tokens)
                if (!string.IsNullOrWhiteSpace(t)) Presets.Add(t);
        }

        // Handler: überschreibt sofort den Freitext von SelectedField
        private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedField == null) return;
            var cb = sender as ComboBox; if (cb == null) return;
            var token = cb.SelectedItem as string; if (string.IsNullOrEmpty(token)) return;
            SelectedField.Value = token; // z.B. "<Date>"
        }

        private void OnPresetClear(object sender, RoutedEventArgs e)
        {
            if (SelectedField == null) return;
            SelectedField.Value = string.Empty;
        }

        public ObservableCollection<FieldVM> Fields { get; private set; } = new ObservableCollection<FieldVM>();

        private FieldVM _selectedField;
        public FieldVM SelectedField
        {
            get { return _selectedField; }
            set { _selectedField = value; OnPropertyChanged(); if (SelectedFieldChanged != null) SelectedFieldChanged(this, value); }
        }

        public event EventHandler<FieldVM> SelectedFieldChanged;


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

        // Bei Größenänderung automatisch anpassen (optional)
        private void OnRootSizeChanged(object sender, SizeChangedEventArgs e) => FitToWindow();

        // Konstanten: DIN A4 in DIPs (1 DIP = 1/96")
        const double DpiDip = 96.0;
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

        // Optional: mm → DIP (für numerische Platzierung in mm)
        public static double MmToDip(double mm) => (mm / 25.4) * DpiDip;
        public static Point MmToDip(double mmX, double mmY) => new Point(MmToDip(mmX), MmToDip(mmY));


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

#if PDFIUM
            if (ext == ".pdf")
            {
                try
                {
                    using (var doc = PdfiumViewer.PdfDocument.Load(filePath))
                    using (var img = doc.Render(0, 300, 300, true)) // first page @300 DPI
                    {
                        var bi = ConvertDrawingBitmapToBitmapImage(img);
                        SetBackground(bi);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to render PDF: " + ex.Message);
                    return;
                }
            }
#endif
            throw new NotSupportedException("Unsupported background format: " + ext);
        }

        public void SaveLayout(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            var ser = new DataContractJsonSerializer(typeof(ObservableCollection<FieldVM>));
            ser.WriteObject(stream, Fields);
        }

        public void LoadLayout(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            var ser = new DataContractJsonSerializer(typeof(ObservableCollection<FieldVM>));
            var loaded = ser.ReadObject(stream) as ObservableCollection<FieldVM>;
            if (loaded != null)
            {
                Fields.Clear();
                foreach (var f in loaded) Fields.Add(f);
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
            for (int i = 0; i < Fields.Count; i++)
                OverlayCanvas.Children.Add(CreateDraggable(Fields[i]));
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

        private void ClampVmToCanvas(FieldVM vm, FrameworkElement el)
        {
            if (vm == null || el == null) return;

            double maxX = Math.Max(0, OverlayCanvas.Width - el.ActualWidth);
            double maxY = Math.Max(0, OverlayCanvas.Height - el.ActualHeight);

            vm.X = Math.Min(Math.Max(0, vm.X), maxX);
            vm.Y = Math.Min(Math.Max(0, vm.Y), maxY);

            Canvas.SetLeft(el, vm.X);
            Canvas.SetTop(el, vm.Y);
        }

        private FrameworkElement CreateDraggable(FieldVM vm)
        {
            var text = new TextBlock();
            text.DataContext = vm;
            text.Text = vm.Value;                 // oder ResolveEffectiveText(vm) falls du Merge hast
            text.FontSize = vm.FontSize;
            text.FontWeight = vm.IsBold ? FontWeights.Bold : FontWeights.Normal;
            text.Foreground = Brushes.Black;
            text.TextAlignment = ToTextAlignment(vm.TextAlignment);
            text.FontFamily = new FontFamily(vm.FontFamilyName);   // NEU

            // bestehende Bindings
            text.SetBinding(TextBlock.TextProperty, new Binding("Value"));      // falls Merge, weglassen
            text.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSize"));



            // auf Änderungen reagieren
            vm.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "IsBold")
                    text.FontWeight = vm.IsBold ? FontWeights.Bold : FontWeights.Normal;
                else if (e.PropertyName == "TextAlignment")
                    text.TextAlignment = ToTextAlignment(vm.TextAlignment);
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
            Canvas.SetLeft(border, vm.X);
            Canvas.SetTop(border, vm.Y);

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
                vm.X = Canvas.GetLeft(border);
                vm.Y = Canvas.GetTop(border);
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
                if (e.PropertyName == "X" || e.PropertyName == "Y")
                    ClampVmToCanvas(vm, border);

            };

            return border;
        }

        private static TextAlignment ToTextAlignment(string value)
        {
            if (string.Equals(value, "Center", StringComparison.OrdinalIgnoreCase)) return TextAlignment.Center;
            if (string.Equals(value, "Right", StringComparison.OrdinalIgnoreCase)) return TextAlignment.Right;
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

        private void OnLoadPdfBackground_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "PDF|*.pdf" };
            if (dlg.ShowDialog() == true)
            {
#if PDFIUM
                try { LoadBackground(dlg.FileName); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
#else
                MessageBox.Show("To enable PDF preview, install PdfiumViewer.Core + PdfiumCore.Native and define PDFIUM.");
#endif
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

            var f = new FieldVM { Key = "Custom", Label = "Custom", Value = "New Field", X = 50, Y = 50, FontSize = 24 };
            Fields.Add(f);
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
            var ok = MessageBox.Show($"Feld \"{SelectedField.Label}\" löschen?",
                "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (ok != MessageBoxResult.Yes) return;

            var victim = SelectedField;
            Fields.Remove(victim);
            SelectedField = null;
            RebuildOverlay();
        }

        private void OnExportPng(object sender, RoutedEventArgs e)
        {
            if (BackgroundImage.Source == null) { MessageBox.Show("Please load a background first."); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PNG Image|*.png" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var rtb = RenderComposite(300);
                    using (var fs = new FileStream(dlg.FileName, FileMode.Create))
                    {
                        var enc = new PngBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(rtb));
                        enc.Save(fs);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Export failed: " + ex.Message); }
            }
        }

        private void OnPrint(object sender, RoutedEventArgs e)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                double w = OverlayCanvas.Width, h = OverlayCanvas.Height;
                var visual = new Grid { Width = w, Height = h };
                visual.Children.Add(new Image { Source = BackgroundImage.Source, Width = w, Height = h });
                visual.Children.Add(new Rectangle { Width = w, Height = h, Fill = new VisualBrush(OverlayCanvas) });
                visual.Measure(new Size(w, h));
                visual.Arrange(new Rect(0, 0, w, h));
                pd.PrintVisual(visual, "Ski Certificate");
            }
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

    [DataContract]
    public class FieldVM : INotifyPropertyChanged
    {
        protected bool SetField<T>(ref T storage, T value, [CallerMemberName] string prop = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(prop);
            return true;
        }

        [DataMember(Order = 0)] public string Key { get; set; }
        [DataMember(Order = 1)] public string Label { get; set; }

        private string _value = string.Empty;
        [DataMember(Order = 2)] public string Value { get { return _value; } set { _value = value; OnPropertyChanged(); } }

        private double _x;
        [DataMember(Order = 3)] public double X { get => _x; set => SetField(ref _x, value); }

        private double _y;
        [DataMember(Order = 4)] public double Y { get => _y; set => SetField(ref _y, value); }

        private double _fontSize = 24.0;
        [DataMember(Order = 5)] public double FontSize { get { return _fontSize; } set { _fontSize = value; OnPropertyChanged(); } }

        private bool _isBold;
        [DataMember(Order = 6)] public bool IsBold { get { return _isBold; } set { _isBold = value; OnPropertyChanged(); } }

        private string _textAlignment = "Left"; // "Left", "Center", "Right"
        [DataMember(Order = 7)] public string TextAlignment { get { return _textAlignment; } set { _textAlignment = value; OnPropertyChanged(); } }

        // In FieldVM:
        [DataMember(Order = 8)]
        public string FontFamilyName
        {
            get { return _fontFamilyName; }
            set { _fontFamilyName = value; OnPropertyChanged(); }
        }
        private string _fontFamilyName = "Segoe UI"; // Default


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
        { var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(n)); }
    }

}