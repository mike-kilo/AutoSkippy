using System.Reflection;

namespace AutoSkippy.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public static string VersionNumber { get; } = Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "Unknown";

    }
}
