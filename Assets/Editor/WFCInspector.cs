using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WFCore))]
public class WFCInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WFCore myScript = (WFCore)target;
        
        if(GUILayout.Button("Create Tilemap"))
        {
            //myScript.Invoke("RestartTilemap",0);
        }
        if (GUILayout.Button("Save Tilemap"))
        {
            myScript.SaveTilemap();
        }
    }
}

[CustomEditor(typeof(WaveFunctionCollapseManager))]
public class WFCModuleInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WaveFunctionCollapseManager myScript = (WaveFunctionCollapseManager)target;

        if (GUILayout.Button("Create Tilemap"))
        {
            //myScript.Invoke("Awake", 0);
        }
        if (GUILayout.Button("Save Tilemap"))
        {
            myScript.SaveTilemap();
        }
    }
}
