using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CustomGrid : MonoBehaviour
{
    public float tileSize = 1;
    [SerializeField] private GameObject defaultTile;

    private Dictionary<Vector2, GameObject> gridObjects;
    
    public GameObject mapParent; // parent 'folder' for tiles
    private Camera cam;

    // Start is called before the first frame update
    private void Awake()
    {
        cam = Camera.main;
        gridObjects = new Dictionary<Vector2, GameObject>();
    }

    private void Update()
    {
        // if (!Input.GetMouseButtonDown(0) || !cam)
        //     return;
        //
        // Vector3 worldMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        // SetTile(defaultTile, worldMousePos.x - offsetX, worldMousePos.y - offsetY);
    }

    public void SetTileSize(float newTileSize) // if changed while running will cause issues
    {
        tileSize = newTileSize;
    }

    public void SetTile(GameObject tileToPlace, float x, float y)
    {
        var coords = new Vector2(x * tileSize, y * tileSize);
        float posX = x * tileSize;
        float posY = y * tileSize;

        if (gridObjects.TryGetValue(coords, out GameObject gridObj))
        {
            Destroy(gridObj);
            gridObjects.Remove(coords);
        }
        
        if (!tileToPlace)
            return;

        GameObject gridObject = Instantiate(tileToPlace, new Vector3(posX, posY, 0), Quaternion.identity, mapParent.transform);
        gridObject.name = "Tile: " + coords;
        gridObject.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        gridObjects.Add(coords, gridObject);

    }
}
