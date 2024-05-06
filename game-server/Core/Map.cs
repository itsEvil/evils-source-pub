using common;

namespace game_server.Core;
public class Map(int width, int height) {
    public readonly WMapTile[] Tiles = new WMapTile[width * height];
    public readonly int Height = width;
    public readonly int Width = height;
    public bool Contains(int x, int y) {
        if(x < 0 || y < 0)
            return false;
        if (x > Width || y > Height)
            return false;
        return true;
    }
}
