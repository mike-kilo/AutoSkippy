namespace AutoSkippy.ViewModels;

public record DecimalSymbolConverter
{
    public delegate string SymbolConverter(string input);

    public char SourceCharacter { get; set; }

    public required string Description { get; init; }

    public required SymbolConverter ConversionMethod { get; init; }
}

public static class ConversionExtensions
{
    extension(string text)
    {
        public string DotsToCommas() => text.Replace('.', ',');

        public string CommasToDots() => text.Replace(',', '.');
    }
}