using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReader<T>
{
    T GetTileTypeAtIndex(int index);

    int GetTileTypesCount();

    float GetTileWeight(T tile);

    bool CheckForPossibleTile(T currentTile, Direction dir, T tileToFind);
}
