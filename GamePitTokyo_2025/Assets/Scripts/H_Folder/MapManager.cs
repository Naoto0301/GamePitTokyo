using System.Collections.Generic;
using UnityEngine;

public class TerrariaMapGen : MonoBehaviour
{
    [Header("=== Map Settings ===")]
    public int width = 200;
    public int height = 120;
    public float noiseScale = 0.05f;

    public Vector2 mapStart = new Vector2(-8.0f, 0);

    [Header("=== Surface Control ===")]
    public int p1_height = 20;  // 左の高さ
    public int p2_height = 40;  // 右の高さ

    [Header("=== Cave Settings ===")]
    public int caveSeed = 0;
    public float caveFillPercent = 0.45f;
    public int smoothCount = 5;

    [Header("=== Prefabs ===")]
    public GameObject blockPrefab;

    private int[,] map;              // 0 = 空洞、1 = 地形
    private GameObject[,] blocks;    // 生成したGameObject格納


    void Start()
    {
        Generate();
    }


    // =============================================================
    //  メイン処理
    // =============================================================
    public void Generate()
    {
        map = new int[width, height];
        blocks = new GameObject[width, height];

        GenerateGroundShape();   // 地形ライン
        FillTerrainBelowLine();  // 下を埋める
        GenerateCave();          // 洞窟掘り

        KeepOnlyConnectedToSurface();

        BuildGameObjects();      // 表面だけ当たり判定 ON
    }


    // =============================================================
    //  地形の上辺ライン生成（2点補完 + Perlin）
    // =============================================================
    void GenerateGroundShape()
    {
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);  // 0 → 1
            float baseHeight = Mathf.Lerp(p1_height, p2_height, t);

            float perlin = Mathf.PerlinNoise(x * noiseScale, 0) * 10f;

            int h = Mathf.RoundToInt(baseHeight + perlin);

            h = Mathf.Clamp(h, 0, height - 1);

            map[x, h] = 1;  // 地形の上端
        }
    }


    // =============================================================
    //  地形ラインより下を全部埋める
    // =============================================================
    void FillTerrainBelowLine()
    {
        for (int x = 0; x < width; x++)
        {
            int top = -1;
            for (int y = height - 1; y >= 0; y--)
            {
                if (map[x, y] == 1)
                {
                    top = y;
                    break;
                }
            }

            if (top == -1) continue;

            for (int y = top - 1; y >= 0; y--)
            {
                map[x, y] = 1;
            }
        }
    }


    // =============================================================
    //  洞窟生成（セルオートマトン）
    // =============================================================
    void GenerateCave()
    {
        System.Random rand = (caveSeed == 0)
            ? new System.Random()
            : new System.Random(caveSeed);

        // 最初にランダムな空洞生成
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (rand.NextDouble() < caveFillPercent)
                    map[x, y] = 0;
            }
        }

        // スムージング複数回
        for (int i = 0; i < smoothCount; i++)
            SmoothMap();
    }

    void SmoothMap()
    {
        int[,] newMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int walls = GetSurroundWallCount(x, y);

                if (walls > 4)
                    newMap[x, y] = 1;
                else if (walls < 4)
                    newMap[x, y] = 0;
                else
                    newMap[x, y] = map[x, y];
            }
        }

        map = newMap;
    }

    int GetSurroundWallCount(int x, int y)
    {
        int cnt = 0;
        for (int nx = x - 1; nx <= x + 1; nx++)
        {
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx == x && ny == y) continue;
                if (map[nx, ny] == 1) cnt++;
            }
        }
        return cnt;
    }

    int GetSurfaceY(int x)
    {
        for (int y = height - 1; y >= 0; y--)
        {
            if (map[x, y] == 1)
                return y;
        }
        return -1;
    }


    void KeepOnlyConnectedToSurface()
    {
        bool[,] visited = new bool[width, height];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        // ---- Surface(表面)の下から空洞探索開始 ----
        for (int x = 0; x < width; x++)
        {
            int y = GetSurfaceY(x);
            if (y == -1) continue;

            // 地表の真下が空洞なら BFS 開始
            if (y - 1 >= 0 && map[x, y - 1] == 0)
            {
                q.Enqueue(new Vector2Int(x, y - 1));
                visited[x, y - 1] = true;
            }
        }

        // ---- BFSで空洞をマーク ----
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var p = q.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nx = p.x + dx[i];
                int ny = p.y + dy[i];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                // 空洞で、まだ訪問していない
                if (map[nx, ny] == 0 && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        // ---- ここに到達しなかった空洞を全部埋める ----
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0 && !visited[x, y])
                {
                    map[x, y] = 1;   // 地形に埋め戻す
                }
            }
        }
    }



    // =============================================================
    //  GameObject 生成（表面だけ isHit = true）
    // =============================================================
    void BuildGameObjects()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    Vector3 pos = new Vector3(
                        mapStart.x + x,
                        mapStart.y + y,
                        0
                    );

                    var obj = Instantiate(blockPrefab, pos, Quaternion.identity);
                    blocks[x, y] = obj;

                    var tile = obj.GetComponent<TileBaseEX>();

                    // ---- 表面だけ当たり判定 ----
                    if (IsSurface(x, y))
                        tile.isHit = true;
                    else
                        tile.isHit = false;
                }
            }
        }
    }

    bool IsSurface(int x, int y)
    {
        if (y + 1 >= height) return true;
        return map[x, y + 1] == 0;
    }
}

