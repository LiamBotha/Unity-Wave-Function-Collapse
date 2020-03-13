using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Module : MonoBehaviour
{
    [SerializeField] private int frequency = 1;
    [SerializeField] private bool isSymmetrical = false;
    [SerializeField] private bool canRepeatUp = true; // TODO - make top bot left right seperate
    [SerializeField] private bool canRepeatDown = true; // TODO - make top bot left right seperate
    [SerializeField] private bool canRepeatLeft = true; // TODO - make top bot left right seperate
    [SerializeField] private bool canRepeatRight = true; // TODO - make top bot left right seperate

    [SerializeField] private int connectorUp;
    [SerializeField] private int connectorDown;
    [SerializeField] private int connectorLeft;
    [SerializeField] private int connectorRight;

    [SerializeField] private Module[] excludedNeighbours; // TODO redo for each side?

    private bool isPossible;

    public int ConnectorUp { get => connectorUp; }
    public int ConnectorDown { get => connectorDown; }
    public int ConnectorLeft { get => connectorLeft; }
    public int ConnectorRight { get => connectorRight; }
    public int Frequency { get => frequency; }
    public bool IsPossible { get => isPossible; set => isPossible = value; }
    public bool CanRepeatUp { get => canRepeatUp; }
    public bool CanRepeatDown { get => canRepeatDown; }
    public bool CanRepeatLeft { get => canRepeatLeft; }
    public bool CanRepeatRight { get => canRepeatRight; }
    public Module[] ExcludedNeighbours { get => excludedNeighbours; }

    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up, connectorUp.ToString());
        Handles.Label(transform.position + Vector3.down, connectorDown.ToString());
        Handles.Label(transform.position + Vector3.left, connectorLeft.ToString());
        Handles.Label(transform.position + Vector3.right, connectorRight.ToString());
    }
}
