using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoSkippy.Models;

public class AppSettings
{
    private static readonly string SETTINGSLOCATION = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
        ".AutoSkippy", 
        "AutoSkippySettings.json");

    public string RecentUsedPort { get; set; } = string.Empty;

    public string RecentUsedFolder { get; set; } = string.Empty;

    public AppSettings() { }

    public async Task Save()
    {
        var json = JsonSerializer.Serialize(this, SourceGenerationContext.Default.AppSettings);

        try
        {
            if (Path.GetDirectoryName(SETTINGSLOCATION) is string p && !Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            using var sw = new StreamWriter(SETTINGSLOCATION, false) { AutoFlush = true };

            await sw.WriteAsync(json);
            sw.Close();
            sw.Dispose();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
    }

    public static async Task<AppSettings?> Load()
    {
        try
        {
            using StreamReader sr = new(SETTINGSLOCATION);
            var json = sr.ReadToEnd();

            return JsonSerializer.Deserialize<AppSettings>(json, JsonConfig.DeserialiserOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }

        return null;
    }
}
