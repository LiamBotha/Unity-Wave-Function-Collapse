using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Direction
{
    Up,
    Down,
    Left, 
    Right
}

public class InputReader : IReader<TileBase>
{
    private Tilemap inputTilemap; // maybe could make an array to sample many rulesets with wrapping;

    private List<TileBase> tileTypes;
    private Dictionary<TileBase, Dictionary<Direction, List<TileBase>>> compatibilities;
    private Dictionary<TileBase, float> weights;
    private BoundsInt bounds;

    private bool wrapsAround;
    private bool gotInput = false;

    public TileBase[] AllTiles { get; }
    public List<TileBase> TileTypes { get => tileTypes;}
    internal Dictionary<TileBase, Dictionary<Direction, List<TileBase>>> Compatibilities { get => compatibilities; }
    public Dictionary<TileBase, float> Weights { get => weights; }
    public bool GotInput { get => gotInput; set => gotInput = value; }

    public InputReader(Tilemap inputTilemap, bool wrapsAround = true)
    {
        this.inputTilemap = inputTilemap;
        this.wrapsAround = wrapsAround;

        compatibilities = new Dictionary<TileBase, Dictionary<Direction, List<TileBase>>>();
        weights = new Dictionary<TileBase, float>();
        tileTypes = new List<TileBase>();

        inputTilemap.CompressBounds();
        bounds = inputTilemap.cellBounds;
        AllTiles = inputTilemap.GetTilesBlock(bounds);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                TileBase tile = GetTileAt(x, y);
                if (tile != null)
                {
                    SetTileWeight(tile);
                    SetTileAdjacencies(x, y);
                }
            }
        }

        //for (int y = 0; y < bounds.size.y; y++)
        //{
        //    for (int x = 0; x < bounds.size.x; x++)
        //    {
        //        TileBase tile = GetTileAt(x, y);
        //        if (tile != null)
        //        {
        //            GetTileNeighboursInAllDirections(tile);
        //        }
        //    }
        //}

        gotInput = true;
    }

    private void SetTileWeight(TileBase tile)
    {
        if (!weights.ContainsKey(tile))
            weights.Add(tile, 0);

        weights[tile] += 1; 
    }

    private void SetTileAdjacencies(int x, int y)
    {
        TileBase tile = GetTileAt(x, y);

        if (tile != null && !compatibilities.ContainsKey(tile))
        {
            compatibilities.Add(tile, new Dictionary<Direction, List<TileBase>>());
            tileTypes.Add(tile);
        }

        if(tile != null)
        {
            var neighbourUp = GetTileInDirection(x, y, Direction.Up);
            var neighbourDown = GetTileInDirection(x, y, Direction.Down);
            var neighbourLeft = GetTileInDirection(x, y, Direction.Left);
            var neighbourRight = GetTileInDirection(x, y, Direction.Right);

            if (neighbourUp != null)
                AddNeighbourInDirection(compatibilities[tile], neighbourUp, Direction.Up);
            if (neighbourDown != null)
                AddNeighbourInDirection(compatibilities[tile], neighbourDown, Direction.Down);
            if (neighbourLeft != null)
                AddNeighbourInDirection(compatibilities[tile], neighbourLeft, Direction.Left);
            if (neighbourRight != null)
                AddNeighbourInDirection(compatibilities[tile], neighbourRight, Direction.Right);
        }
    }

    private void AddNeighbourInDirection(Dictionary<Direction, List<TileBase>> adjacenciesDictionary, TileBase neighbourTile, Direction dir)
    {
        if(!adjacenciesDictionary.ContainsKey(dir))
        {
            adjacenciesDictionary.Add(dir, new List<TileBase>());
        }

        if (!adjacenciesDictionary[dir].Contains(neighbourTile))
            adjacenciesDictionary[dir].Add(neighbourTile);
    }

    private TileBase GetTileAt(int x, int y)
    {
        if(wrapsAround)
        {
            x = mod(x, bounds.size.x);
            y = mod(y, bounds.size.y);
        }

        if ((x + bounds.size.x * y > -1) && (x + bounds.size.x * y < AllTiles.Length))
            return AllTiles[x + (bounds.size.x * y)];

        return null;
    }

    private TileBase GetTileInDirection(int x, int y, Direction dir) //array starts bottom left
    {
        TileBase neighbourTile = null;

        switch (dir)
        {
            case Direction.Up:
                {
                    neighbourTile = GetTileAt(x, y + 1);
                }
                break;
            case Direction.Down:
                {
                    neighbourTile = GetTileAt(x, y - 1);
                }
                break;
            case Direction.Left:
                {
                    neighbourTile = GetTileAt(x - 1, y);
                }
                break;
            case Direction.Right:
                {
                    neighbourTile = GetTileAt(x + 1, y);
                }
                break;
            default:
                neighbourTile = null;
                break;
        }

        return neighbourTile;
    }

    private void GetTileNeighboursInAllDirections(TileBase tile)
    {
        if(tile != null)
        {
            foreach(Direction dir in Enum.GetValues(typeof(Direction)))
            {
                if(compatibilities[tile].ContainsKey(dir))
                {
                    var compatNeighboursInDir = compatibilities[tile][dir];
                    string output = "tile: " + tile.name + ", " + dir.ToString() + " neighbour: ";

                    foreach (var neighbour in compatNeighboursInDir)
                    {
                        output += neighbour.name + ", ";
                    }

                    //Debug.Log("Tile Adjacencies: " + output);
                }
            }
        }
    }

    public bool CheckForPossibleTile(TileBase currentTile, Direction dir, TileBase tileToFind)
    {
        if (Compatibilities.ContainsKey(currentTile) && Compatibilities[currentTile].ContainsKey(dir))
        {
            bool hasTile = Compatibilities[currentTile][dir].Contains(tileToFind);

            return hasTile;
        }

        return false;
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public TileBase GetTileTypeAtIndex(int index)
    {
        return tileTypes[index];
    }

    public int GetTileTypesCount()
    {
        return tileTypes.Count;
    }

    public float GetTileWeight(TileBase tile)
    {
        return weights[tile];
    }
}
