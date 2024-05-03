namespace common;
public class WMapTile(string biome = "", byte elevation = 0, float moisture = 0f, string name = "", 
    int polygonId = 0, Region region = Region.None, Terrain terrain = Terrain.None, ushort tileId = 0, string tileObj = "") : IEquatable<WMapTile>
{
    public readonly string Biome = biome;
    public readonly byte Elevation = elevation;
    public readonly float Moisture = moisture;
    public readonly string Name = name;
    public readonly int PolygonId = polygonId;
    public readonly Region Region = region;
    public readonly Terrain Terrain = terrain;
    public readonly ushort TileId = tileId;
    public readonly string TileObj = tileObj;

    public bool Equals(WMapTile other) => TileId == other.TileId && TileObj == other.TileObj && Name == other.Name && Terrain == other.Terrain && Region == other.Region;
}
