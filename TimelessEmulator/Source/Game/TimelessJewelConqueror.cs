namespace TimelessEmulator.Game;

public class TimelessJewelConqueror
{

    public uint Index { get; private set; }

    public uint Version { get; private set; }
    public string Name { get; private set; }

    public TimelessJewelConqueror(uint index, uint version, string name)
    {
        this.Index = index;
        this.Version = version;
        this.Name = name;
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Index)}: {Index}, {nameof(Version)}: {Version}";
    }

}
