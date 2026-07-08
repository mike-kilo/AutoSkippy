using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpYaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AutoSkippy.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static string VersionNumber => Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "Unknown";

    [ObservableProperty]
    private ScpiPayload _currentPayload = new();

    [ObservableProperty]
    private string _currentPayloadPath = string.Empty;

    public MainWindowViewModel()
    {
    }

    public static async void SavePayloadToYaml(ScpiPayload payload, string fullFileName)
    {
        var yaml = YamlSerializer.Serialize(payload);
        try
        {
            using var sw = new StreamWriter(fullFileName, false) { AutoFlush = true };
            await sw.WriteAsync(yaml);
            sw.Close();
            sw.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
    }

    [RelayCommand]
    public void SavePayload()
    {
        if (CurrentPayload is ScpiPayload p && !string.IsNullOrEmpty(CurrentPayloadPath))
        {
            SavePayloadToYaml(p, CurrentPayloadPath);
        }
    }
}
