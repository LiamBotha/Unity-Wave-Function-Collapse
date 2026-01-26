using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CustomGrid : MonoBehaviour
{
    [SerializeField] public float tileSize = 1;
    [SerializeField] private GameObject defaultTile;
    [SerializeField] private float offsetX = 0.5f, offsetY = 0.5f; // positional offset for grid // breaks algo at 1

    private Dictionary<Vector2, GameObject> gridObjects;
    
    public GameObject mapParent; // parent 'folder' for tiles
    private Camera cam;

    // Start is called before the first frame update
    private void Awake()
    {
        cam = Camera.main;
        gridObjects = new Dictionary<Vector2, GameObject>();
        
        for(int y = 0 ; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                SetTile(defaultTile, x, y);
            }
        }
    }

    private void Update()
    {
        // if (!Input.GetMouseButtonDown(0) || !cam)
        //     return;
        //
        // Vector3 worldMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        // SetTile(defaultTile, worldMousePos.x - offsetX, worldMousePos.y - offsetY);
    }

    public void SetTile(GameObject tileToPlace, float x, float y)
    {
        // use this only for the grid pos not game pos
        float gridX = x * tileSize;
        float gridY = y * tileSize;

        var coords = new Vector2(gridX, gridY);
        float posX = (gridX) + (offsetX * tileSize);
        float posY = (gridY) + (offsetY * tileSize);

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

    private bool ContainsTile(float x, float y)
    {
        int gridX = Mathf.RoundToInt(x - offsetX / tileSize);
        int gridY = Mathf.RoundToInt(y - offsetY / tileSize);

        var coords = new Vector2Int(gridX, gridY);
        return gridObjects.ContainsKey(coords);
    }

    public GameObject GetTileAtPosition(float worldX, float worldY)
    {
        float gridX = worldX - offsetX * tileSize;
        float gridY = worldY - offsetY * tileSize;

        var coords = new Vector2(gridX, gridY);
        return gridObjects.GetValueOrDefault(coords);
    }

    // Finds all blocks that have been placed within a certain range
    public GameObject[] GetAllTilesInRange(float minX, float minY, float maxX, float maxY)
    {
        List<GameObject> tilesPlaced = new();

        float rangeX = Mathf.Abs(maxX - minX);
        float rangeY = Mathf.Abs(maxY - minY);

        for (int y = 0; y < rangeY; y++)
        {
            for (int x = 0; x < rangeX; x++)
            {
                int gridX = Mathf.RoundToInt(minX + x / tileSize);
                int gridY = Mathf.RoundToInt(minY + y / tileSize);
                var coords = new Vector2Int(gridX, gridY);

                if (gridObjects.TryGetValue(coords, out GameObject obj))
                    tilesPlaced.Add(obj);
            }
        }

        return tilesPlaced.ToArray();
    }
}
