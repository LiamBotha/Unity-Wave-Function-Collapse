using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCore : MonoBehaviour
{
    [SerializeField] private Tilemap inputTilemap = null;
    [SerializeField] private Tilemap outputTilemap = null;
    [SerializeField] private TileBase defaultTile = null;
    [SerializeField] private bool wrapsAround = true, isPlayerDriven = false;
    [SerializeField] private int width = 3, height = 3;

    private TileContainer[][] grid;
    private TileContainer[] playerGrid; // grid created by player 

    private InputReader inputReader;

    private bool outputStarted = false;
    private bool tilemapCompleted = false;

    // Start is called before the first frame update
    void Awake()
    {
        outputStarted = false;
        tilemapCompleted = false;

        inputReader = new InputReader(inputTilemap, wrapsAround);
        InitializeGrid();

        if (isPlayerDriven)
        {
            playerGrid = new TileContainer[width * height];

            string output = "";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int flippedY = height - (y + 1);
                    TileBase tileFromOutput = outputTilemap.GetTile(new Vector3Int(x - (width / 2), flippedY, 0));

                    if (tileFromOutput != null)
                    {
                        playerGrid[x + width * y] = new TileContainer(tileFromOutput, x, y);
                        grid[x + width * y] = new[] { new TileContainer(tileFromOutput, x, y) };
                        Propagate(new Vector2Int(x, y));

                        output += playerGrid[x + width * y].Tilebase.name.Replace("medievalTile", "T") + " ";
                    }
                }
                output += Environment.NewLine;
            }

            Debug.Log("Initial Grid State");
            OutputGrid(grid, width, height);
            Debug.Log(output);
            Debug.Log("--------------------------------------");
        }

        outputTilemap.ClearAllTiles();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputReader.GotInput && !IsFullyCollapsed() && !outputStarted)
        {
            Iterate();
        }
        else if (inputReader.GotInput && HasCollision() && !outputStarted)
        {
            Debug.Log("Failed Due to Collision!");

            outputTilemap.ClearAllTiles();
            InitializeGrid();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var gridObject = playerGrid[x + width * y];
                    if (gridObject != null)
                    {
                        grid[x + width * y] = new[] { gridObject };
                        Propagate(new Vector2Int(x, y));
                    }
                }
            }
        }
        else if (inputReader.GotInput && outputStarted == false)
        {
            outputStarted = true;
            OutputGrid(grid, width, height);
        }

        GenerateOutputTilemap();
    }

    private bool HasCollision()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x + width * y].Length == 0)
                    return true;
            }
        }

        return false;
    }

    private void InitializeGrid()
    {
        grid = new TileContainer[width * height][];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x + width * y] = new TileContainer[inputReader.TileTypes.Count];
                for (int i = 0; i < inputReader.TileTypes.Count; ++i)
                {
                    grid[x + width * y][i] = new TileContainer(inputReader.TileTypes[i], x, y);
                }
            }
        }

    } // enables every posible tile for each index in grid

    private void Iterate()
    {
        Debug.Log("--------------------------------------");
        Debug.Log("Pre Iterate");
        OutputGrid(grid, width, height);

        Vector2Int coords = GetLowestEntropy();
        Collapse(coords);
        Propagate(coords);

        Debug.Log("Post Iterate");
        OutputGrid(grid, width, height);
        Debug.Log("--------------------------------------");
    } // Core loop of the algorithm

    private void Collapse(Vector2Int coords)
    {
        int x = coords.x, y = coords.y;

        var gridPos = grid[x + width * y];

        float totalWeight = 0;
        foreach (var possibleTile in gridPos)
        {
            totalWeight += inputReader.Weights[possibleTile.Tilebase];
        }

        float randVal = UnityEngine.Random.Range(0f, 1f) * totalWeight;

        foreach (var possibleTile in gridPos)
        {
            randVal -= inputReader.Weights[possibleTile.Tilebase];
            if (randVal <= 0)
            {
                grid[x + width * y] = new[] { possibleTile };
                //outputTilemap.SetTile(new Vector3Int(x - (width / 2), height - (y + 1), 0), possibleTile.Tilebase);
                break;
            }
        }

        //Debug.Log("Collapsed at " + coords + ", " + grid[x + width * y].Length + " TotalWeight: " + totalWeight);
    } // collapses the position in grid to a single tile

    private void Propagate(Vector2Int coords)
    {
        //Debug.Log("Started Propagation at " + coords);
        Vector2Int[] directions = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        Stack<Vector2Int> propagationStack = new Stack<Vector2Int>();

        propagationStack.Push(coords);

        while (propagationStack.Count > 0)
        {
            var current = propagationStack.Pop();
            int x = current.x, y = current.y;

            var currPossibleTiles = grid[x + width * y];

            for (int d = 0; d < directions.Length; ++d)
            {
                int dx = x + directions[d].x, dy = y + directions[d].y;
                int dirIndex = (dx + width * dy);

                if (dirIndex > 0 && dirIndex < grid.Length)
                {
                    //Debug.Log("value: " + dirIndex + " grid " + grid.Length);
                    List<TileContainer> otherCoords = grid[dirIndex].ToList();

                    foreach (var otherTile in otherCoords)
                    {
                        bool isCompat = (from currTile in currPossibleTiles
                                         where inputReader.CheckForPossibleTile(currTile.Tilebase, (Direction)d, otherTile.Tilebase) && otherTile.IsPossible
                                         select currTile).Any();

                        if (!isCompat)
                        {
                            otherTile.IsPossible = false;
                            propagationStack.Push(new Vector2Int(dx, dy));
                        }
                    }

                    grid[dx + width * dy] = otherCoords.Where(val => val.IsPossible == true).ToArray();

                    if(grid[dx + width * dy].Length == 1 && dx > 0 && dx < width && dy > 0 && dy < height)
                    {
                        //outputTilemap.SetTile(new Vector3Int(dx - (width / 2), height - (dy + 1), 0), grid[dx + width * dy][0].Tilebase);
                    }
                    else if(grid[dx + width * dy].Length == 0)
                    {
                        propagationStack.Clear();
                        break;
                    }
                }
            }
        }

        //Debug.Log("Propagated changes at " + coords);
    } // removes nonviable tiles from neighbours of the tile at coords // propagate removes hand placed tiles if doesnt match a rule

    private float GetEntropy(int x, int y)
    {
        float sumWeights = 0;
        float sumWeightsLog = 0;

        foreach (var potentialTile in grid[x + width * y])
        {
            if (potentialTile.IsPossible)
            {
                float weight = inputReader.Weights[potentialTile.Tilebase];
                sumWeights += weight;
                sumWeightsLog += weight * (float)Math.Log(weight);
            }
        }

        return (float)Math.Log(sumWeights) - (sumWeightsLog / sumWeights);
    } // returns the number of possible tiles at the requested position

    private Vector2Int GetLowestEntropy()
    {
        float minEntropy = -1;
        Vector2Int minCoords = new Vector2Int();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x + width * y].Length > 1)
                {
                    float entropy = GetEntropy(x, y) - (UnityEngine.Random.Range(0f, 1f) / 1000);

                    if (minEntropy == -1 || entropy < minEntropy)
                    {
                        minEntropy = entropy;
                        minCoords = new Vector2Int(x, y);
                    }
                }
            }
        }

        return minCoords;
    } // returns the postion with the lowest number of possible  tiles

    private bool IsFullyCollapsed()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x + width * y].Length > 1)
                    return false;
            }
        }
        return true;
    } // checks if there is only one tile for each position and the alogorithm is completed

    private void OutputGrid(TileContainer[][] gridToParse, int yMax, int xMax)
    {
        string output = "";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                output += "[";
                if (gridToParse[x + width * y].Length != 1)
                {
                    if(gridToParse[x + width * y].Length < 3)
                    {
                        for (int i = 0; i < gridToParse[x + width * y].Length; i++)
                        {
                            output += gridToParse[x + width * y][i].Tilebase.name.Replace("medievalTile", "T") + " ";
                        }
                    }

                    output += "|" + gridToParse[x + width * y].Length;
                }
                else if (gridToParse[x + width * y].Length == 1)
                    output += gridToParse[x + width * y][0].Tilebase.name.Replace("medievalTile","T") + " ";
                output += "]";
            }
            output += Environment.NewLine;
        }

        Debug.Log(output);
    } // outputs the current state of the grid

    private void GenerateOutputTilemap()
    {
        for (int y = 1; y <= height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(grid[x + width * (height - y)] != null && grid[x + width * (height - y)].Length == 1)
                {
                    TileBase tileToPlace = grid[x + width * (height - y)][0].Tilebase;
                    outputTilemap.SetTile(new Vector3Int(x - (width / 2), y - 1, 0), tileToPlace);
                    //Debug.Log("Output tile - (" + x + "," + y + ")");
                }

                outputTilemap.SetTile(new Vector3Int(width, height, 0), defaultTile);
                outputTilemap.SetTile(new Vector3Int(width, -1, 0), defaultTile);
                outputTilemap.SetTile(new Vector3Int(-1, -1, 0), defaultTile);
                outputTilemap.SetTile(new Vector3Int(-1, height, 0), defaultTile);
            }
        }
        tilemapCompleted = true;
    } // Places all the tiles onto the tilemap 

    private void GeneratePlayerGrid()
    {
        for (int y = 1; y <= height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (playerGrid[x + width * (height - y)] != null)
                {
                    TileBase tileToPlace = playerGrid[x + width * (height - y)].Tilebase;
                    outputTilemap.SetTile(new Vector3Int(x - (width / 2), y - 1, 0), tileToPlace);
                }
            }
        }
    }

    public void SaveTilemap()
    {
        if(tilemapCompleted == true)
        {
            GameObject objectToSave = outputTilemap.gameObject;

            PrefabUtility.SaveAsPrefabAsset(objectToSave, "Assets/Saved/Output.prefab");
        }
    } // saves tilemap out to a prefab. prefab must be placed on a grid object
}
