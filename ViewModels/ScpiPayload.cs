using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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

    public int StepsCount => 
        SetupLines.Split(Environment.NewLine).Length + 
        LoopCount * LoopLines.Split(Environment.NewLine).Length +
        TeardownLines.Split(Environment.NewLine).Length;
}

[DataContract]
public class ScpiPayloadSerialisable
{
    [DataMember]
    public string? SetupLines { get; set; }

    [DataMember]
    public string? LoopLines { get; set; }

    [DataMember]
    public string? TeardownLines { get; set; }

    [DataMember]
    public int LoopCout { get; set; }

    [JsonConstructor]
    public ScpiPayloadSerialisable() { }

    public ScpiPayloadSerialisable(string? setupLines, string? loopLines, string? teardownLines, int loopCout)
    {
        SetupLines = setupLines;
        LoopLines = loopLines;
        TeardownLines = teardownLines;
        LoopCout = loopCout;
    }

    public ScpiPayload ToActual() => new()
    {
        SetupLines = SetupLines ?? string.Empty,
        LoopLines = LoopLines ?? string.Empty,
        TeardownLines = TeardownLines ?? string.Empty,
        LoopCount = LoopCout,
    };
}
