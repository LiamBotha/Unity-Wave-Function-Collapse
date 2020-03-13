using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleContainer
{
    private Module module;
    private int x, y;
    private bool isPossible = true;

    public Module Module { get => module; set => module = value; }
    public bool IsPossible { get => isPossible; set => isPossible = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public ModuleContainer(Module module, int x, int y)
    {
        this.isPossible = true;
        this.module = module;
        this.x = x;
        this.y = y;
    }
}
