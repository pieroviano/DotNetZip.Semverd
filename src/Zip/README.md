# DotNetZip

Zip, zlib, gzip and bzip2 for .NET — Dino Chiesa's DotNetZip library, maintained and
repackaged.

Create and read zip archives with a small, direct API: no streams to wire together for the
common cases, no temp files to manage. Zip64, AES encryption, split archives, Unicode entry
names and self-extracting executables are all supported.

**Targets:** net35, net40, netstandard2.0

```
dotnet add package Net4x.DotNetZip
```

## Creating an archive

```csharp
using Ionic.Zip;

using (var zip = new ZipFile())
{
    zip.AddFile("report.pdf", "docs");     // into the "docs" folder in the archive
    zip.AddDirectory(@"C:\photos", "pics");
    zip.AddEntry("readme.txt", "generated at " + DateTime.Now);
    zip.Save("archive.zip");
}
```

## Reading and extracting

```csharp
using (var zip = ZipFile.Read("archive.zip"))
{
    foreach (ZipEntry e in zip)
        Console.WriteLine("{0}  {1} -> {2}", e.FileName, e.UncompressedSize, e.CompressedSize);

    zip.ExtractAll(@"C:\out", ExtractExistingFileAction.OverwriteSilently);
}
```

## Passwords and AES

```csharp
using (var zip = new ZipFile())
{
    zip.Password = "hunter2";
    zip.Encryption = EncryptionAlgorithm.WinZipAes256;   // or PkzipWeak
    zip.AddFile("secret.docx");
    zip.Save("secure.zip");
}
```

`Password` applies to entries added after it is set, so you can mix protected and
unprotected entries in one archive.

## Writing entry content directly

An entry's bytes can come from a delegate rather than a file, which keeps large or
generated content out of memory:

```csharp
using (var zip = new ZipFile())
{
    zip.AddEntry("data.csv", (name, stream) => WriteCsvTo(stream));
    zip.Save("archive.zip");
}
```

## Larger archives

```csharp
using (var zip = new ZipFile())
{
    zip.UseZip64WhenSaving = Zip64Option.AsNecessary;  // > 4 GB, or > 65535 entries
    zip.ParallelDeflateThreshold = 1024 * 1024;        // compress big entries on many cores
    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
    zip.AddDirectory(@"C:\big");
    zip.Save("big.zip");
}
```

Split (spanned) archives come from `MaxOutputSegmentSize`; progress is reported through the
`SaveProgress`, `ReadProgress` and `ExtractProgress` events.

## The compression codecs on their own

The streams underneath the zip implementation are public API, usable without touching an
archive:

```csharp
using Ionic.Zlib;

using (var raw  = File.OpenRead("data.bin"))
using (var gz   = File.Create("data.bin.gz"))
using (var comp = new GZipStream(gz, CompressionMode.Compress))
    raw.CopyTo(comp);
```

`Ionic.Zlib` provides `GZipStream`, `DeflateStream`, `ZlibStream`, `ZlibCodec` and
`ParallelDeflateOutputStream`. `Ionic.BZip2` provides `BZip2InputStream`,
`BZip2OutputStream` and `ParallelBZip2OutputStream`. `Ionic.Crc` provides `CRC32` and
`CrcCalculatorStream`.

## Self-extracting archives

```csharp
using (var zip = new ZipFile())
{
    zip.AddDirectory(@"C:\payload");
    zip.SaveSelfExtractor("setup.exe", SelfExtractorFlavor.ConsoleApplication);
}
```

`SaveSelfExtractor` emits a runnable `.exe`. It needs the stub resources embedded in the
.NET Framework builds, so it is available on the **net35 and net40** targets rather than on
netstandard2.0.

## License

Microsoft Public License (Ms-PL) for DotNetZip's own code, plus the terms of the projects
it derives from — zlib, jzlib, Apache Commons Compress and the LZMA SDK. See the `LICENSE`
and `NOTICE` files in the repository.
