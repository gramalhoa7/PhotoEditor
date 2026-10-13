using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace EdicaoFotos;

public partial class EditorWindow : Window
{
    private BitmapSource? _OriginalPhoto;
    private double _contrast = 0.0; // Default contrast value
    private double _brightness = 0.0; // Default brightness value
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
        MirrorPanel.Visibility = Visibility.Collapsed;
        ContrastPanel.Visibility = Visibility.Collapsed;
        BrightnessPanel.Visibility = Visibility.Collapsed;

        panel.Visibility = Visibility.Visible;
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

        ImageControl.Source = AdjustContrast(_OriginalPhoto, _contrast, _brightness);

    }

    private BitmapSource AdjustContrast(BitmapSource source, double contrast, double brightness)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int width = converted.PixelWidth;
        int height = converted.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[stride * height];
        converted.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i]     = Adjust(pixels[i], contrast, brightness);
            pixels[i + 1] = Adjust(pixels[i + 1], contrast, brightness);
            pixels[i + 2] = Adjust(pixels[i + 2], contrast, brightness);
        }

        var result = BitmapSource.Create(width, height, source.DpiX, source.DpiY,
                                         PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    private byte Adjust(byte color, double contrast, double brightness)
    {
        double factor = (100.0 + contrast) / 100.0;
        double v = Math.Clamp((color - 128) * factor + 128, 0, 255);   // 1) contraste

        double t = Math.Abs(brightness) / 100.0;                        // 2) brilho
        v = brightness >= 0 ? v + (255 - v) * t : v * (1 - t);

        return (byte)Math.Round(v);
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

        ImageControl.Source = AdjustBrightness(_OriginalPhoto, _contrast, _brightness);
    }

    private BitmapSource AdjustBrightness(BitmapSource source, double contrast, double brightness)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        int width = converted.PixelWidth;
        int height = converted.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[stride * height];
        converted.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i]     = Adjust(pixels[i], contrast, brightness);
            pixels[i + 1] = Adjust(pixels[i + 1], contrast, brightness);
            pixels[i + 2] = Adjust(pixels[i + 2], contrast, brightness);
        }

        var result = BitmapSource.Create(width, height, source.DpiX, source.DpiY,
                                         PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    private byte AdjustBrightnessF(byte color, double brightness)
    {
        double f = brightness / 100.0; // Convert percentage to a factor

        double r = brightness >= 0 ? color + (255 - color)*f : color * (1 - f); // Adjust brightness based on the factor
        return (byte)Math.Round(r);
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

}