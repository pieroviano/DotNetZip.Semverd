#nullable disable
namespace Ionic.IO;

public class ReparsePointInfo
{
  private string _printName;
  public string Source;
  public string Target;
  public ReparsePointFlavor Flavor;

  private ReparsePointInfo()
  {
  }

  public ReparsePointInfo(string source) => this.Source = source;

  public string PrintName
  {
    get => string.IsNullOrEmpty(this._printName) ? this.Target : this._printName;
    set => this._printName = value;
  }
}
