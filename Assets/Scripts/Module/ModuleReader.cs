using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModuleReader : IReader<Module>
{
    private Module[] modules;

    private Dictionary<Module, Dictionary<Direction, List<Module>>> compatibilities;
    private Dictionary<Module, float> weights;
    private bool gotInput = false;

    public Dictionary<Module, Dictionary<Direction, List<Module>>> Compatibilities { get => compatibilities; set => compatibilities = value; }
    public Dictionary<Module, float> Weights { get => weights; set => weights = value; }
    public bool GotInput { get => gotInput; set => gotInput = value; }

    public ModuleReader(Module[] modules)
    {
        this.modules = modules;

        compatibilities = new Dictionary<Module, Dictionary<Direction, List<Module>>>();
        weights = new Dictionary<Module, float>();

        for (int i = 0; i < modules.Length; i++)
        {
            Module currModule = modules[i];

            for (int j = 0; j < modules.Length; j++)
            {
                Module otherModule = modules[j];

                if (!currModule.ExcludedNeighbours.Contains(otherModule))
                    SetModuleAdjacencies(currModule, otherModule);
            }

            if(!weights.ContainsKey(currModule))
            {
                weights.Add(currModule, 0);
            }

            weights[currModule] = currModule.Frequency;
        }

        gotInput = true;
    }

    private void SetModuleAdjacencies(Module currModule, Module otherModule)
    {
        bool isSelf = currModule == otherModule;

        if (currModule.ConnectorUp == otherModule.ConnectorDown)
        {
            if ((isSelf && currModule.CanRepeatUp) || !isSelf)
            {
                AddNeighbour(currModule, Direction.Up, otherModule);
            }
        }

        if (currModule.ConnectorDown == otherModule.ConnectorUp)
        {
            if ((isSelf && currModule.CanRepeatDown) || !isSelf)
            {
                AddNeighbour(currModule, Direction.Down, otherModule);
            }         
        }

        if (currModule.ConnectorLeft == otherModule.ConnectorRight)
        {
            if ((isSelf && currModule.CanRepeatLeft) || !isSelf)
            {
                AddNeighbour(currModule, Direction.Left, otherModule);
            }
        }

        if (currModule.ConnectorRight == otherModule.ConnectorLeft)
        {
            if ((isSelf && currModule.CanRepeatRight) || !isSelf)
            {
                AddNeighbour(currModule, Direction.Right, otherModule);
            }
        }
    }

    private void AddNeighbour(Module currModule, Direction dir, Module otherModule)
    {
        if(!compatibilities.ContainsKey(currModule))
        {
            compatibilities.Add(currModule, new Dictionary<Direction, List<Module>>());
        }

        if(!compatibilities[currModule].ContainsKey(dir))
        {
            compatibilities[currModule].Add(dir, new List<Module>());
        }

        compatibilities[currModule][dir].Add(otherModule);
    }

    public Module GetTileTypeAtIndex(int index)
    {
        return modules[index];
    }

    public int GetTileTypesCount()
    {
        return modules.Length;
    }

    public float GetTileWeight(Module tile)
    {
        return weights[tile];
    }

    public bool CheckForPossibleTile(Module currentTile, Direction dir, Module tileToFind)
    {
        if (Compatibilities.ContainsKey(currentTile) && Compatibilities[currentTile].ContainsKey(dir))
        {
            bool hasTile = Compatibilities[currentTile][dir].Contains(tileToFind);

            return hasTile;
        }

        return false;
    }
}
