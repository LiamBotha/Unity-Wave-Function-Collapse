using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Direction
{
    Up = 0,
    Down = 1,
    Left = 2, 
    Right = 3,
}

// Stores all possibilities for tile modules and their weights
public class TileModuleReader : IReader<TileModule>
{
    private readonly TileModule[] tileModules;

    private Dictionary<TileModule, Dictionary<Direction, List<TileModule>>> CompatibleTileModules { get; }
    private Dictionary<TileModule, float> TileWeights { get; }

    public TileModuleReader(TileModule[] tileModules)
    {
        this.tileModules = tileModules;

        CompatibleTileModules = new Dictionary<TileModule, Dictionary<Direction, List<TileModule>>>();
        TileWeights = new Dictionary<TileModule, float>();
        
        // go over all tile modules created
        for (int i = 0; i < tileModules.Length; i++)
        {
            TileModule currTileModule = tileModules[i];
            // check all other tile module to see if it can be a neighbor
            for (int j = 0; j < tileModules.Length; j++)
            {
                TileModule otherTileModule = tileModules[j];

                if (currTileModule.ExcludedNeighbours.Contains(otherTileModule) == false)
                    AddPossibleNeighborToTileModule(currTileModule, otherTileModule);
            }

            // add the tile's weight to the weighting
            TileWeights.TryAdd(currTileModule, 0);
            TileWeights[currTileModule] = currTileModule.Frequency;
        }
    }
    
    // Sets all possible neighbor tiles in each direction
    private void AddPossibleNeighborToTileModule(TileModule currTileModule, TileModule neighborTileModule)
    {
        bool bIsSameTile = currTileModule == neighborTileModule;

        // check if the connectors match
        if (currTileModule.ConnectorUp == neighborTileModule.ConnectorDown)
        {
            // if it is the same tile check that it is allowed to repeat
            if ((bIsSameTile && currTileModule.CanRepeatUp) || bIsSameTile == false)
            {
                AddNeighbour(currTileModule, Direction.Up, neighborTileModule);
            }
        }

        if (currTileModule.ConnectorDown == neighborTileModule.ConnectorUp)
        {
            if ((bIsSameTile && currTileModule.CanRepeatDown) || bIsSameTile == false)
            {
                AddNeighbour(currTileModule, Direction.Down, neighborTileModule);
            }         
        }

        if (currTileModule.ConnectorLeft == neighborTileModule.ConnectorRight)
        {
            if ((bIsSameTile && currTileModule.CanRepeatLeft) || bIsSameTile == false)
            {
                AddNeighbour(currTileModule, Direction.Left, neighborTileModule);
            }
        }

        // ReSharper disable once InvertIf
        if (currTileModule.ConnectorRight == neighborTileModule.ConnectorLeft)
        {
            if ((bIsSameTile && currTileModule.CanRepeatRight) || bIsSameTile == false)
            {
                AddNeighbour(currTileModule, Direction.Right, neighborTileModule);
            }
        }
    }

    private void AddNeighbour(TileModule currTileModule, Direction dir, TileModule possibleNeighborTileModule)
    {
        if(CompatibleTileModules.ContainsKey(currTileModule) == false)
        {
            // create new entry for the tile
            CompatibleTileModules.Add(currTileModule, new Dictionary<Direction, List<TileModule>>());
        }

        if(CompatibleTileModules[currTileModule].ContainsKey(dir) == false)
        {
            // create new entry for the direction
            CompatibleTileModules[currTileModule].Add(dir, new List<TileModule>());
        }

        // add possible tile module to list of compatible tiles in direction
        CompatibleTileModules[currTileModule][dir].Add(possibleNeighborTileModule);
    }

    public TileModule GetTileTypeAtIndex(int index)
    {
        return tileModules[index];
    }

    public int GetTileTypesCount()
    {
        return tileModules.Length;
    }

    // For determining which tile to choose from the possibilities / how likely tile is to be chosen
    public float GetTileWeight(TileModule tile)
    {
        return TileWeights[tile];
    }

    
    public bool CheckForPossibleTile(TileModule currentTile, Direction dir, TileModule tileToFind)
    {
        // if there is no info for the tile or possibilities in the specified direction
        if (!CompatibleTileModules.ContainsKey(currentTile) || !CompatibleTileModules[currentTile].ContainsKey(dir))
            return false;
        
        // eg MedievalTile_53 - Right - MedievalTile_48
        bool hasTile = CompatibleTileModules[currentTile][dir].Contains(tileToFind);

        return hasTile;
    }
}
