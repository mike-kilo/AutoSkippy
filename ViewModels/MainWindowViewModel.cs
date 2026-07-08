using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace AutoSkippy.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public static string VersionNumber => Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "Unknown";

    [ObservableProperty]
    private ScpiPayload? _currentPayload;

    public MainWindowViewModel()
    {
        CurrentPayload = new() { LoopCount = 10, SetupLines = "SETUP", LoopLines = "LOOP", TeardownLines = "TEARDOWN" };
    }
}
