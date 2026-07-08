using AutoSkippy.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SharpYaml;
using SharpYaml.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AutoSkippy.Views;

public partial class MainWindow : Window
{
    private YamlSerializerOptions _yamlSerializerOptions = new YamlSerializerOptions
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

    public static FilePickerFileType SCPIPayload { get; } = new("SCPI Payload")
    {
        Patterns = [ "*.yaml" ],
        MimeTypes = ["text/x-yaml"],
    };

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ButtonBrowseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel) return;
        if (this.DataContext is not MainWindowViewModel vm) return;

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SCPI payload",
            AllowMultiple = false,
            SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
            FileTypeFilter = [ SCPIPayload, FilePickerFileTypes.All ],
            SuggestedFileType = SCPIPayload,
        });

        if (files.Count != 1) return;

        vm.CurrentPayload = new();
        vm.CurrentPayloadPath = files[0].Path.LocalPath;
        try
        {
            using StreamReader sr = new(vm.CurrentPayloadPath);
            var yaml = YamlStream.Load(sr);
            var pld = YamlSerializer.Deserialize<ScpiPayload>(yaml.ToString(), _yamlSerializerOptions);
            if (pld is not null)
            {
                vm.CurrentPayload = pld;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private async void ButtonSaveAsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel) return;
        if (this.DataContext is not MainWindowViewModel vm) return;

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Open SCPI payload",
            DefaultExtension = "yaml",
            ShowOverwritePrompt = true,
            SuggestedFileName = System.IO.Path.GetFileName(vm.CurrentPayloadPath),
            SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
            SuggestedFileType = SCPIPayload,
        });

        if (file is null) return;

        if (file.TryGetLocalPath() is string path)
        { 
            MainWindowViewModel.SavePayloadToYaml(vm.CurrentPayload, path); 
        }
    }
}