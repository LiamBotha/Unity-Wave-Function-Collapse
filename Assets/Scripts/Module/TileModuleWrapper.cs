
public class TileModuleWrapper
{
    public TileModule TileModule { get; set; } // why not make this an array that stores all possibilities?
    public bool IsPossible { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public TileModuleWrapper(TileModule tileModule, int x, int y)
    {
        IsPossible = true;
        TileModule = tileModule;
        X = x;
        Y = y;
    }
}
