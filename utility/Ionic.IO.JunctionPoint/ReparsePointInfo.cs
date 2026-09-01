namespace Ionic.IO;

public class ReparsePointInfo
{
    private string _printName;
    public ReparsePointFlavor Flavor;
    public string Source;
    public string Target;

    private ReparsePointInfo()
    {
    }

    public ReparsePointInfo(string source)
    {
        Source = source;
    }

    public string PrintName
    {
        get => string.IsNullOrEmpty(_printName) ? Target : _printName;
        set => _printName = value;
    }
}