using AutoSkippy.Models;
using AutoSkippy.ViewModels;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using System.IO;

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

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SCPI payload",
            AllowMultiple = false,
            SuggestedStartLocation = await vm.GetPayloadFolder(),
            FileTypeFilter = [ SCPIPayload, FilePickerFileTypes.All ],
            SuggestedFileType = SCPIPayload,
        });

        if (files.Count != 1) return;
        vm.RecentFolder = Path.GetDirectoryName(files[0].Path.LocalPath) ?? string.Empty;

        vm.CurrentPayloadPath = files[0].Path.LocalPath;
        vm.CurrentPayload = await ScpiPayloadSerialisable.Load(vm.CurrentPayloadPath) is ScpiPayloadSerialisable pld ? pld.ToActual() : new();
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
            SuggestedFileName = Path.GetFileName(vm.CurrentPayloadPath),
            FileTypeChoices = [SCPIPayload, FilePickerFileTypes.All],
            SuggestedStartLocation = await vm.GetPayloadFolder(),
            SuggestedFileType = SCPIPayload,
        });

        if (file is null) return;

        vm.RecentFolder = Path.GetDirectoryName(file.Path.LocalPath) ?? string.Empty;

        if (file.TryGetLocalPath() is string path)
        { 
            await vm.CurrentPayload.ToSerialisable().Save(path);
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