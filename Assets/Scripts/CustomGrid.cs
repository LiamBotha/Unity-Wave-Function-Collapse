using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid : MonoBehaviour
{
    //[SerializeField] int width = 1, height = 1;
    [SerializeField] private float tileSize = 1;
    [SerializeField] private GameObject defaultTile;
    [SerializeField] private float offsetX = 0.5f, offsetY = 0.5f;

    private Dictionary<Vector2Int, GameObject> gridObjects;

    public GameObject mapObject;
    private Camera cam;

    // Start is called before the first frame update
    private void Awake()
    {
        cam = Camera.main;
        gridObjects = new Dictionary<Vector2Int, GameObject>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || !cam)
            return;

        Vector3 worldMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        SetTile(defaultTile, worldMousePos.x - offsetX, worldMousePos.y - offsetY);
    }

    public void SetTile(GameObject tileToPlace, float X, float Y)
    {
        int gridX = Mathf.RoundToInt(X / tileSize);
        int gridY = Mathf.RoundToInt(Y / tileSize);

        var coords = new Vector2Int(gridX, gridY);
        if (gridObjects.ContainsKey(coords))
        {
            Destroy(gridObjects[coords]);
            gridObjects.Remove(coords);
        }

        GameObject gridObject = Instantiate(tileToPlace, new Vector3(gridX + offsetX * tileSize, gridY + offsetY * tileSize, 0), Quaternion.identity,
            mapObject.transform);
        gridObject.name = "Tile: " + coords;
        gridObjects.Add(coords, gridObject);
    }

    private bool ContainsTile(float x, float y)
    {
        int gridX = Mathf.RoundToInt(x - offsetX / tileSize);
        int gridY = Mathf.RoundToInt(y - offsetY / tileSize);

        var coords = new Vector2Int(gridX, gridY);
        return gridObjects.ContainsKey(coords);
    }

    public GameObject GetTileAtPosition(float x, float y)
    {
        int gridX = Mathf.RoundToInt(x - offsetX / tileSize);
        int gridY = Mathf.RoundToInt(y - offsetY / tileSize);

        var coords = new Vector2Int(gridX, gridY);
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
