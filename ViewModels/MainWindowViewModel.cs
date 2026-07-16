using AutoSkippy.Models;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoSkippy.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static string VersionNumber => Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "Unknown";

    [ObservableProperty]
    public partial ScpiPayload CurrentPayload { get; set; } = new();

    [ObservableProperty]
    public partial string CurrentPayloadPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResultsLines { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RecentFolder { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ComPortComm Communicator { get; set; } = new();

    public ObservableCollection<string> AvailableComPorts { get; private set; } = [];

    public PayloadProcessor Processor { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectComCommand))]
    public partial string? SelectedComPort { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int ProgressSteps { get; set; } = 0;

    [ObservableProperty]
    public partial string RecentCommand { get; set; } = string.Empty;

    public AppSettings Settings { get; set; } = new();

    public TopLevel? MainVindowTopLevel { get; set;  }

    public MainWindowViewModel()
    {
        Processor = new(Communicator);
        Processor.Progressed += ProcessorProgressed;
        Processor.LineReceived += ProcessorLineReceived;
        Processor.RecentCommand += ProcessorRecentCommand;
    }

    private void ProcessorRecentCommand(object? sender, string e) => RecentCommand = e;

    private void ProcessorProgressed(object? sender, EventArgs e) => ProgressSteps++;

    private void ProcessorLineReceived(object? sender, PayloadProcessor.LineReceivedEventArgs e) => ResultsLines += e.Text.Trim() + Environment.NewLine;

    public static async Task SavePayloadToJson(ScpiPayload payload, string fullFileName) => await payload.ToSerialisable().Save(fullFileName);

    [RelayCommand]
    public async Task SavePayload()
    {
        if (CurrentPayload is ScpiPayload p && !string.IsNullOrEmpty(CurrentPayloadPath))
        {
            await SavePayloadToJson(p, CurrentPayloadPath);
            
            Settings.RecentUsedFolder = RecentFolder;
            await Settings.Save();
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
    public async Task ConnectCom()
    {
        if (string.IsNullOrEmpty(SelectedComPort)) return;
        if (Communicator.OpenConnection(SelectedComPort))
        {
            Settings.RecentUsedPort = SelectedComPort;
            await Settings.Save();
        }
    }

    [RelayCommand]
    public void DisconnectCom() => Communicator.CloseConnection();

    [RelayCommand]
    public async Task RunPayload()
    {
        ProgressSteps = 0;
        if (CurrentPayload.StepsCount == 0) return;

        await Processor.Process(CurrentPayload);
    }

    [RelayCommand]
    public async Task AbortPayload() => Processor.IsBreakRequested = true;

    public async Task<IStorageFolder?> GetPayloadFolder() =>
        MainVindowTopLevel?.StorageProvider is not IStorageProvider sp ? null :
        !string.IsNullOrEmpty(RecentFolder) && await sp.TryGetFolderFromPathAsync(RecentFolder) is IStorageFolder sf ? sf :
        !string.IsNullOrEmpty(Settings.RecentUsedFolder) && await sp.TryGetFolderFromPathAsync(Settings.RecentUsedFolder) is IStorageFolder sf0 ? sf0 :
        await sp.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

    [RelayCommand]
    public async Task RunOncePayloadSection(object? parameter)
    {
        if (parameter is string linesRaw && linesRaw.Split(Environment.NewLine) is string[] lines && lines.Length > 0)
        {
            await Processor.Process(lines);
        }
    }

    [RelayCommand]
    public async Task CopyText(object? parameter)
    {
        if (parameter is not string text) return;
        if (MainVindowTopLevel?.Clipboard is not IClipboard clipboard) return;

        await clipboard.SetTextAsync(text);
    }

    [RelayCommand]
    public void DotsToCommas() => ResultsLines = ResultsLines.Replace('.', ',');

    [RelayCommand]
    public void CommasToDots() => ResultsLines = ResultsLines.Replace(',', '.');

}
