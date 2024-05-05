using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Passphrase;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const string Author = "Restrio";
    const string FileName = "Passphrase.txt";
    private string _appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Author);
    private string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Author, FileName);
    private Config _config;

    public MainWindow()
    {
        InitializeComponent();

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(.5),
        };
        timer.Tick += (sender, e) =>
        {
            Date.Text = DateTime.Now.ToUniversalTime().ToString("dd.MM.yyyy - HH:mm:ss") + " UTC";
        };
        timer.Start();

        _config = LoadConfig();
        Passphrase.Text = _config.Passphrase;
        OpacitySlider.Value = _config.Opacity;

        Left = _config.Location.X;
        Top = _config.Location.Y;
        Height = _config.Size.Height;
        Width = _config.Size.Width;
        Background.Opacity = _config.Opacity;
        BorderBrush.Opacity = _config.Opacity;
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveConfig();
        base.OnClosed(e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        var mainWindow = Application.Current.MainWindow;
        _config.Location = new Location(mainWindow.Left, mainWindow.Top);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _config.Size = new Size(Height, Width);
    }

    public void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void OnPassphraseKeyUp(object sender, KeyEventArgs args)
    {
        if (sender is TextBox textBox)
        {
            _config.Passphrase = textBox.Text;
        }
    }

    private Config LoadConfig()
    {
        if (!Path.Exists(_appData))
        {
            Directory.CreateDirectory(_appData);
        }
        using var fileStream = new FileStream(_configPath, FileMode.OpenOrCreate);
        using var reader = new StreamReader(fileStream);
        var configString = reader.ReadToEnd() ?? "";

        try
        {
            return JsonSerializer.Deserialize<Config>(configString);
        }
        catch (Exception e)
        {
            using EventLog eventLog = new EventLog("Application");
            eventLog.Source = "Application";
            eventLog.WriteEntry("Wasn't able to load config. Resetting configuration.", EventLogEntryType.Error);
            return new Config("", 1, new Location(0, 0), new Size(180, 180));
        }
    }

    private void SaveConfig()
    {
        using var fileStream = new FileStream(_configPath, FileMode.Create);
        using var writer = new StreamWriter(fileStream);
        writer.Write(JsonSerializer.Serialize(_config));
    }

    public void OpacitySliderMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is Slider slider)
        {
            _config.Opacity = slider.Value;
            Background.Opacity = slider.Value;
            BorderBrush.Opacity = _config.Opacity;
        }
    }
}