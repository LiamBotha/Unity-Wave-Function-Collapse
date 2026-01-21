using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WaveFunctionCollapseManager : MonoBehaviour
{
    [SerializeField] private GameObject modulesParent;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private int width = 3, height = 3;

    private List<Module> modules;

    private ModuleContainer[][] grid;

    private ModuleReader inputReader;
    private CustomGrid customGrid;

    private bool outputStarted;
    private bool tilemapCompleted;

    // Start is called before the first frame update
    public void Awake()
    {
        outputStarted = false;
        tilemapCompleted = false;

        modules = modulesParent.GetComponentsInChildren<Module>().ToList(); // get all modules to build possibilities
        customGrid = FindFirstObjectByType<CustomGrid>();
        customGrid.mapObject = mapObject;

        inputReader = new ModuleReader(modules.ToArray());

        for (int i = 0; i < mapObject.transform.childCount; ++i)
        {
            Destroy(mapObject.transform.GetChild(i).gameObject);
        }

        if (inputReader.GotInput)
            InitializeGrid();
    }

    // Update is called once per frame
    private void Update()
    {
        if (inputReader.GotInput && !IsFullyCollapsed())
        {
            Iterate();
        }
        else if (inputReader.GotInput && outputStarted == false)
        {
            outputStarted = true;
            GenerateOutputTilemap();
        }
    }

    // enables every possible tile for each index in grid
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
    }

    // Core loop of the algorithm
    private void Iterate()
    {
        Vector2Int coords = GetLowestEntropy();
        Collapse(coords);
        Propagate(coords);
    }

    // Collapses the position in grid to a single tile
    private void Collapse(Vector2Int coords)
    {
        int x = coords.x, y = coords.y;

        ModuleContainer[] gridPos = grid[x + width * y];

        float totalWeight = 0;
        foreach (ModuleContainer possibleTile in gridPos)
        {
            totalWeight += inputReader.GetTileWeight(possibleTile.Module);
        }

        float randVal = UnityEngine.Random.Range(0f, 1f) * totalWeight;

        foreach (ModuleContainer possibleTile in gridPos)
        {
            randVal -= inputReader.GetTileWeight(possibleTile.Module);

            if (!(randVal <= 0))
                continue;

            grid[x + width * y] = new[] { possibleTile };
            break;
        }
    }

    // removes nonviable tiles from neighbors of the tile at coords
    private void Propagate(Vector2Int coords)
    {
        Vector2Int[] directions = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        Stack<Vector2Int> propagationStack = new Stack<Vector2Int>();

        propagationStack.Push(coords);

        
        while (propagationStack.Count > 0)
        {
            Vector2Int current = propagationStack.Pop();
            int x = current.x, y = current.y;

            ModuleContainer[] currPossibleTiles = grid[x + width * y];

            for (int d = 0; d < directions.Length; ++d)
            {
                int dx = x + directions[d].x, dy = y + directions[d].y;
                int dirIndex = (dx + width * dy);

                if (dirIndex <= 0 || dirIndex >= grid.Length)
                    continue;
                
                List<ModuleContainer> otherCoords = grid[dirIndex].ToList();

                foreach (ModuleContainer otherTile in otherCoords)
                {
                    bool isCompat = (
                        from currTile in currPossibleTiles
                        where inputReader.CheckForPossibleTile(currTile.Module, (Direction)d, otherTile.Module) && otherTile.IsPossible
                        select currTile
                    ).Any();

                    if (isCompat)
                        continue;

                    otherTile.IsPossible = false;
                    propagationStack.Push(new Vector2Int(dx, dy));
                }

                grid[dx + width * dy] = otherCoords.Where(val => val.IsPossible == true).ToArray();

                if (grid[dx + width * dy].Length != 0)
                    continue;

                InitializeGrid();
                propagationStack.Clear();
                break;
            }
        }
    }

    private float GetEntropy(int x, int y)
    {
        float sumWeights = 0;
        float sumWeightsLog = 0;

        foreach (ModuleContainer possibleTile in grid[x + width * y])
        {
            if (!possibleTile.IsPossible)
                continue;

            float weight = inputReader.GetTileWeight(possibleTile.Module);
            sumWeights += weight;
            sumWeightsLog += weight * (float)Math.Log(weight);
        }

        return (float)Math.Log(sumWeights) - (sumWeightsLog / sumWeights);
    } // returns the number of possible tiles at the requested position

    private Vector2Int GetLowestEntropy()
    {
        float minEntropy = -1;
        var minCoords = new Vector2Int();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x + width * y].Length > 1)
                {
                    float entropy = GetEntropy(x, y) - (UnityEngine.Random.Range(0f, 1f) / 1000);

                    if (!Mathf.Approximately(minEntropy, -1) && !(entropy < minEntropy))
                        continue;

                    minEntropy = entropy;
                    minCoords = new Vector2Int(x, y);
                }
            }
        }

        return minCoords;
    } // returns the postion with the lowest number of possible  tiles

    // checks if there is only one tile for each position and the algorithm is completed
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
    }

    // Places all the tiles onto the tilemap 
    private void GenerateOutputTilemap()
    {
        for (int y = 1; y <= height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Module tileToPlace = grid[x + width * (height - y)][0].Module;

                customGrid.SetTile(tileToPlace.gameObject, x - (width / 2f), y - (height / 1.8f));
            }
        }
        tilemapCompleted = true;
    }

    // Saves tilemap out to a prefab. prefab must be placed on a grid object
    public void SaveTilemap() // TODO - rework for gameobjects
    {
        if (!tilemapCompleted)
            return;

        GameObject objectToSave = mapObject.gameObject;

        PrefabUtility.SaveAsPrefabAsset(objectToSave, "Assets/Saved/Output" + (mapObject.transform.GetChild(0).GetHashCode() + UnityEngine.Random.Range(0,10000)) + ".prefab");
    }
}

