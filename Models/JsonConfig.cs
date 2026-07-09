using AutoSkippy.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSkippy.Models;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(ScpiPayloadSerialisable), GenerationMode = JsonSourceGenerationMode.Default)]
public partial class SourceGenerationContext : JsonSerializerContext { }

public static class JsonConfig
{
    public static JsonSerializerOptions DeserialiserOptions => new()
    {
        TypeInfoResolver = SourceGenerationContext.Default,
        IgnoreReadOnlyProperties = true,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    };

    public static ScpiPayloadSerialisable ToSerialisable(this ScpiPayload payload) => new()
    {
        SetupLines = payload.SetupLines,
        LoopLines = payload.LoopLines,
        TeardownLines = payload.TeardownLines,
        LoopCout = payload.LoopCount,
    };
}
