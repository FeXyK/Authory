public class Map
{
    public int MapIndex { get; set; }
    public string MapName { get; set; }

    public string MapIP { get; set; }
    public int MapPort { get; set; }


    public Map(int mapIndex, string mapName)
    {
        MapIndex = mapIndex;
        MapName = mapName;
    }
}