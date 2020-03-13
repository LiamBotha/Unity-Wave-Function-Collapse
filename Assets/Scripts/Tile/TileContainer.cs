using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileContainer
{
    private TileBase tilebase;
    private int x, y;
    private bool isPossible = true;

    public TileBase Tilebase { get => tilebase; set => tilebase = value; }
    public bool IsPossible { get => isPossible; set => isPossible = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public TileContainer(TileBase tilebase, int x, int y)
    {
        this.isPossible = true;
        this.tilebase = tilebase;
        this.x = x;
        this.y = y;
    }
}
