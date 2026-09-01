# DotNetZip

A maintained build of **DotNetZip**, Dino Chiesa's zip, zlib, gzip and bzip2 library for .NET.

The library reads and writes zip archives — including Zip64, AES encryption, split
archives and self-extracting executables — and also ships the compression codecs it is
built on (`Ionic.Zlib`, `Ionic.BZip2`) as usable stream classes in their own right.

The `DotNetZip` package targets **net35**, **net40** and **netstandard2.0**. Everything
else in the repository (tools, examples, tests) is .NET Framework.

For the library itself, its API and usage examples, see [`src/Zip/README.md`](src/Zip/README.md).

## Layout

```
src/
  Zip/                DotNetZip.csproj — the shipping library (net35, net40, netstandard2.0)
  Zip.Shared/         shared sources for the zip implementation (.projitems)
  Zlib/ Zlib.Shared/  Ionic.Zlib — deflate, zlib and gzip streams
  BZip2/              Ionic.BZip2 — bzip2 streams, including a parallel compressor
  Zip Reduced/        Ionic.Zip.Reduced — a smaller build of the same sources
  CommonSrc/          code shared across the assemblies (CRC32, …)
  Tools/              ZipIt, UnZip, GZip, BZip2, ConvertZipToSfx, Win Forms App
  Examples/C#/        CreateZip, ReadZip, ZipDir, ZipTreeView, QuickZip
  TestCommon/         Ionic.TestCommon — xUnit infrastructure shared by the test projects
  Zip Tests/          Zlib Tests/   BZip2 Tests/
utility/              support projects used only by the tests
```

`utility/` sits outside `src/` and is therefore **not** covered by `src/Directory.Build.Props`;
those four projects carry their own version and assembly metadata.

## Building

Open `src/DotNetZip.sln`, or build from the command line with **MSBuild**:

```
msbuild "src\DotNetZip.sln" /t:Build /p:Configuration=Debug /m
```

> **Use MSBuild, not `dotnet build`.** The net35/net40 targets embed non-string resources,
> which the .NET SDK's `GenerateResource` rejects with `MSB3823`/`MSB3822` unless
> `GenerateResourceUsePreserializedResources` is set. The desktop MSBuild that ships with
> Visual Studio builds them as-is.

`src/Help/HelpViewer.shfbproj` builds the API documentation and needs
[Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB) with `SHFBROOT` set. It
is part of the solution, so a whole-solution build fails without it; build the individual
projects, or install SHFB, if you don't need the docs.

### Packaging

`DotNetZip.csproj` has `GeneratePackageOnBuild`, so a build drops a `.nupkg` into
`src/Packages/`. The package id is **`Net4x.DotNetZip`**. Versions come from
`src/Directory.NuGet.props` (`DotNetZipVersion`) combined with a build suffix, giving
`<version>.<yy><day-of-year>`.

## Tests

Three xUnit suites — `Zip Tests`, `Zlib Tests` and `BZip2 Tests` — sharing
`src/TestCommon` (`Ionic.TestCommon`).

```
dotnet test "src\Zip Tests\Zip Tests.csproj" --no-build
```

These are integration tests: they drive the library against the real file system, and many
of them shell out to other archivers to prove interoperability. A few things follow from
that, and they are worth knowing before you read a red test as a bug.

**Test parallelization is disabled** for all three assemblies
(`TestCommon/TestAssemblyInfo.cs`). Each test creates a temp directory and calls
`Directory.SetCurrentDirectory` into it; the current directory is process-wide, so tests
running concurrently would read and write each other's files. For the same reason, **do not
run two test sessions at once** — an IDE runner and a command-line runner together will
interfere.

**Shadow copying is off** (`TestCommon/xunit.runner.json`). Some tests locate the source
tree by walking up from the test assembly's location, which does not work from xUnit's
default shadow-copy directory.

### What some tests need from the machine

| Suite | Requirement | Without it |
|---|---|---|
| `Compatibility` | An **elevated** process — it registers the library for COM with `RegAsm /codebase`, which writes to `HKEY_CLASSES_ROOT` | All 56 tests skip with an explanatory message |
| WinZip interop (`WinZipAesTests`, some `Streams`) | WinZip (`wzzip.exe`, `wzunzip.exe`) | Fail with `no winzip!` |
| 7-Zip and Info-ZIP interop | `7z.exe`, `zip.exe`, `unzip.exe` | Fail similarly |
| `Split` spanned-archive tests | Permission to create symbolic links (elevation or Developer Mode) | Fail creating the link |
| `LongRunning`, `Zip64Tests` | Tens of minutes and many GB of scratch space | Very slow |

### Shared test infrastructure

`Ionic.TestCommon` exists because these tests were written for MSTest and later run under
NUnit, and a literal translation to xUnit would have lost a lot:

- **`Assert`** — the classic `AreEqual`/`IsTrue`/`IsNotNull` surface over `Xunit.Assert`,
  keeping the failure-message argument that xUnit's assertions do not have, and the
  element-wise sequence and cross-type numeric comparison that classic `AreEqual` implied.
  Files pick it up with `using Assert = Ionic.Tests.Assert;`.
- **`[ExpectedExceptionFact(typeof(T))]`** — a test that passes only if it throws exactly
  `T`. Built on xUnit's test-case extensibility point.
- **`TestBase` / `TestContext`** — per-test output, so `TestContext.WriteLine(...)` reaches
  the test report through xUnit's `ITestOutputHelper`.

## License

DotNetZip's own code is under the **Microsoft Public License (Ms-PL)**. It also includes
work derived from zlib, jzlib, Apache Commons Compress and the LZMA SDK, under their own
terms. See [LICENSE](LICENSE) and [NOTICE](NOTICE) — using the software requires accepting
all of them.
