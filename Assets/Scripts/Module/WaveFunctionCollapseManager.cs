using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class WaveFunctionCollapseManager : MonoBehaviour
{
    [SerializeField] private GameObject modulesParent; // obj holding all the modules
    [SerializeField] private GameObject mapObject; // obj to hold all the tiles
    [SerializeField] private int startWidth = 10, startHeight = 10; // how many tiles to place
    private int numTilesX, numTilesY;

    private List<TileModule> modules;
    private TileModuleWrapper[][] grid;
    private TileModuleReader inputReader; // actually handles calculating possible tiles
    private CustomGrid customGrid; // handles tile object placement in world
    [SerializeField] private Slider slider;
    
    private bool tilemapCompleted; // finished generating tilemap ?

    // Start is called before the first frame update
    public void Awake()
    {
        customGrid = FindFirstObjectByType<CustomGrid>();
        
        // get all modules to build possibilities
        modules = modulesParent.GetComponentsInChildren<TileModule>().ToList();
        
        // init the reader with all the tile modules
        inputReader = new TileModuleReader(modules.ToArray());
        
        // clear existing tiles & fill the grid with the possible tiles
        InitializeGrid();
    }

    public void StartMapGeneration()
    {
        StopAllCoroutines();
        
        //outputStarted = false;
        tilemapCompleted = false;
        
        customGrid.mapParent = mapObject;

        // cache the slider value only when starting generation
        customGrid.tileSize = slider.value;

        // adjust number of tiles to place based on tile size
        numTilesX = Mathf.FloorToInt(startWidth / customGrid.tileSize);
        numTilesY = Mathf.FloorToInt(startHeight / customGrid.tileSize);

        if (Camera.main != null)
        {
            Debug.Log(numTilesY + ", " + ((numTilesY - 1) * customGrid.tileSize) / 2f);
            
            Vector3 vector3 = Camera.main.transform.position;
            vector3.y = ((numTilesY - 1) * customGrid.tileSize) / 2f;
            Camera.main.transform.position = vector3;
        }

        // clear old tiles placed 
        for (int i = 0; i < mapObject.transform.childCount; ++i)
        {
            Destroy(mapObject.transform.GetChild(i).gameObject);
        }
        
        InitializeGrid();
        
        StartCoroutine(GenerateMap(customGrid.tileSize));
    }
    
    private IEnumerator GenerateMap(float tileSize)
    {
        // are there tiles that haven't been set yet?
        while (!IsFullyCollapsed())
        {
            Iterate();

            yield return null;

        }

        yield return null;
    }
    
    // for each position in the grid fills the module with every possible tile neighbor
    private void InitializeGrid()
    {
        grid = new TileModuleWrapper[numTilesX * numTilesY][];

        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                // new create a new wrapper array and add all possible tile types to it 
                grid[x + numTilesX * y] = new TileModuleWrapper[inputReader.GetTileTypesCount()];
                for (int i = 0; i < inputReader.GetTileTypesCount(); ++i)
                {
                    grid[x + numTilesX * y][i] = new TileModuleWrapper(inputReader.GetTileTypeAtIndex(i), x, y);
                    customGrid.SetTile(null, x, y);
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
    
        GenerateOutputTilemap();
    }

    // Collapses the position in grid to a single tile
    private void Collapse(Vector2Int coords)
    {
        TileModuleWrapper[] gridPos = grid[coords.x + numTilesX * coords.y];

        // Calculate weight of all possible tiles at each position in grid to determine which to collapse
        float totalWeight = gridPos.Sum(possibleTile => inputReader.GetTileWeight(possibleTile.TileModule));
        float randVal = UnityEngine.Random.Range(0f, 1f) * totalWeight;

        foreach (TileModuleWrapper possibleTile in gridPos)
        {
            randVal -= inputReader.GetTileWeight(possibleTile.TileModule);

            if (randVal > 0)
                continue;
            
            // collapse possibilities into a single tile
            grid[coords.x + numTilesX * coords.y] = new[] { possibleTile };
            break;
        }
    }

    // removes nonviable tiles from neighbors of the tile at coords
    private void Propagate(Vector2Int coords)
    {
        Vector2Int[] directions = { new(0, -1), new(0, 1), new(-1, 0), new(1, 0) };
        Stack<Vector2Int> propagationStack = new ();

        propagationStack.Push(coords);
        
        // keep going till the neighbor's possibilities can't be further reduced
        while (propagationStack.Count > 0)
        {
            Vector2Int currentNeighborPos = propagationStack.Pop();
            TileModuleWrapper[] neighborPossibleTiles = grid[currentNeighborPos.x + numTilesX * currentNeighborPos.y];
            
            // for each neighbor - left, right, top, bottom
            for (int d = 0; d < directions.Length; ++d)
            {
                int dirX = currentNeighborPos.x + directions[d].x;
                int dirY = currentNeighborPos.y + directions[d].y;
                int neighborIndex = (dirX + numTilesX * dirY);

                // make sure index is in bounds
                if (neighborIndex < 0 || neighborIndex >= grid.Length)
                    continue;
                
                foreach (TileModuleWrapper potentialNeighborTile in grid[neighborIndex])
                {
                    bool bIsCompatible = (
                        from possibleTile in neighborPossibleTiles
                        where inputReader.CheckForPossibleTile(possibleTile.TileModule, (Direction)d, potentialNeighborTile.TileModule) && potentialNeighborTile.IsPossible
                        select possibleTile
                    ).Any();

                    if (bIsCompatible)
                        continue;
                    
                    potentialNeighborTile.IsPossible = false;
                    propagationStack.Push(new Vector2Int(dirX, dirY)); // removed tile add to list of propagations
                }

                // filter tiles to only be possible tiles
                grid[dirX + numTilesX * dirY] = grid[neighborIndex].Where(potentialTile => potentialTile.IsPossible).ToArray();

                // make sure it hasn't run out of possible tiles
                if (grid[dirX + numTilesX * dirY].Length != 0)
                    continue;
                
                // failed to get a valid grid, so restart from beginning
                InitializeGrid();
                propagationStack.Clear();
                break;
            }
        }
    }

    // returns the entropy of possible tiles at the requested position
    private float GetEntropy(int x, int y)
    {
        float sumWeights = 0;
        float sumWeightsLog = 0;

        foreach (TileModuleWrapper potentialTile in grid[x + numTilesX * y])
        {
            if (!potentialTile.IsPossible)
                continue;

            float weight = inputReader.GetTileWeight(potentialTile.TileModule);
            sumWeights += weight;
            sumWeightsLog += weight * (float)Math.Log(weight);
        }

        return (float)Math.Log(sumWeights) - (sumWeightsLog / sumWeights);
    }

    // returns the position with the lowest number of possible tiles
    private Vector2Int GetLowestEntropy()
    {
        float minEntropy = -1;
        var minCoords = new Vector2Int();

        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                if (grid[x + numTilesX * y].Length <= 1)
                    continue;

                float entropy = GetEntropy(x, y) - (UnityEngine.Random.Range(0f, 1f) / 1000);

                if (Mathf.Approximately(minEntropy, -1) == false && entropy >= minEntropy)
                    continue;

                minEntropy = entropy;
                minCoords = new Vector2Int(x, y);
            }
        }

        return minCoords;
    }

    // checks if there is only one tile for each position and the algorithm is completed
    private bool IsFullyCollapsed()
    {
        // TODO - Keep track of collapsed tiles, then just check value matches array size
        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                if (grid[x + numTilesX * y].Length > 1)
                    return false;
            }
        }

        return true;
    }

    // Places all the tiles onto the tilemap 
    private void GenerateOutputTilemap()
    {
        for (int y = 1; y <= numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                // if there are multiple tiles then this gridPos hasn't been collapsed
                if (grid[x + numTilesX * (numTilesY - y)].Length > 1)
                    continue;

                float halfExtents = ((numTilesX - 1) / 2f);
                
                TileModule tileToPlace = grid[x + numTilesX * (numTilesY - y)][0].TileModule;
                customGrid.SetTile(tileToPlace.gameObject, x - halfExtents, y - 1);
            }
        }
        tilemapCompleted = true;
    }

    #if UNITY_EDITOR
    // Saves tilemap out to a prefab. prefab must be placed on a grid object
    public void SaveTilemap()
    {
        if (!tilemapCompleted)
            return;

        GameObject objectToSave = mapObject.gameObject;

        PrefabUtility.SaveAsPrefabAsset(objectToSave, "Assets/Saved/Output" + (mapObject.transform.GetChild(0).GetHashCode() + UnityEngine.Random.Range(0,10000)) + ".prefab");
    }
    #endif
}

