using AutoSkippy.Models;
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
    private ScpiPayload _currentPayload = new();

    [ObservableProperty]
    private string _currentPayloadPath = string.Empty;

    [ObservableProperty]
    private string _resultsLines = string.Empty;

    [ObservableProperty]
    private string _recentFolder = string.Empty;

    [ObservableProperty]
    private ComPortComm _communicator = new();

    public ObservableCollection<string> AvailableComPorts { get; private set; } = [];

    public PayloadProcessor Processor { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectComCommand))]
    private string? _selectedComPort = string.Empty;

    [ObservableProperty]
    private int _progressSteps = 0;

    [ObservableProperty]
    private string _recentCommand = string.Empty;

    public AppSettings Settings { get; set; } = new();

    public IStorageProvider? StorageProvider { get; set; }

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
        StorageProvider is null ? null :
        !string.IsNullOrEmpty(RecentFolder) && await StorageProvider.TryGetFolderFromPathAsync(RecentFolder) is IStorageFolder sf ? sf :
        !string.IsNullOrEmpty(Settings.RecentUsedFolder) && await StorageProvider.TryGetFolderFromPathAsync(Settings.RecentUsedFolder) is IStorageFolder sf0 ? sf0 :
        await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
}
