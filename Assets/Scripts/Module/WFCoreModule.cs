using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCoreModule : MonoBehaviour
{
    [SerializeField] private GameObject modulesParent;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private int width = 3, height = 3;

    private List<Module> modules = null;

    private ModuleContainer[][] grid;

    private ModuleReader inputReader;

    private bool outputStarted = false;
    private bool tilemapCompleted = false;

    // Start is called before the first frame update
    void Awake()
    {
        outputStarted = false;
        tilemapCompleted = false;

        modules = modulesParent.GetComponentsInChildren<Module>().ToList();

        inputReader = new ModuleReader(modules.ToArray());

        for(int i = 0; i < mapObject.transform.childCount; ++i)
        {
            Destroy(mapObject.transform.GetChild(i).gameObject);
        }

        if (inputReader.GotInput)
            InitializeGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputReader.GotInput && !IsFullyCollapsed())
            Iterate();
        else if (inputReader.GotInput && outputStarted == false)
        {
            outputStarted = true;
            OutputGrid();
            GenerateOutputTilemap();
        }
    }

    private void InitializeGrid()
    {
        grid = new ModuleContainer[width * height][];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x + width * y] = new ModuleContainer[inputReader.GetTileTypesCount()];
                for (int i = 0; i < inputReader.GetTileTypesCount(); ++i)
                {
                    grid[x + width * y][i] = new ModuleContainer(inputReader.GetTileTypeAtIndex(i), x, y);
                }
            }
        }
    } // enables every posible tile for each index in grid

    private void Iterate()
    {
        Vector2Int coords = GetLowestEntropy();
        Collapse(coords);
        Propagate(coords);
    } // Core loop of the algorithm

    private void Collapse(Vector2Int coords)
    {
        int x = coords.x, y = coords.y;

        var gridPos = grid[x + width * y];

        float totalWeight = 0;
        foreach (var possibleTile in gridPos)
        {
            totalWeight += inputReader.GetTileWeight(possibleTile.Module);
        }

        float randVal = UnityEngine.Random.Range(0f, 1f) * totalWeight;

        foreach (var possibleTile in gridPos)
        {
            randVal -= inputReader.GetTileWeight(possibleTile.Module);
            if (randVal <= 0)
            {
                grid[x + width * y] = new[] { possibleTile };
                //GameObject.Instantiate(possibleTile.Module.gameObject, new Vector3(x - (width / 2), height - (y + 1), 0), Quaternion.identity);
                //outputTilemap.SetTile(new Vector3Int(x - (width / 2), height - (y + 1), 0), possibleTile.Module);
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
                    List<ModuleContainer> otherCoords = grid[dirIndex].ToList();

                    foreach (var otherTile in otherCoords)
                    {
                        bool isCompat = (from currTile in currPossibleTiles
                                         where inputReader.CheckForPossibleTile(currTile.Module, (Direction)d, otherTile.Module) && otherTile.IsPossible
                                         select currTile).Any();

                        if (!isCompat)
                        {
                            otherTile.IsPossible = false;
                            propagationStack.Push(new Vector2Int(dx, dy));
                        }
                    }

                    grid[dx + width * dy] = otherCoords.Where(val => val.IsPossible == true).ToArray();

                    if (grid[dx + width * dy].Length == 0)
                    {
                        Debug.Log("Failed");
                        OutputGrid();

                        InitializeGrid();
                        propagationStack.Clear();
                        break;
                    }
                    //else if (grid[dx + width * dy].Length == 1 && dx > 0 && dx < width && dy > 0 && dy < height)
                    //{
                    //    outputTilemap.SetTile(new Vector3Int(dx - (width / 2), height - (dy + 1), 0), grid[dx + width * dy][0].Module);
                    //}
                }
            }
        }

        //Debug.Log("Propagated changes at " + coords);
    } // removes nonviable tiles from neighbours of the tile at coords

    private float GetEntropy(int x, int y)
    {
        float sumWeights = 0;
        float sumWeightsLog = 0;

        foreach (var possibleTile in grid[x + width * y])
        {
            if (possibleTile.IsPossible)
            {
                float weight = inputReader.GetTileWeight(possibleTile.Module);
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

    private void OutputGrid()
    {
        string output = "";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x + width * y].Length > 1)
                    output += grid[x + width * y].Length;
                else if (grid[x + width * y].Length == 1)
                    output += grid[x + width * y][0].Module.name + " ";
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
                Module tileToPlace = grid[x + width * (height - y)][0].Module;
                GameObject.Instantiate(tileToPlace.gameObject, new Vector3(x - (width / 2), y - (height / 1.8f), 0), Quaternion.identity, mapObject.transform);
            }
        }
        tilemapCompleted = true;
    } // Places all the tiles onto the tilemap 

    public void SaveTilemap() // TODO - rework for gameobjects
    {
        if (tilemapCompleted == true)
        {
            GameObject objectToSave = mapObject.gameObject;

            PrefabUtility.SaveAsPrefabAsset(objectToSave, "Assets/Saved/Output" + (mapObject.transform.GetChild(0).GetHashCode() + UnityEngine.Random.Range(0,10000)) + ".prefab");
        }
    } // saves tilemap out to a prefab. prefab must be placed on a grid object
}

