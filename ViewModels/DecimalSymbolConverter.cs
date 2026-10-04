using System;

namespace AutoSkippy.ViewModels;

public record DecimalSymbolConverter
{
    public char SourceCharacter { get; set; }

    public required string Description { get; init; }

    public required Delegate ConversionMethod { get; init; }
}
