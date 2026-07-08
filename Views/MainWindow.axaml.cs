using AutoSkippy.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.IO;

namespace AutoSkippy.Views;

public partial class MainWindow : Window
{
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

        vm.CurrentPayloadPath = files[0].Path.LocalPath;
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
            SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
            SuggestedFileType = SCPIPayload,
        });

        if (file is null) return;
    }
}