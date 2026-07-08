using CommunityToolkit.Mvvm.ComponentModel;
using SharpYaml.Serialization;

namespace AutoSkippy.ViewModels;

public partial class ScpiPayload : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private string _setupLines = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private string _loopLines = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private int _loopCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private string _teardownLines = string.Empty;

    [YamlIgnore]
    public int StepsCount => 
        SetupLines.Split("{Environment.NewLine}").Length + 
        LoopCount * LoopLines.Split("{Environment.NewLine}").Length +
        TeardownLines.Split("{Environment.NewLine}").Length;
}
