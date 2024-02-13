// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

static class ZipArchiveExtensions
{
    public static List<string> Lines(this ZipArchiveEntry entry, Encoding? encoding = null)
    {
        return entry.ReadToString(encoding).Replace("\r\n", "\n").Split('\n').ToList();
    }

    public static string ReadToString(this ZipArchiveEntry entry, Encoding? encoding = null)
    {
        Stream stream = entry.Open();
        byte[] buffer = stream.ReadToEnd();
        // Remove UTF-8 BOM if present
        int index = 0;
        if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            index = 3;
        }
        encoding ??= Encoding.UTF8;
        string fileText = encoding.GetString(buffer, index, buffer.Length - index);
        return fileText;
    }

    public static ZipArchiveEntry GetNuspec(this ZipArchive package)
    {
        return package.Entries.Where(entry => entry.FullName.EndsWith(".nuspec")).Single();
    }

    public static (string? Name, string? Version) GetPackageNameAndVersion(this ZipArchive package)
    {
        var nuspecData = package.GetNuspec().ReadToString();

        XDocument doc = XDocument.Parse(nuspecData);
        XNamespace? ns = doc.Root?.GetDefaultNamespace() ?? "";
        XElement? versionElement = doc.Root?.Element(ns + "metadata")?.Element(ns + "version");
        XElement? nameElement = doc.Root?.Element(ns + "metadata")?.Element(ns + "id");
        return (nameElement?.Value, versionElement?.Value);
    }

    public static byte[] ReadToEnd(this Stream stream)
    {
        int bufferSize = 2048;
        byte[] buffer = new byte[bufferSize];
        int offset = 0;
        while (true)
        {
            int bytesRead = stream.Read(buffer, offset, bufferSize - offset);
            offset += bytesRead;
            if (bytesRead == 0)
            {
                break;
            }
            if (offset == bufferSize)
            {
                Array.Resize(ref buffer, bufferSize * 2);
                bufferSize *= 2;
            }
        }
        Array.Resize(ref buffer, offset);
        return buffer;
    }
}
