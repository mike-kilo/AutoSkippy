using AutoSkippy.Models;
using AutoSkippy.ViewModels;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AutoSkippy.Views;

public partial class MainWindow : Window
{
    public static FilePickerFileType SCPIPayload { get; } = new("SCPI Payload")
    {
        Patterns = [ "*.json" ],
        MimeTypes = ["text/json"],
    };

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ButtonBrowseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel) return;
        if (this.DataContext is not MainWindowViewModel vm) return;

        var suggestedFolder = !string.IsNullOrEmpty(vm.RecentFolder) && await StorageProvider.TryGetFolderFromPathAsync(vm.RecentFolder) is IStorageFolder sf ?
                sf : await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SCPI payload",
            AllowMultiple = false,
            SuggestedStartLocation = suggestedFolder,
            FileTypeFilter = [ SCPIPayload, FilePickerFileTypes.All ],
            SuggestedFileType = SCPIPayload,
        });

        if (files.Count != 1) return;
        vm.RecentFolder = Path.GetDirectoryName(files[0].Path.LocalPath) ?? string.Empty;

        vm.CurrentPayload = new();
        vm.CurrentPayloadPath = files[0].Path.LocalPath;
        try
        {
            using StreamReader sr = new(vm.CurrentPayloadPath);
            var json = sr.ReadToEnd();

            var pld = JsonSerializer.Deserialize<ScpiPayloadSerialisable>(json, JsonConfig.DeserialiserOptions);

            if (pld is not null)
            {
                vm.CurrentPayload = pld.ToActual();
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

        var suggestedFolder = !string.IsNullOrEmpty(vm.RecentFolder) && await StorageProvider.TryGetFolderFromPathAsync(vm.RecentFolder) is IStorageFolder sf ?
        sf : await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Open SCPI payload",
            DefaultExtension = "yaml",
            ShowOverwritePrompt = true,
            SuggestedFileName = Path.GetFileName(vm.CurrentPayloadPath),
            FileTypeChoices = [SCPIPayload, FilePickerFileTypes.All],
            SuggestedStartLocation = suggestedFolder,
            SuggestedFileType = SCPIPayload,
        });

        if (file is null) return;

        vm.RecentFolder = Path.GetDirectoryName(file.Path.LocalPath) ?? string.Empty;

        if (file.TryGetLocalPath() is string path)
        { 
            MainWindowViewModel.SavePayloadToJson(vm.CurrentPayload, path); 
        }
    }

    private void WindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.DataContext is not MainWindowViewModel vm) return;
        vm.RefreshComPorts();
    }

    private async void ButtonCopyClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not TopLevel topLevel) return;
        if (this.DataContext is not MainWindowViewModel vm) return;
        if (topLevel.Clipboard is not IClipboard clipboard) return;

        await clipboard.SetTextAsync(vm.ResultsLines);
    }
}