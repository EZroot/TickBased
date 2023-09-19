using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using TickBased.Utils;
using UnityEngine;
using Logger = TickBased.Logger.Logger;
using Random = UnityEngine.Random;

namespace FearProj.ServiceLocator
{
    public class GridManager : MonoBehaviour, IServiceGridManager
    {
        public enum TileState
        {
            Empty,
            Obstacle,
            Object
        }

        private const int DISTANCE_FROM_PLAYER_TO_RENDER_CHUNK = CHUNK_SIZE + CHUNK_SIZE / 4;
        private const int CHUNK_SIZE = 32; //has to be divisible by 4

        [SerializeField] private GameObject _chunkPrefab;
        [SerializeField] private GameObject _mouseGridOutlinePrefab;

        [Header("- Grid Settings - Must be divisible by 4")] [SerializeField]
        private int _gridSize = 64; //divisible by 4

        [SerializeField] private int _tileSize = 2;
        [SerializeField] private int _seed = 6969;

        [Header("- Generation Settings -")] [Range(0f, 1f)] [SerializeField]
        private float _perlinNoiseLayer1Scale = 0.1f;

        [Range(0f, 1f)] [SerializeField] private float _perlinNoiseLayer2Scale = 0.5f;
        [Range(0f, 2f)] [SerializeField] private float _distanceFromEdgeToGenerate = 1f;

        private int _gridSeed = -1;
        private MouseGridOutline _mouseGridOutline;
        private List<GridChunkMesh> _gridChunks;
        private GridData _gridData;
        private Tile[,] _tiles;
        private Coroutine _rasterizeCoroutine;
        public int GridSeed => _gridSeed;
        public int GridSize => _gridSize;
        public int GridTileSize => _tileSize;

        public GridData Grid => _gridData;
        public int TileSize => _gridData.TileSize;

        public event Action OnPreGridGeneration;
        public event Action OnPostGridGeneration;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
                var player = creatureManager.Player;
                int centerX = player.GridCoordinates.X; // -1 to leave space for the grid
                int centerY = player.GridCoordinates.Y + 4; // -1 to leave space for the grid

                for (int x = centerX - 1; x <= centerX + 1; x++)
                {
                    for (int y = centerY - 1; y <= centerY + 1; y++)
                    {
                        SetTileData(x, y, new Tile(TileState.Obstacle, null));
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                var settings = ServiceLocator.Get<IServiceGameManager>().GameSettings.DataSettings;
                var map = FileUtils.GetAllFilesWithExtension(settings.AssetPath, ".png");

                if (map.Length == 0)
                {
                    Logger.LogError("Failed to load images. Map rasturization failed.", "GridManager");
                }
                else
                {
                    RasterizeImage(map[0]);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                var pos = GetGridPositionFromMouse(Camera.allCameras[0]);
                SetTileData(pos.X, pos.Y, new Tile(TileState.Obstacle, null));
            }
        }

        private void FixedUpdate()
        {
            if (_mouseGridOutline != null)
            {
                var pos = GetGridPositionFromMouse(Camera.allCameras[0]);
                var vecpos = GridToWorld(pos);
                _mouseGridOutline.SetPosition(vecpos);
            }
        }

        public void InitializeGridBySeed(int seed, GridData grid)
        {
            StartCoroutine(LoadGrid(seed, grid));
        }

        private IEnumerator LoadGrid(int seed, GridData grid)
        {
            OnPreGridGeneration?.Invoke();
            yield return null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _gridSeed = seed;
            Random.InitState(seed);
            InitializeGrid(grid);
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            TickBased.Logger.Logger.LogWarning($"Diagnostics: InitializeGridBySeed Executed: {elapsedMilliseconds}ms",
                "GridManager");
            
            var mouseGridOutline = Instantiate(_mouseGridOutlinePrefab, transform);
            _mouseGridOutline = mouseGridOutline.GetComponent<MouseGridOutline>();
            _mouseGridOutline.GenerateMouseGrid(new GridCoordinate(0, 0), _tileSize, new Color(0, 0, 0, 0.5f));
            
            yield return new WaitForSeconds(1f);
            OnPostGridGeneration?.Invoke();
        }

        public void InitializeGrid(GridData grid)
        {
            _gridData = grid;
            _tiles = new Tile[_gridData.Width, _gridData.Height];

            // Initialize tiles as needed
            for (int x = 0; x < _gridData.Width; x++)
            {
                for (int y = 0; y < _gridData.Height; y++)
                {
                    _tiles[x, y] = new Tile(TileState.Empty);
                }
            }

            GenerateMesh();

            for (var axp = 0; axp < 50; axp++)
            {
                SetTileColor(axp + 10, 1, Color.red);

                SetTileColor(axp, 2, Color.green);
                SetTileColor(axp, 3, Color.blue);
            }

            var tileSize = grid.TileSize;
            transform.position = new Vector3(-tileSize / 2, -tileSize / 2, 1f);

            var lightManager = ServiceLocator.Get<IServiceLightManager>();
            lightManager.GenerateLightMesh(grid);

            var tickManager = ServiceLocator.Get<IServiceTickManager>();
            tickManager.OnCommandExecuted += UpdateChunkVisibility;
        }

        public void GenerateMesh()
        {
            CreateChunks();

            PerlinNoiseGenerator perlinGenerator = new PerlinNoiseGenerator(
                _perlinNoiseLayer1Scale,
                _perlinNoiseLayer2Scale,
                new Vector2(_gridData.Width / 2, _gridData.Height / 2),
                _distanceFromEdgeToGenerate
            );

            foreach (var chunk in _gridChunks)
            {
                // Determine chunk bounds
                int startX = chunk.ChunkPosition.X;
                int startY = chunk.ChunkPosition.Y;
                int endX = startX + CHUNK_SIZE;
                int endY = startY + CHUNK_SIZE;

                // Initialize mesh data for this chunk
                Vector3[] vertices = new Vector3[4 * CHUNK_SIZE * CHUNK_SIZE];
                int[] triangles = new int[6 * CHUNK_SIZE * CHUNK_SIZE];
                Color[] colors = new Color[4 * CHUNK_SIZE * CHUNK_SIZE];

                int vertexIndex = 0;
                int triangleIndex = 0;

                // Generate mesh for this chunk
                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        if (x >= _gridData.Width || y >= _gridData.Height)
                            continue;

                        TickBased.Utils.MeshUtils.CreateSquare(x, y, _gridData.TileSize, vertices, triangles, colors,
                            ref vertexIndex,
                            ref triangleIndex);

                        float noiseValue = perlinGenerator.GetNoiseValue(x, y);
                        Color perlinColor =
                            perlinGenerator
                                .GetTerrainColor(noiseValue); //new Color(noiseValue, noiseValue, noiseValue);
                        for (int i = vertexIndex - 4; i < vertexIndex; i++)
                        {
                            colors[i] = perlinColor;
                        }

                        // Color randomColor = new Color(Random.value, Random.value, Random.value);
                        // for (int i = vertexIndex - 4; i < vertexIndex; i++)
                        // {
                        //     colors[i] = randomColor;
                        // }
                    }
                }

                // Apply mesh data to chunk's MeshFilter
                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.colors = colors;
                chunk.MeshFilter.mesh = mesh;
            }
        }

        private void CreateChunks()
        {
            if (_gridChunks != null && _gridChunks.Count > 0)
            {
                for (int i = 0; i < _gridChunks.Count; i++)
                {
                    Destroy(_gridChunks[i].gameObject);
                }

                _gridChunks.Clear();
            }

            _gridChunks = new List<GridChunkMesh>();
            for (int x = 0; x < _gridData.Width; x += CHUNK_SIZE)
            {
                for (int y = 0; y < _gridData.Height; y += CHUNK_SIZE)
                {
                    GridCoordinate chunkPosition = new GridCoordinate(x, y);

                    var chunkObject = Instantiate(_chunkPrefab, transform);
                    var chunkMesh = chunkObject.GetComponent<GridChunkMesh>();
                    chunkMesh.ChunkPosition = chunkPosition;
                    _gridChunks.Add(chunkMesh);
                }
            }
        }

        void UpdateChunkVisibility()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
            foreach (var chunk in _gridChunks)
            {
                var chunkPosition = chunk.ChunkPosition + CHUNK_SIZE / 2;
                var playerPosition = creatureManager.Player.TileObject.GridCoordinates;

                float distance = TickBased.Utils.MathUtils.CalculateDistance(playerPosition.X, playerPosition.Y,
                    chunkPosition.X,
                    chunkPosition.Y);
//                TickBased.Logger.Logger.Log(
//                    $"ChunKPos {chunkPosition.X},{chunkPosition.Y} PlayerPos {playerPosition.X},{playerPosition.Y} - dist {distance}");
                if (distance > DISTANCE_FROM_PLAYER_TO_RENDER_CHUNK)
                {
                    chunk.gameObject.SetActive(false);
                }
                else
                {
                    chunk.gameObject.SetActive(true);
                }
            }

            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            TickBased.Logger.Logger.LogWarning($"Diagnostics: UpdateChunkVisibility Executed: {elapsedMilliseconds}ms",
                "GridManager");
        }

        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < _gridData.Width && y >= 0 && y < _gridData.Height)
            {
                return _tiles[x, y];
            }

            TickBased.Logger.Logger.LogError("Coordinates are out of grid bounds.", "GridManager");
            return new Tile();
        }

        public void SetTileData(int x, int y, Tile tile)
        {
            if (x >= 0 && x < _gridData.Width && y >= 0 && y < _gridData.Height)
            {
                _tiles[x, y] = tile;
            }
            else
            {
                TickBased.Logger.Logger.LogError("Coordinates are out of grid bounds.", "GridManager");
            }


            for (var i = 0; i < _gridData.Width; i++)
            {
                for (var k = 0; k < _gridData.Height; k++)
                {
                    if (_tiles[i, k].ObjectOnTile == null)
                        continue;

                    var t = _tiles[i, k];
                    //TickBased.Logger.Logger.Log($"Tile: [{i},{k}] - {t.State} - {t.ObjectOnTile != null}", "GridManager");
                }
            }
        }


        public void RasterizeImage(string fileName)
        {
            if (_rasterizeCoroutine == null)
                _rasterizeCoroutine = StartCoroutine(RasterizeImageCoroutine(fileName));
        }

        public IEnumerator RasterizeImageCoroutine(string fileName)
        {
            Logger.Log($"Trying to rasterize {fileName}", "GridManager");
            var loadManager = ServiceLocator.Get<IServiceUIManager>();
            loadManager.ShowLoadingScreen($"Rasterizing 0/0");
            yield return null;
            var colorDataTask = FileUtils.LoadImageToColorArray(fileName).GetAwaiter();
            while (!colorDataTask.IsCompleted) yield return null;
            var colorData = colorDataTask.GetResult();
            if (colorData != null)
            {
                int width = Mathf.FloorToInt(Mathf.Sqrt(colorData.Length)); // Assuming the image is square
                int height = width; // Assuming the image is square

                var loadingResultFileSize = width * height; //3 is bit depth
                var loadingCounter = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int index = y * width + x;
                        SetTileColor(x, y, colorData[index]);
                        loadingCounter ++;

                        if (loadingCounter % 256 == 0)
                        {
                            loadManager.SetLoadingScreenText($"Rasterizing {loadingCounter}/{loadingResultFileSize} tiles");
                            yield return null;
                        }
                    }
                }
            }
            else
            {
                Logger.LogError("RasterizeImage: Failed to load color data", "GridManager");
            }

            yield return null;
            loadManager.HideLoadingScreen();
            _rasterizeCoroutine = null;
        }

        public void SetTileColor(int x, int y, Color color)
        {
            int chunkX = x / CHUNK_SIZE;
            int chunkY = y / CHUNK_SIZE;
            int numChunksInRow = _gridData.Width / CHUNK_SIZE;
            int numChunksInColumn = _gridData.Height / CHUNK_SIZE;
            int chunkIndex = chunkX * numChunksInRow + chunkY;
            Logger.Log(
                $"SetTileColor: ChunkX {chunkX} ChunkY {chunkY} numChunksInRow {numChunksInRow} numChunksInColumn {numChunksInColumn} chunkIndex {chunkIndex}",
                "GridManager");

            int localX = x % CHUNK_SIZE;
            int localY = y % CHUNK_SIZE;

            int vertexIndex = (localX * CHUNK_SIZE + localY) * 4;
            if (chunkIndex >= 0 && chunkIndex < _gridChunks.Count)
            {
                GridChunkMesh chunk = _gridChunks[chunkIndex];
                Logger.Log($"SetTileColor: Grabbing Chunk: [{chunk.ChunkPosition.X},{chunk.ChunkPosition.Y}]",
                    "GridManager");
                Mesh mesh = chunk.MeshFilter.mesh;
                Color[] colors = mesh.colors;

                colors[vertexIndex] = color;
                colors[vertexIndex + 1] = color;
                colors[vertexIndex + 2] = color;
                colors[vertexIndex + 3] = color;

                mesh.colors = colors;
            }
            else
            {
                Logger.LogError("SetTileColor: CHUNKS OUT OF BOUNDS", "GridManager");
            }
        }


        public GridCoordinate GetGridPositionFromMouse(Camera camera)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = camera.ScreenToWorldPoint(mousePosition);

            int gridX = Mathf.FloorToInt(worldPosition.x / _tileSize);
            int gridY = Mathf.FloorToInt(worldPosition.y / _tileSize);

            return new GridCoordinate(gridX, gridY);
        }

        public bool TileRaycast(int startX, int startY, int endX, int endY, out Tile blockingTile, int thickness = 1)
        {
            blockingTile = new Tile();

            // Angle of the ray
            float angle = Mathf.Atan2(endY - startY, endX - startX);

            // For each line in the thickness band
            for (int offset = -thickness / 2; offset <= thickness / 2; offset++)
            {
                // Offset the starting and ending points
                int offsetX = Mathf.RoundToInt(offset * Mathf.Sin(angle));
                int offsetY = Mathf.RoundToInt(offset * Mathf.Cos(angle));

                int newStartX = startX + offsetX;
                int newStartY = startY + offsetY;
                int newEndX = endX + offsetX;
                int newEndY = endY + offsetY;

                int dx = Mathf.Abs(newEndX - newStartX);
                int dy = Mathf.Abs(newEndY - newStartY);
                int x = newStartX;
                int y = newStartY;
                int n = 1 + dx + dy;
                int x_inc = (newEndX > newStartX) ? 1 : -1;
                int y_inc = (newEndY > newStartY) ? 1 : -1;
                int error = dx - dy;
                dx *= 2;
                dy *= 2;

                int debugCounter = 0;

                for (; n > 0; --n)
                {
//                     if (debugCounter % 10 == 0)
//                     {
// #if UNITY_EDITOR
//                         Vector3 worldStart = GridToWorld(newStartX, newStartY);
//                         Vector3 worldEnd = GridToWorld(x, y);
//                         Debug.DrawRay(worldStart, worldEnd - worldStart, Color.red, 1f);
// #endif
//                     }

                    debugCounter++;
                    if (x >= 0 && x < _gridData.Width && y >= 0 && y < _gridData.Height)
                    {
                        Tile tile = GetTile(x, y);
                        if (tile.State == TileState.Obstacle)
                        {
                            blockingTile = tile;
                            return true;
                        }
                    }

                    if (error > 0)
                    {
                        x += x_inc;
                        error -= dy;
                    }
                    else
                    {
                        y += y_inc;
                        error += dx;
                    }
                }
            }

            return false;
        }

        public Vector2 GridToWorld(int gridX, int gridY)
        {
            float worldX = gridX * TileSize;
            float worldY = gridY * TileSize;
            return new Vector2(worldX, worldY);
        }

        public Vector2 GridToWorld(GridCoordinate gridCoordinate)
        {
            float worldX = gridCoordinate.X * TileSize;
            float worldY = gridCoordinate.Y * TileSize;
            return new Vector2(worldX, worldY);
        }

        // void OnDrawGizmos()
        // {
        //     var gridWidth = _gridData.Width;
        //     var gridHeight = _gridData.Height;
        //     Gizmos.color = Color.gray;
        //     // for (int x = 0; x <= _gridData.Width; x++)
        //     // {
        //     //     Gizmos.DrawLine(new Vector3(x * TileSize, 0, 0), new Vector3(x * TileSize, gridHeight, 0));
        //     // }
        //     //
        //     // for (int y = 0; y <= _gridData.Height; y++)
        //     // {
        //     //     Gizmos.DrawLine(new Vector3(0, y * TileSize, 0), new Vector3(gridWidth, y * TileSize, 0));
        //     // }
        //
        //     Gizmos.color = Color.black;
        //     for (int x = 0; x < gridWidth; x++)
        //     {
        //         for (int y = 0; y < gridHeight; y++)
        //         {
        //             Gizmos.DrawWireCube(new Vector3(x * TileSize, y * TileSize, 0), new Vector3(TileSize, TileSize, 0));
        //         }
        //     }
        //
        //     Camera camera = Camera.allCameras[0]; // Or your specific camera
        //     GridCoordinate coord = GetGridPositionFromMouse(camera);
        //
        //     Vector3 worldPosition = new Vector3(coord.X * _tileSize, coord.Y * _tileSize, 0);
        //
        //     Gizmos.color = Color.red; // Set the color
        //     Gizmos.DrawCube(worldPosition, new Vector3(_tileSize, _tileSize, 1)); // Draw a cube at the grid position
        // }

        [System.Serializable]
        public struct GridData
        {
            public int Width;
            public int Height;
            public int TileSize;

            public GridData(int width, int height, int tileSize)
            {
                Width = width;
                Height = height;
                TileSize = tileSize;
            }
        }

        [System.Serializable]
        public struct GridCoordinate
        {
            public int X;
            public int Y;

            public GridCoordinate(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static GridCoordinate operator *(GridCoordinate coord, int multiplier)
            {
                return new GridCoordinate(coord.X * multiplier, coord.Y * multiplier);
            }

            public static GridCoordinate operator *(int multiplier, GridCoordinate coord)
            {
                return new GridCoordinate(coord.X * multiplier, coord.Y * multiplier);
            }

            public static GridCoordinate operator +(GridCoordinate coord, int addition)
            {
                return new GridCoordinate(coord.X + addition, coord.Y + addition);
            }

            public static GridCoordinate operator +(int addition, GridCoordinate coord)
            {
                return new GridCoordinate(coord.X + addition, coord.Y + addition);
            }

            public static GridCoordinate operator -(GridCoordinate coord, int addition)
            {
                return new GridCoordinate(coord.X - addition, coord.Y - addition);
            }

            public static GridCoordinate operator -(int addition, GridCoordinate coord)
            {
                return new GridCoordinate(coord.X - addition, coord.Y - addition);
            }
        }

        [System.Serializable]
        public struct Tile
        {
            public TileState State;
            public ITileObject ObjectOnTile;

            public Tile(TileState state, ITileObject objectOnTile = null)
            {
                State = state;
                ObjectOnTile = objectOnTile;
            }
        }
    }
}