using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Transactions;
using System.Data;

namespace EdicaoFotos;

public partial class EditorWindow : Window
{
    private BitmapSource? _OriginalPhoto;
    private double _contrast = 0.0; // Default contrast value
    private double _brightness = 0.0; // Default brightness value
    private double _gamma = 1.0; // Default gamma value
    private double _pointX = 0.5; //Defalt
    private bool _draggingPoint = false;
    private const double GammaGraphSize = 110;
    public EditorWindow(string imagePath)
    {
        InitializeComponent();
        SetImage(imagePath);
    }

    public void SetImage(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        ImageControl.Source = bitmap;
        _OriginalPhoto = bitmap;

        //ImageSize
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        ImageSizeText.Text = $"Image size: {width} x {height}";
    }
    
    //Panels dawn bar
    private void ShowPanel(UIElement panel)
    {
        MainToolsPanel.Visibility = Visibility.Collapsed;
        ContrastPanel.Visibility = Visibility.Collapsed;
        BrightnessPanel.Visibility = Visibility.Collapsed;
        GammaPanel.Visibility = Visibility.Collapsed;
        ChromaKeyPanel.Visibility = Visibility.Collapsed;

        panel.Visibility = Visibility.Visible;
    }





    //Master function
    private byte Adjust(byte color, double contrast, double brightness, double gamma)
    {
        double factor = (100.0 + contrast) / 100.0;
        double v = Math.Clamp((color - 128) * factor + 128, 0, 255);   // 1) contraste

        double t = Math.Abs(brightness) / 100.0;                        // 2) brilho
        v = brightness >= 0 ? v + (255 - v) * t : v * (1 - t);

        return (byte)Math.Round(v);
    }





    //contrast
    private void ContrastButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ContrastPanel);
    }

    private void ContrastBackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(MainToolsPanel);
    }

    //o aplique
    private void ApplyContrast()
    {
        if (_OriginalPhoto == null) return;

        ImageControl.Source = AdjustContrast(_OriginalPhoto, _contrast, _brightness, _gamma);

    }

    private BitmapSource AdjustContrast(BitmapSource source, double contrast, double brightness, double gamma)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int width = converted.PixelWidth;
        int height = converted.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[stride * height];
        converted.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i]     = Adjust(pixels[i], contrast, brightness, gamma);
            pixels[i + 1] = Adjust(pixels[i + 1], contrast, brightness, gamma);
            pixels[i + 2] = Adjust(pixels[i + 2], contrast, brightness, gamma);
        }

        var result = BitmapSource.Create(width, height, source.DpiX, source.DpiY,
                                         PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    //slider and textbox
    private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ContrastTextBox == null) return;

        _contrast = Math.Round(e.NewValue, 2); // Round to 2 decimal places
        ContrastTextBox.Text = e.NewValue.ToString();  
        ApplyContrast();
    }

    private void ContrastTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitContrastText();
        }
    }

    private void CommitContrastText()
    {
        if(!double.TryParse(ContrastTextBox.Text, out double typed))
        {
            ContrastTextBox.Text = _contrast.ToString(); // Reset to previous valid value
            return;
        }
        typed = Math.Clamp(typed, -100, 100); // Clamp value between -100 and 100
        ContrastSlider.Value = typed; // Update the slider value
        ContrastTextBox.Text = typed.ToString(); // Update the textbox to reflect the clamped value
    }





    //Brightness
    private void BrightnessButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(BrightnessPanel);
    }

    private void BrightnessBackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(MainToolsPanel);
    }

    private void ApplyBrightness()
    {
        if (_OriginalPhoto == null) return;

        ImageControl.Source = AdjustBrightness(_OriginalPhoto, _contrast, _brightness, _gamma);
    }

    private BitmapSource AdjustBrightness(BitmapSource source, double contrast, double brightness, double gamma)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int width = converted.PixelWidth;
        int height = converted.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[stride * height];
        converted.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i]     = Adjust(pixels[i], contrast, brightness, gamma);
            pixels[i + 1] = Adjust(pixels[i + 1], contrast, brightness, gamma);
            pixels[i + 2] = Adjust(pixels[i + 2], contrast, brightness, gamma);
        }

        var result = BitmapSource.Create(width, height, source.DpiX, source.DpiY,
                                         PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (BrightnessTextBox == null) return;

        _brightness = Math.Round(e.NewValue, 2); // Round to 2 decimal places
        BrightnessTextBox.Text = e.NewValue.ToString();  
        ApplyBrightness();
    }

    private void BrightnessTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitBrightnessText();
        }
    }

    private void CommitBrightnessText()
    {
        if(!double.TryParse(BrightnessTextBox.Text, out double typed))
        {
            BrightnessTextBox.Text = _brightness.ToString(); // Reset to previous valid value
            return;
        }
        typed = Math.Clamp(typed, -100, 100); // Clamp value between -100 and 100
        BrightnessSlider.Value = typed; // Update the slider value
        BrightnessTextBox.Text = typed.ToString(); // Update the textbox to reflect the clamped value
    }




    //Gama
    private void GammaButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(GammaPanel);
    }

    private void GammaBackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(MainToolsPanel);
    }

    private void ApplyGamma()
    {
        if (_OriginalPhoto == null) return;

        ImageControl.Source = AdjustGamma(_OriginalPhoto, _contrast, _brightness, _gamma);
    }

    private BitmapSource AdjustGamma(BitmapSource source, double contrast, double brightness, double gamma)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int width = converted.PixelWidth;
        int height = converted.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[stride * height];
        converted.CopyPixels(pixels, stride, 0);

        byte[] lut = new byte[256];
        for(int v = 0; v < 256; v++)
        {
            lut[v] = (byte)Math.Round(255 * Math.Pow(v/255, gamma));
        }

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i]     = Adjust(pixels[i], contrast, brightness, gamma);
            pixels[i + 1] = Adjust(pixels[i + 1], contrast, brightness, gamma);
            pixels[i + 2] = Adjust(pixels[i + 2], contrast, brightness, gamma);

            // Apply gamma correction
            pixels[i]     = (byte)(255 * Math.Pow(pixels[i] / 255.0, gamma));
            pixels[i + 1] = (byte)(255 * Math.Pow(pixels[i + 1] / 255.0, gamma));
            pixels[i + 2] = (byte)(255 * Math.Pow(pixels[i + 2] / 255.0, gamma));
        }

        var result = BitmapSource.Create(width, height, source.DpiX, source.DpiY,
                                         PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    private void GammaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if(GammaTextBox == null || GammaCurve == null) return;

        _gamma = Math.Round(e.NewValue, 2);
        GammaTextBox.Text = e.NewValue.ToString();
        UpdateGammaGraph();
        ApplyGamma();
    }

    private void GammaTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if(e.Key == Key.Enter)
        {
            CommitGammaText();
        }
    }

    private void CommitGammaText()
    {
        if(!double.TryParse(GammaTextBox.Text, out double typed))
        {
            GammaTextBox.Text = _gamma.ToString();
            return;
        }

        typed = Math.Clamp(typed, 0.2, 3); // Clamp value between 
        GammaSlider.Value = typed; // Update the slider value
        GammaTextBox.Text = typed.ToString(); // Update the textbox to reflect the clamped value
    }

    private void GammaPoint_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _draggingPoint = true;
        GammaCanvas.CaptureMouse();
        MoveGammaPoint(e.GetPosition(GammaCanvas));
    }

    private void GammaCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if(_draggingPoint) MoveGammaPoint(e.GetPosition(GammaCanvas));

        if(e.LeftButton != MouseButtonState.Pressed) return;

        MoveGammaPoint(e.GetPosition(GammaCanvas));
    }

    private void GammaCanvas_MouseUp(object sender, MouseButtonEventArgs E)
    {
        _draggingPoint = false;
        GammaCanvas.ReleaseMouseCapture();
    }

    private void MoveGammaPoint(Point pos)
    {
        // Pixels do Canvas viram valores de 0 a 1. O Y é invertido porque no WPF ele cresce para baixo.
        double x = Math.Clamp(pos.X / GammaGraphSize, 0.1, 0.9);
        double y = Math.Clamp(1 - pos.Y / GammaGraphSize, 0.05, 0.95);

        _pointX = x;

        // A curva passa por (x, y) quando o expoente é ln(y) / ln(x)
        double gamma = Math.Log(y) / Math.Log(x);
        gamma = Math.Clamp(gamma, GammaSlider.Minimum, GammaSlider.Maximum);

        GammaSlider.Value = gamma;   // dispara o ValueChanged, que atualiza o resto
        UpdateGammaGraph();          // garante o redesenho se o valor do slider não mudou
    }

    private void UpdateGammaGraph()
    {
        var points = new PointCollection();
        for(int i = 0; i <= 40; i++)
        {
            double x = i/40;
            double y = Math.Pow(x, _gamma);
            points.Add(new Point(x * GammaGraphSize, (1 - y) * GammaGraphSize));
        }
        GammaCurve.Points = points;

        // Ponto: sempre em cima da curva, na posição horizontal que o usuário escolheu
        double pointY = Math.Pow(_pointX, _gamma);
        Canvas.SetLeft(GammaPoint, _pointX * GammaGraphSize - GammaPoint.Width / 2);
        Canvas.SetTop(GammaPoint, (1 - pointY) * GammaGraphSize - GammaPoint.Height / 2);
    }



    
    //ChromaKey
    private void ChromaKeyButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ChromaKeyPanel);
    }

    private void ChromaKeyBackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(MainToolsPanel);
    }

}