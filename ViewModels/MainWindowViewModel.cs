using AutoSkippy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

    public ObservableCollection<string> AvailableComPorts { get; private set; } = [];

    public PayloadProcessor Processor { get; private set; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectComCommand))]
    private string? _selectedComPort = string.Empty;

    [ObservableProperty]
    private int _progressSteps = 0;

    public MainWindowViewModel()
    {
        Processor.PropertyChanged += ProcessorPropertyChanged;
    }

    private void ProcessorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (nameof(PayloadProcessor.ProgressStep).Equals(e.PropertyName)) 
            ProgressSteps = Processor.ProgressStep;
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

    public bool IsComPortSelected() => !string.IsNullOrEmpty(SelectedComPort);

    [RelayCommand(CanExecute = nameof(IsComPortSelected))]
    public void ConnectCom()
    {
        if (string.IsNullOrEmpty(SelectedComPort)) return;
        ComPortComm.OpenConnection(SelectedComPort);
    }

    [RelayCommand]
    public void DisconnectCom()
    {
        ComPortComm.CloseConnection();
    }

    [RelayCommand]
    public async Task RunPayload()
    {
        ProgressSteps = 0;
        if (CurrentPayload.StepsCount == 0) return;

        await Processor.Process(CurrentPayload);
    }

    [RelayCommand]
    public async Task AbortPayload() => Processor.IsBreakRequested = true;
}
