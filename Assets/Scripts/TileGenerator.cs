using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [Header("Grid Size")]
    [Min(1)] public int x = 4;
    [Min(1)] public int y = 4;


    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject cityPrefab;
    public GameObject spawnPrefab;
    public GameObject connectorPrefab;


    [Header("Roots")]
    public Transform nodeRoot;
    public Transform connectorRoot;


    [Header("Layout")]
    public Vector2 spacing = new Vector2(2.5f, 1.5f);

    public Vector2 offset = Vector2.zero;


    [Header("Scale")]
    public float farScale = 0.9f;
    public float nearScale = 1.1f;


    private BattleNode[,] nodes;


    public void Generate()
    {
        Validate();

        Clear();

        EnsureRoots();

        InstantiateNodes();

        ConnectNodes();

        GenerateConnectors();
    }


    // =========================================================
    // Instantiate
    // =========================================================

    private void InstantiateNodes()
    {
        int width = x + 2;

        nodes = new BattleNode[width, y];


        for (int row = 0; row < y; row++)
        {
            for (int column = 0; column < width; column++)
            {
                GameObject prefab =
                    GetPrefabForColumn(column);

                GameObject obj =
                    Instantiate(prefab, nodeRoot);


                obj.name =
                    GetNodeName(column, row);


                obj.transform.localPosition =
                    GetNodePosition(column, row);


                float scale =
                    GetRowScale(row);


                obj.transform.localScale =
                    Vector3.one * scale;


                BattleNode node =
                    obj.GetComponent<BattleNode>();


                if (node == null)
                {
                    Debug.LogError(
                        $"{obj.name} has no BattleNode component."
                    );

                    continue;
                }


                node.Initialize(
                    column,
                    row
                );


                nodes[column, row] =
                    node;
            }
        }
    }


    private GameObject GetPrefabForColumn(int column)
    {
        // City column
        if (column == 0)
            return cityPrefab;

        // Spawn column
        if (column == x + 1)
            return spawnPrefab;

        // Normal battle tile
        return tilePrefab;
    }


    private string GetNodeName(
        int column,
        int row
    )
    {
        if (column == 0)
            return $"City_{row}";

        if (column == x + 1)
            return $"Spawn_{row}";

        return $"Tile_{column - 1}_{row}";
    }


    // =========================================================
    // Connections
    // =========================================================

    private void ConnectNodes()
    {
        int width = x + 2;


        for (int row = 0; row < y; row++)
        {
            for (int column = 0; column < width; column++)
            {
                BattleNode current =
                    nodes[column, row];

                if (current == null)
                    continue;


                current.ClearConnections();


                // Left
                if (column > 0)
                {
                    current.AddConnection(
                        nodes[column - 1, row]
                    );
                }


                // Right
                if (column < width - 1)
                {
                    current.AddConnection(
                        nodes[column + 1, row]
                    );
                }


                // Up
                if (row > 0)
                {
                    current.AddConnection(
                        nodes[column, row - 1]
                    );
                }


                // Down
                if (row < y - 1)
                {
                    current.AddConnection(
                        nodes[column, row + 1]
                    );
                }
            }
        }
    }


    // =========================================================
    // Connector Visual
    // =========================================================

    private void GenerateConnectors()
    {
        if (connectorPrefab == null)
            return;


        int width = x + 2;


        for (int row = 0; row < y; row++)
        {
            for (int column = 0; column < width; column++)
            {
                BattleNode current =
                    nodes[column, row];

                if (current == null)
                    continue;


                // สร้างเฉพาะ Right
                // เพื่อไม่ให้เส้นซ้ำ
                if (column < width - 1)
                {
                    CreateConnector(
                        current.transform,
                        nodes[column + 1, row].transform
                    );
                }


                // สร้างเฉพาะ Down
                // เพื่อไม่ให้เส้นซ้ำ
                if (row < y - 1)
                {
                    CreateConnector(
                        current.transform,
                        nodes[column, row + 1].transform
                    );
                }
            }
        }
    }


    private void CreateConnector(
        Transform from,
        Transform to
    )
    {
        if (from == null || to == null)
            return;


        Vector3 start =
            from.position;

        Vector3 end =
            to.position;


        Vector3 direction =
            end - start;


        float distance =
            direction.magnitude;


        Vector3 center =
            (start + end) * 0.5f;


        GameObject connector =
            Instantiate(
                connectorPrefab,
                center,
                Quaternion.identity,
                connectorRoot
            );


        connector.name =
            $"Connector_{from.name}_{to.name}";


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;


        connector.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        SpriteRenderer sr =
            connector.GetComponent<SpriteRenderer>();


        if (sr == null)
            return;


        if (
            sr.drawMode == SpriteDrawMode.Sliced ||
            sr.drawMode == SpriteDrawMode.Tiled
        )
        {
            Vector2 size =
                sr.size;

            size.x =
                distance;

            sr.size =
                size;
        }
        else
        {
            Vector3 scale =
                connector.transform.localScale;


            float spriteWidth =
                sr.sprite != null
                    ? sr.sprite.bounds.size.x
                    : 1f;


            if (spriteWidth > 0f)
            {
                scale.x =
                    distance / spriteWidth;
            }


            connector.transform.localScale =
                scale;
        }
    }


    // =========================================================
    // Position
    // =========================================================

    private Vector3 GetNodePosition(
        int column,
        int row
    )
    {
        int width = x + 2;


        float rowScale =
            GetRowScale(row);


        float centerColumn =
            (width - 1) * 0.5f;


        float scaledSpacingX =
            spacing.x * rowScale;


        float posX =
            offset.x +
            (column - centerColumn)
            * scaledSpacingX;


        float centerRow =
            (y - 1) * 0.5f;


        float posY =
            offset.y -
            (row - centerRow)
            * spacing.y;


        return new Vector3(
            posX,
            posY,
            0f
        );
    }


    public float GetRowScale(int row)
    {
        if (y <= 1)
            return 1f;


        float t =
            row / (float)(y - 1);


        return Mathf.Lerp(
            farScale,
            nearScale,
            t
        );
    }


    // =========================================================
    // Validation / Root
    // =========================================================

    private void Validate()
    {
        if (tilePrefab == null)
            Debug.LogError("Tile prefab missing.");

        if (cityPrefab == null)
            Debug.LogError("City prefab missing.");

        if (spawnPrefab == null)
            Debug.LogError("Spawn prefab missing.");
    }


    private void EnsureRoots()
    {
        if (nodeRoot == null)
        {
            GameObject obj =
                new GameObject("Nodes");

            obj.transform.SetParent(
                transform,
                false
            );

            nodeRoot =
                obj.transform;
        }


        if (connectorRoot == null)
        {
            GameObject obj =
                new GameObject("Connectors");

            obj.transform.SetParent(
                transform,
                false
            );

            connectorRoot =
                obj.transform;
        }
    }


    // =========================================================
    // Clear
    // =========================================================

    public void Clear()
    {
        ClearChildren(nodeRoot);
        ClearChildren(connectorRoot);

        nodes = null;
    }


    private void ClearChildren(
        Transform root
    )
    {
        if (root == null)
            return;


        for (
            int i = root.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                root.GetChild(i).gameObject;


            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }
}