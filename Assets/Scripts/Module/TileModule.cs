using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileModule : MonoBehaviour
{
    [SerializeField] private int frequency = 1; // how rare this tile is
    [SerializeField] private bool isSymmetrical = false;
    [SerializeField] private bool canRepeatUp = true;
    [SerializeField] private bool canRepeatDown = true;
    [SerializeField] private bool canRepeatLeft = true;
    [SerializeField] private bool canRepeatRight = true;

    [SerializeField] private int connectorUp;
    [SerializeField] private int connectorDown;
    [SerializeField] private int connectorLeft;
    [SerializeField] private int connectorRight;

    [SerializeField] private TileModule[] excludedNeighbours; // TODO redo for each side?

    public int ConnectorUp { get => connectorUp; }
    public int ConnectorDown { get => connectorDown; }
    public int ConnectorLeft { get => connectorLeft; }
    public int ConnectorRight { get => connectorRight; }
    public int Frequency { get => frequency; }
    public bool IsPossible { get; set; }
    public bool CanRepeatUp { get => canRepeatUp; }
    public bool CanRepeatDown { get => canRepeatDown; }
    public bool CanRepeatLeft { get => canRepeatLeft; }
    public bool CanRepeatRight { get => canRepeatRight; }
    public TileModule[] ExcludedNeighbours { get => excludedNeighbours; }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up, connectorUp.ToString());
        Handles.Label(transform.position + Vector3.down, connectorDown.ToString());
        Handles.Label(transform.position + Vector3.left, connectorLeft.ToString());
        Handles.Label(transform.position + Vector3.right, connectorRight.ToString());
    }
    #endif
}
