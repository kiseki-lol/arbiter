namespace Kiseki.Arbiter.Models;

using MessagePack;

[MessagePackObject]
public class Signal
{
    [Key(0)]
    public Guid Uuid { get; set; }

    [Key(1)]
    public Command Command { get; set; }

    [Key(2)]
    public Dictionary<string, string>? Data { get; set; }
}