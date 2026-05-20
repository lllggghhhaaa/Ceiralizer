using System;
using Ceiralizer.Generator.Utils;

namespace Ceiralizer.Generator.Models;

public class CascadedOptions(PacketOptions options)
{
    public PacketOptions Options { get; } = options;

    public static CascadedOptions Create(PacketOptions globalOptions, PacketOptions fieldOptions)
    {
        var merged = new PacketOptions();

        if (globalOptions.Text.Encoder is not null)
            merged.Text.Encoder = globalOptions.Text.Encoder;

        if (globalOptions.Text.PrefixLength is not null)
            merged.Text.PrefixLength = globalOptions.Text.PrefixLength;

        if (globalOptions.Collection.Size is not null)
            merged.Collection.Size = globalOptions.Collection.Size;

        if (fieldOptions.Text.Encoder is not null)
            merged.Text.Encoder = fieldOptions.Text.Encoder;

        if (fieldOptions.Text.PrefixLength is not null)
            merged.Text.PrefixLength = fieldOptions.Text.PrefixLength;

        if (fieldOptions.Collection.Size is not null)
            merged.Collection.Size = fieldOptions.Collection.Size;

        return new CascadedOptions(merged);
    }

    public string GetEncodingCode()
    {
        var encoder = Options.Text.Encoder;
        if (encoder is null)
            return "Encoding.UTF8";

        return encoder.Value.GetValue<TextEncoding>() switch
        {
            TextEncoding.ASCII => "Encoding.ASCII",
            TextEncoding.UTF7 => "Encoding.UTF7",
            TextEncoding.UTF8 => "Encoding.UTF8",
            TextEncoding.Unicode => "Encoding.Unicode",
            TextEncoding.UTF32 => "Encoding.UTF32",
            TextEncoding.BigEndianUnicode => "Encoding.BigEndianUnicode",
            _ => "Encoding.UTF8"
        };
    }

    public string GetWriterPrefixCode()
    {
        var prefix = Options.Text.PrefixLength;
        if (prefix is null)
            return "";

        return prefix.Value.GetValue<PrefixType>() switch
        {
            PrefixType.Byte => "(byte)",
            PrefixType.SByte => "(sbyte)",
            PrefixType.Short => "(short)",
            PrefixType.UShort => "(ushort)",
            _ => ""
        };
    }

    public string GetReaderPrefixCode()
    {
        var prefix = Options.Text.PrefixLength;
        if (prefix is null)
            return "reader.ReadInt()";

        return prefix.Value.GetValue<PrefixType>() switch
        {
            PrefixType.Byte => "reader.ReadByte()",
            PrefixType.SByte => "reader.ReadSByte()",
            PrefixType.Short => "reader.ReadShort()",
            PrefixType.UShort => "reader.ReadUShort()",
            _ => "reader.ReadInt()"
        };
    }
}


