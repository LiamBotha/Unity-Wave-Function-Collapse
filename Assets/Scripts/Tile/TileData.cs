using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    private TileBase tilebase;
    private int x, y;
    private Dictionary<Direction, List<TileBase>> possibleNeighbours;

    public TileBase Tilebase { get => tilebase; set => tilebase = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public Dictionary<Direction, List<TileBase>> PossibleNeighbours { get => possibleNeighbours; set => possibleNeighbours = value; }

    public TileData(TileBase tilebase, int x, int y)
    {
        this.tilebase = tilebase;
        this.x = x;
        this.y = y;
    }

    public void AddPossibleNeighbourInDirection(Direction dir, TileBase neighbour)
    {
        if(!possibleNeighbours.ContainsKey(dir))
        {
            possibleNeighbours.Add(dir, new List<TileBase>());
        }

        possibleNeighbours[dir].Add(neighbour);
    }
}
