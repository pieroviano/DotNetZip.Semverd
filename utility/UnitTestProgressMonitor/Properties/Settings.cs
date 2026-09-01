// Decompiled with JetBrains decompiler
// Type: UnitTestProgressMonitor.Properties.Settings
// Assembly: UnitTestProgressMonitor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 94234F9C-809A-4975-9F88-5ACE4AC3B10B
// Assembly location: D:\CommonLibrary\DotNetZip\src\Zip Tests\Resources\UnitTestProgressMonitor.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

#nullable disable
namespace UnitTestProgressMonitor.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
  private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

  public static Settings Default
  {
    get
    {
      Settings defaultInstance = Settings.defaultInstance;
      return defaultInstance;
    }
  }
}
