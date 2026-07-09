using AutoSkippy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace AutoSkippy.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static string VersionNumber => Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "Unknown";

    [ObservableProperty]
    private ScpiPayload _currentPayload = new();

    [ObservableProperty]
    private string _currentPayloadPath = string.Empty;

    [ObservableProperty]
    private string _resultsLines = string.Empty;

    [ObservableProperty]
    private string _recentFolder = string.Empty;

    public MainWindowViewModel()
    {
    }

    public static async void SavePayloadToJson(ScpiPayload payload, string fullFileName)
    {
        var json = JsonSerializer.Serialize(payload.ToSerialisable(), SourceGenerationContext.Default.ScpiPayloadSerialisable);

        try
        {
            using var sw = new StreamWriter(fullFileName, false) { AutoFlush = true };

            await sw.WriteAsync(json);
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
            SavePayloadToJson(p, CurrentPayloadPath);
        }
    }

    [RelayCommand]
    public void ClearResults() => ResultsLines = string.Empty;

    [RelayCommand]
    public void RefreshComPorts()
    {
        AvailableComPorts.Clear();
        foreach(var p in ComPortComm.GetPorts())
        {
            AvailableComPorts.Add(p);
    }
        }
}
