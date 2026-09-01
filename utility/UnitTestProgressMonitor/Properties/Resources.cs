// Decompiled with JetBrains decompiler
// Type: UnitTestProgressMonitor.Properties.Resources
// Assembly: UnitTestProgressMonitor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 94234F9C-809A-4975-9F88-5ACE4AC3B10B
// Assembly location: D:\CommonLibrary\DotNetZip\src\Zip Tests\Resources\UnitTestProgressMonitor.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

#nullable disable
namespace UnitTestProgressMonitor.Properties;

[CompilerGenerated]
[DebuggerNonUserCode]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
internal class Resources
{
  private static ResourceManager resourceMan;
  private static CultureInfo resourceCulture;

  internal Resources()
  {
  }

  [EditorBrowsable(EditorBrowsableState.Advanced)]
  internal static ResourceManager ResourceManager
  {
    get
    {
      if (UnitTestProgressMonitor.Properties.Resources.resourceMan == null)
        UnitTestProgressMonitor.Properties.Resources.resourceMan = new ResourceManager("UnitTestProgressMonitor.Properties.Resources", typeof (UnitTestProgressMonitor.Properties.Resources).Assembly);
      return UnitTestProgressMonitor.Properties.Resources.resourceMan;
    }
  }

  [EditorBrowsable(EditorBrowsableState.Advanced)]
  internal static CultureInfo Culture
  {
    get => UnitTestProgressMonitor.Properties.Resources.resourceCulture;
    set => UnitTestProgressMonitor.Properties.Resources.resourceCulture = value;
  }
}
