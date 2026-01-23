using System.Collections.Generic;
using UnityEngine;

#region enums

public enum TileType
{
    Air = 0,
    Solid = 1,
    Cave = 2
}


#endregion

public class MapManager : MonoBehaviour
{
    [Header("=== Map ===")]
    public int width = 200;
    public int height = 120;
    public Vector2 mapStart = new Vector2(-8, 0);
    public float noiseScale = 0.05f;

    [Header("=== Surface ===")]
    public int p1_height = 20;
    public int p2_height = 40;

    [Header("=== Cave Path ===")]
    public int caveCount = 8;
    public int caveRadius = 2;
    public float caveDownBias = 0.35f;
    public float caveTurnRate = 0.3f;

    [Header("=== Cave Rooms ===")]
    public float caveRoomChance = 0.07f;
    public int caveRoomMinRadius = 4;
    public int caveRoomMaxRadius = 7;
    public int caveRoomStartStep = 18;

    [Header("=== Bedrock ===")]
    public int bedrockHeight = 3;
    [Header("=== Side Walls ===")]
    public int sideWallWidth = 2;

    [Header("=== BlockPrefab ===")]
    public GameObject blockPrefab;

    [Header("=== Enemies ===")]
    public GameObject[] groundEnemyPrefabs;
    public int groundEnemyCount = 10;

    public GameObject[] airEnemyPrefabs;
    public int airEnemyCount = 8;

    public SpawnRange groundEnemyRange;
    public SpawnRange airEnemyRange;

    [Header("=== Gimmick ===")]
    public GameObject[] gimmickPrefabs;
    public int gimmickCount = 4;

    public SpawnRange gimmickRange;

    TileType[,] map;

    void Start()
    {
        Generate();
    }

    // =========================================================
    void Generate()
    {
        map = new TileType[width, height];

        GenerateSurface();
        FillBelowSurface();
        GenerateCaves();
        ForceBedrock();
        ForceSideWalls();
        BuildBlocks();


        SpawnGroundEnemies();
        SpawnAirEnemies();
        GimmickEnemies();
    }


    // =========================================================
    void GenerateSurface()
    {
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            float h = Mathf.Lerp(p1_height, p2_height, t);
            h += Mathf.PerlinNoise(x * noiseScale, 0) * 10f;

            int y = Mathf.Clamp(Mathf.RoundToInt(h), 0, height - 1);
            map[x, y] = TileType.Solid;
        }
    }

    void FillBelowSurface()
    {
        for (int x = 0; x < width; x++)
        {
            int top = GetSurfaceY(x);
            for (int y = top - 1; y >= 0; y--)
                map[x, y] = TileType.Solid;
        }
    }

    // =========================================================
    void GenerateCaves()
    {
        System.Random rand = new System.Random();

        for (int i = 0; i < caveCount; i++)
        {
            int x = rand.Next(5, width - 5);
            int y = GetSurfaceY(x) - 1;
            if (y <= 0) continue;

            DigCave(x, y, rand);
        }
    }

    void DigCave(int x, int y, System.Random rand)
    {
        int length = rand.Next(40, 90);
        Vector2 pos = new Vector2(x, y);

        Vector2 dir = new Vector2(
            (float)(rand.NextDouble() * 2 - 1),
            -(float)rand.NextDouble()
        ).normalized;

        for (int i = 0; i < length; i++)
        {
            CarveCircle((int)pos.x, (int)pos.y, caveRadius);

            if (i > caveRoomStartStep && rand.NextDouble() < caveRoomChance)
            {
                int r = rand.Next(caveRoomMinRadius, caveRoomMaxRadius + 1);
                CarveRoom((int)pos.x, (int)pos.y, r);
            }

            Vector2 turn = new Vector2(
                (float)(rand.NextDouble() * 2 - 1),
                (float)(rand.NextDouble() * 2 - 1)
            ) * caveTurnRate;

            dir = (dir + turn + Vector2.down * caveDownBias).normalized;
            pos += dir;

            pos.x = Mathf.Clamp(pos.x, 2, width - 3);
            pos.y = Mathf.Clamp(pos.y, bedrockHeight + 2, height - 3);
        }
    }

    void CarveCircle(int cx, int cy, int r)
    {
        for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
            {
                if (dx * dx + dy * dy > r * r) continue;

                int nx = cx + dx;
                int ny = cy + dy;

                if (!InRange(nx, ny) || ny < bedrockHeight) continue;
                map[nx, ny] = TileType.Cave;
            }
    }

    void ForceSideWalls()
    {
        for (int x = 0; x < sideWallWidth; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = TileType.Solid;

        for (int x = width - sideWallWidth; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = TileType.Solid;
    }
    void CarveRoom(int cx, int cy, int r)
    {
        for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
            {
                float d = dx * dx + dy * dy;
                float n = Mathf.PerlinNoise((cx + dx) * 0.15f, (cy + dy) * 0.15f);
                if (d > r * r * (0.7f + n * 0.6f)) continue;

                int nx = cx + dx;
                int ny = cy + dy;

                if (!InRange(nx, ny) || ny < bedrockHeight) continue;
                map[nx, ny] = TileType.Cave;
            }
    }

    void ForceBedrock()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < bedrockHeight; y++)
                map[x, y] = TileType.Solid;
    }

    // =========================================================
    void BuildBlocks()
    {
        GameObject root = new GameObject("MapRoot");

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != TileType.Solid) continue;

                Vector3 pos = new Vector3(mapStart.x + x, mapStart.y + y, 0);
                var obj = Instantiate(blockPrefab, pos, Quaternion.identity, root.transform);

                var tile = obj.GetComponent<TileBaseEX>();
                tile.exposed = GetExposedDir(x, y);
                tile.corner = GetCornerType(x, y);
                tile.ApplyVisual();

                tile.col.enabled = tile.exposed != ExposedDir.None;

                tile.isHit = tile.exposed.HasFlag(ExposedDir.Up);
            }
    }

    ExposedDir GetExposedDir(int x, int y)
    {
        ExposedDir d = ExposedDir.None;
        if (IsOpen(x, y + 1)) d |= ExposedDir.Up;
        if (IsOpen(x, y - 1)) d |= ExposedDir.Down;
        if (IsOpen(x - 1, y)) d |= ExposedDir.Left;
        if (IsOpen(x + 1, y)) d |= ExposedDir.Right;
        return d;
    }

    CornerType GetCornerType(int x, int y)
    {
        bool up = IsOpen(x, y + 1);
        bool down = IsOpen(x, y - 1);
        bool left = IsOpen(x - 1, y);
        bool right = IsOpen(x + 1, y);

        bool ul = IsOpen(x - 1, y + 1);
        bool ur = IsOpen(x + 1, y + 1);
        bool dl = IsOpen(x - 1, y - 1);
        bool dr = IsOpen(x + 1, y - 1);

        if (up && left && !ul) return CornerType.OuterUpLeft;
        if (up && right && !ur) return CornerType.OuterUpRight;
        if (down && left && !dl) return CornerType.OuterDownLeft;
        if (down && right && !dr) return CornerType.OuterDownRight;

        if (!up && !left && ul) return CornerType.InnerUpLeft;
        if (!up && !right && ur) return CornerType.InnerUpRight;
        if (!down && !left && dl) return CornerType.InnerDownLeft;
        if (!down && !right && dr) return CornerType.InnerDownRight;

        return CornerType.None;
    }

    // =========================================================
    bool IsOpen(int x, int y)
    {
        if (!InRange(x, y)) return true;
        return map[x, y] == TileType.Air || map[x, y] == TileType.Cave;
    }

    bool InRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    int GetSurfaceY(int x)
    {
        for (int y = height - 1; y >= 0; y--)
            if (map[x, y] == TileType.Solid)
                return y;
        return -1;
    }

    void SpawnGroundEnemies()
    {
        System.Random rand = new System.Random();
        int spawned = 0;
        int tryLimit = 5000;

        while (spawned < groundEnemyCount && tryLimit-- > 0)
        {
            int x = rand.Next(groundEnemyRange.left, groundEnemyRange.right + 1);
            int y = rand.Next(groundEnemyRange.bottom, groundEnemyRange.top - 3);

            if (!InSpawnRange(x, y, groundEnemyRange)) continue;
            if (map[x, y] != TileType.Solid) continue;

            if (!IsOpen(x, y + 1)) continue;
            if (!IsOpen(x, y + 2)) continue;
            if (!IsOpen(x, y + 3)) continue;

            GameObject prefab =
                groundEnemyPrefabs[rand.Next(groundEnemyPrefabs.Length)];

            Vector3 pos = new Vector3(
                mapStart.x + x,
                mapStart.y + y + 1,
                0
            );

            Instantiate(prefab, pos, Quaternion.identity);
            spawned++;
        }
    }
    void SpawnAirEnemies()
    {
        System.Random rand = new System.Random();
        int spawned = 0;
        int tryLimit = 5000;

        while (spawned < airEnemyCount && tryLimit-- > 0)
        {
            int x = rand.Next(airEnemyRange.left, airEnemyRange.right + 1);
            int y = rand.Next(airEnemyRange.bottom, airEnemyRange.top + 1);

            if (!InSpawnRange(x, y, airEnemyRange)) continue;
            if (!IsOpen(x, y)) continue;

            GameObject prefab =
                airEnemyPrefabs[rand.Next(airEnemyPrefabs.Length)];

            Vector3 pos = new Vector3(
                mapStart.x + x,
                mapStart.y + y,
                0
            );

            Instantiate(prefab, pos, Quaternion.identity);
            spawned++;
        }
    }


    bool InSpawnRange(int x, int y, SpawnRange r)
    {
        return
            x >= r.left &&
            x <= r.right &&
            y >= r.bottom &&
            y <= r.top;
    }

    void GimmickEnemies()
    {
        System.Random rand = new System.Random();
        int spawned = 0;
        int tryLimit = 5000;

        while (spawned < gimmickCount && tryLimit-- > 0)
        {
            int x = rand.Next(gimmickRange.left, gimmickRange.right + 1);
            int y = rand.Next(gimmickRange.bottom, gimmickRange.top - 1);

            if (!InSpawnRange(x, y, gimmickRange)) continue;
            if (map[x, y] != TileType.Solid) continue;

            if (map[x,y + 1] != TileType.Cave) continue;

            GameObject prefab =
                gimmickPrefabs[rand.Next(gimmickPrefabs.Length)];

            Vector3 pos = new Vector3(
                mapStart.x + x,
                mapStart.y + y + 1,
                0
            );

            Instantiate(prefab, pos, Quaternion.identity);
            spawned++;
        }
    }
}


