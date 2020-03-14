using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid : MonoBehaviour
{
    [SerializeField] int width = 1, height = 1;
    [SerializeField] float tileSize = 1;
    [SerializeField] GameObject defaultTile;
    [SerializeField] float offsetX = 0.5f, offsetY = 0.5f;

    private Dictionary<Vector2Int, GameObject> gridObjects;

    public GameObject mapObject;

    // Start is called before the first frame update
    void Awake()
    {
        gridObjects = new Dictionary<Vector2Int, GameObject>();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            var worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            SetTile(defaultTile, worldMousePos.x - offsetX, worldMousePos.y - offsetY);

            Debug.Log(worldMousePos);
        }
    }

    public void SetTile(GameObject tileToPlace, float X, float Y)
    {
        int gridX = Mathf.RoundToInt(X / tileSize);
        int gridY = Mathf.RoundToInt(Y / tileSize);

        var coords = new Vector2Int(gridX, gridY);
        if(gridObjects.ContainsKey(coords))
        {
            Destroy(gridObjects[coords]);
            gridObjects.Remove(coords);
        }

        GameObject gridObject = Instantiate(tileToPlace, new Vector3(gridX + offsetX * tileSize, gridY + offsetY * tileSize, 0), Quaternion.identity, mapObject.transform);
        gridObject.name = "Tile: " + coords;
        gridObjects.Add(coords, gridObject);
        Debug.Log(X + " X, " + gridX + " gX, " + gridY + " gy, " + Y + " Y, " + coords + " coords");
    }

    private bool ContainsTile(float X, float Y)
    {
        int gridX = Mathf.RoundToInt(X - offsetX / tileSize);
        int gridY = Mathf.RoundToInt(Y - offsetY / tileSize);

        var coords = new Vector2Int(gridX, gridY);
        if (gridObjects.ContainsKey(coords))
        {
            return true;
        }

        return false;
    }

    public GameObject GetTileAtPosition(float X, float Y)
    {
        int gridX = Mathf.RoundToInt(X - offsetX / tileSize);
        int gridY = Mathf.RoundToInt(Y - offsetY / tileSize);

        var coords = new Vector2Int(gridX, gridY);
        if (gridObjects.ContainsKey(coords))
        {
            return gridObjects[coords];
        }

        return null;
    }

    public GameObject[] GetAllTilesInRange(float minX, float minY, float maxX, float maxY) // Finds all blocks that have been placed within a certain range
    {
        List<GameObject> tilesPlaced = new List<GameObject>();

        var rangeX = Mathf.Abs(maxX - minX);
        var rangeY = Mathf.Abs(maxY - minY);

        for (int y = 0; y < rangeY; y++)
        {
            for (int x = 0; x < rangeX; x++)
            {
                int gridX = Mathf.RoundToInt(minX + x / tileSize);
                int gridY = Mathf.RoundToInt(minY + y / tileSize);
                var coords = new Vector2Int(gridX, gridY);

                if (gridObjects.ContainsKey(coords))
                    tilesPlaced.Add(gridObjects[coords]);
            }
        }

        return tilesPlaced.ToArray();
    }
}
