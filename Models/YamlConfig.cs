using SharpYaml;
using System.Text.Json;

namespace AutoSkippy.Models
{
    public static class YamlConfig
    {
        public static YamlSerializerOptions SerializerOptions = new YamlSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IndentSize = 4,
            DefaultIgnoreCondition = YamlIgnoreCondition.WhenWritingNull,
            DuplicateKeyHandling = YamlDuplicateKeyHandling.FirstWins,
            MappingOrder = YamlMappingOrderPolicy.Declaration,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip,
            PropertyNameCaseInsensitive = true,
        };
    }
}
