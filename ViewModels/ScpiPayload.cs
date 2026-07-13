using AutoSkippy.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoSkippy.ViewModels;

public partial class ScpiPayload : ViewModelBase
{
    public static readonly string DefaultPreFetchValueCommands = "CONF:TCH,CONF:TDW,CONF:TME,CONF:TDIS,4";
    public static readonly string DefaultPreFetchAppliedCommands = "FETC";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    [NotifyPropertyChangedFor(nameof(PreFetchDelay))]
    private string _setupLines = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private string _loopLines = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private int _loopCount = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepsCount))]
    private string _teardownLines = string.Empty;

    public int StepsCount => 
        SetupLines.Split(Environment.NewLine).Length + 
        LoopCount * LoopLines.Split(Environment.NewLine).Length +
        TeardownLines.Split(Environment.NewLine).Length;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreFetchDelay))]
    private string _preFetchValueCommands = DefaultPreFetchValueCommands;

    [ObservableProperty]
    private string _preFetchAppliedCommands = DefaultPreFetchAppliedCommands;

    public int PreFetchDelay => CalculateDelay(PreFetchValueCommands, SetupLines);

    public static bool IsInSet(string command, string[] commands) => commands.Any(c => command.StartsWith(c));

    public static int CalculateDelay(string valueCommands, string source)
    {
        var rawCommands = valueCommands.Split(',');
        var fixedValue = rawCommands.Sum(c => int.TryParse(c, out int cv) ? cv : 0);
        var delayValue = source
            .Split(Environment.NewLine)
            .Where(l => IsInSet(l, rawCommands))
            .DefaultIfEmpty()
            .Sum(c => int.TryParse(c?.Split(' ').Skip(1).FirstOrDefault(), out int v) ? v : 0);

        return fixedValue + delayValue;
    }
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

    [DataMember]
    public string? PreFetchValueCommands { get; set; }

    [DataMember]
    public string? PreFetchAppliedCommands { get; set; }

    [JsonConstructor]
    public ScpiPayloadSerialisable() { }

    public ScpiPayloadSerialisable(string? setupLines, string? loopLines, string? teardownLines, int loopCout, string? preFetchValueCommands, string? preFetchAppliedCommands)
    {
        SetupLines = setupLines;
        LoopLines = loopLines;
        TeardownLines = teardownLines;
        LoopCout = loopCout;
        PreFetchValueCommands = preFetchValueCommands;
        PreFetchAppliedCommands = preFetchAppliedCommands;
    }

    public ScpiPayload ToActual() => new()
    {
        SetupLines = SetupLines ?? string.Empty,
        LoopLines = LoopLines ?? string.Empty,
        TeardownLines = TeardownLines ?? string.Empty,
        LoopCount = LoopCout,

        PreFetchValueCommands = PreFetchValueCommands ?? ScpiPayload.DefaultPreFetchValueCommands,
        PreFetchAppliedCommands = PreFetchAppliedCommands ?? ScpiPayload.DefaultPreFetchAppliedCommands,
    };

    public async Task Save(string filename)
    {
        var json = JsonSerializer.Serialize(this, SourceGenerationContext.Default.ScpiPayloadSerialisable);

        try
        {
            using var sw = new StreamWriter(filename, false) { AutoFlush = true };

            await sw.WriteAsync(json);
            sw.Close();
            sw.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
    }

    public static async Task<ScpiPayloadSerialisable?> Load(string filename)
    {
        try
        {
            using StreamReader sr = new(filename);
            var json = sr.ReadToEnd();

            return JsonSerializer.Deserialize<ScpiPayloadSerialisable>(json, JsonConfig.DeserialiserOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }

        return null;
    }
}
