// this file needs to be synchronized with the server! (see playerManager.ts PlayerState)
// TODO(julian): auto-generate from a single source of truth
// because these things can easily get out of sync, remember to tag commits that work together!
// keep these type of files as simple as possible! (POCO) https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-5-0#serialization-example

public class Position
{
    public float x { get; set; }
    public float y { get; set; }
}

public class PlayerData
{
    public string uuid { get; set; }
    public string name { get; set; }
    public Position pos;
    public bool connected { get; set; }
}
