using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Logger = TickBased.Logger.Logger;
using Random = UnityEngine.Random;

namespace FearProj.ServiceLocator
{
    public class LightManager : MonoBehaviour, IServiceLightManager
    {
        private const int MAX_LIGHT_RENDER_DISTANCE = 24;
        private const int RADIUS_TO_RENDER_PLAYER_SHADOWS = 32;
        
        private const int DISTANCE_FROM_PLAYER_TO_RENDER_CHUNK = CHUNK_SIZE + CHUNK_SIZE / 4;
        private const int CHUNK_SIZE = 32;

        [SerializeField] private Color _backGroundColor = Color.black;
        [SerializeField] private GameObject _chunkPrefab;

        private List<GridChunkMesh> _lightGridChunks;
        private int _gridWidth;
        private int _gridHeight;
        //private Mesh _mesh;
        private float[,] _alphaAccumulator;
        private Color[,] _colorAccumulator; // New Color accumulator

        private Dictionary<string, LightSource> _lights = new Dictionary<string, LightSource>();

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                int randomX = UnityEngine.Random.Range(0, _gridWidth);
                int randomY = UnityEngine.Random.Range(0, _gridHeight);
                Color randomColor = Color.clear;//new Color(UnityEngine.Random.value * 0.2f, UnityEngine.Random.value * 0.2f,
                    //UnityEngine.Random.value * 0.2f, 0f);

                var playerManager = ServiceLocator.Get<IServiceCreatureManager>();
                var player = playerManager.Player.TileObject;
                AddRadialLight($"debuglight{randomX}{randomY}", player.GridCoordinates.X, player.GridCoordinates.Y + 6, 8, randomColor);
            }
        }

        public void GenerateLightMesh(GridManager.GridData gridData)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _gridWidth = gridData.Width;
            _gridHeight = gridData.Height;
            int tileSize = gridData.TileSize;
            
            _alphaAccumulator = new float[_gridWidth, _gridHeight];
            _colorAccumulator = new Color[_gridWidth, _gridHeight]; // Initialize color accumulator
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _colorAccumulator[x, y] = _backGroundColor; // set default color
                }
            }

            transform.position = new Vector3(-tileSize / 2, -tileSize / 2, -1f);

            CreateLightChunks(tileSize);  // Create chunks
            
            var tickManager = ServiceLocator.Get<IServiceTickManager>();
            tickManager.OnCommandExecuted += UpdateChunkVisibility;
            
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            TickBased.Logger.Logger.LogWarning($"Diagnostics: GenerateLightMesh Executed: {elapsedMilliseconds}ms", "LightManager");
        }
        private void CreateLightChunks(int tileSize)
        {
            // Destroy previous chunks if any exist
            if (_lightGridChunks != null && _lightGridChunks.Count > 0)
            {
                for (int i = 0; i < _lightGridChunks.Count; i++)
                {
                    Destroy(_lightGridChunks[i].gameObject);
                }
                _lightGridChunks.Clear();
            }
    
            _lightGridChunks = new List<GridChunkMesh>();
            for (int x = 0; x < _gridWidth; x += CHUNK_SIZE)
            {
                for (int y = 0; y < _gridHeight; y += CHUNK_SIZE)
                {
                    GridManager.GridCoordinate chunkPosition = new GridManager.GridCoordinate(x, y);

                    var chunkObject = Instantiate(_chunkPrefab, transform);
                    var chunkMesh = chunkObject.GetComponent<GridChunkMesh>();
                    chunkMesh.ChunkPosition = chunkPosition;
                    _lightGridChunks.Add(chunkMesh);
                    
                    Vector3[] vertices = new Vector3[CHUNK_SIZE * CHUNK_SIZE * 4];
                    Vector2[] uvs = new Vector2[CHUNK_SIZE * CHUNK_SIZE * 4];
                    int[] triangles = new int[CHUNK_SIZE * CHUNK_SIZE * 6];
                    Color[] colors = new Color[CHUNK_SIZE * CHUNK_SIZE * 4];
                    
                    int vertexIndex = 0;
                    int triangleIndex = 0;

                    var startX = chunkPosition.X;
                    var startY = chunkPosition.Y;
                    var endX = startX + CHUNK_SIZE;
                    var endY = startY + CHUNK_SIZE;
                    for (int i = startX; i < endX; i++)
                    {
                        for (int k = startY; k < endY; k++)
                        {
                            TickBased.Utils.MeshUtils.CreateSquare(i, k, tileSize, vertices,uvs, triangles, colors, ref vertexIndex,
                                ref triangleIndex);
                        }
                    }

                    var newMesh = new Mesh();
                    newMesh.vertices = vertices;
                    newMesh.triangles = triangles;
                    newMesh.colors = colors;
                    chunkMesh.MeshFilter.mesh = newMesh;
                    
                    chunkObject.SetActive(false);
                }
            }
        }
         void UpdateLights()
        {
            foreach (var chunk in _lightGridChunks)
            {
                if (chunk.isActiveAndEnabled)
                {
//                    Logger.LogError($"Not an error: IS ENABLED -> {chunk.ChunkPosition} ");
                    ApplyAccumulatedLight(chunk);
                }
            }
        }
        
        public void ApplyAccumulatedLight(GridChunkMesh chunkMesh)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int chunkStartX = chunkMesh.ChunkPosition.X;
            int chunkStartY = chunkMesh.ChunkPosition.Y;
            //Resetting alpha
            for (int x = chunkStartX; x < chunkStartX + CHUNK_SIZE; x++)
            {
                for (int y = chunkStartY; y < chunkStartY + CHUNK_SIZE; y++)
                {
                    if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
                    {
                        _alphaAccumulator[x, y] = _backGroundColor.a;
                        _colorAccumulator[x, y] = _backGroundColor;
                    }
                }
            }
            var gridManager = ServiceLocator.Get<IServiceGridManager>();

            var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
            var creature = creatureManager.Player;


            var creatureAsTileObject = creature.TileObject;
            int playerX = creatureAsTileObject.GridCoordinates.X;
            int playerY = creatureAsTileObject.GridCoordinates.Y;
            //Alpha blending + Color blending for all lights within player distance
            foreach (var light in _lights.Values)
            {
                float pdx = playerX - light.X;
                float pdy = playerY - light.Y;
                float distanceToPlayer = Mathf.Sqrt(pdx * pdx + pdy * pdy);

                //make sure our light is within the player distance
                if (distanceToPlayer > MAX_LIGHT_RENDER_DISTANCE)
                {
                    continue; // Skip processing this light
                }

                // For example, for light processing
                int startX = Mathf.Max(chunkStartX, light.X - light.Radius);
                int startY = Mathf.Max(chunkStartY, light.Y - light.Radius);
                int endX = Mathf.Min(chunkStartX + CHUNK_SIZE, light.X + light.Radius);
                int endY = Mathf.Min(chunkStartY + CHUNK_SIZE, light.Y + light.Radius);


                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        float dx = x - light.X;
                        float dy = y - light.Y;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);

                        bool isBlocked = gridManager.TileRaycast(light.X, light.Y, x, y, out var blockingTile);

                        if (!isBlocked && distance < light.Radius)
                        {
                            float influence = 1 - (distance / light.Radius); // how much the light influences this point
                            _colorAccumulator[x, y] += light.LightColor * influence; // Add to color accumulator

                            float
                                alpha = (distance /
                                         light
                                             .Radius); // if its colored we should use -> //Mathf.Pow((distance / light.Radius), 4); // Raised to the power of 2
                            _alphaAccumulator[x, y] = Mathf.Min(_alphaAccumulator[x, y], alpha);
                        }
                    }
                }
            }
            //render player light + shadows
            if (_lights.ContainsKey(creature.UniqueID))
            {
                var light = _lights[creature.UniqueID];

                int startX = Mathf.Max(0, playerX - RADIUS_TO_RENDER_PLAYER_SHADOWS);
                int startY = Mathf.Max(0, playerY - RADIUS_TO_RENDER_PLAYER_SHADOWS);
                int endX = Mathf.Min(_gridWidth, playerX + RADIUS_TO_RENDER_PLAYER_SHADOWS);
                int endY = Mathf.Min(_gridHeight, playerY + RADIUS_TO_RENDER_PLAYER_SHADOWS);

                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        float dx = x - light.X;
                        float dy = y - light.Y;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);

                        bool isBlocked = gridManager.TileRaycast(light.X, light.Y, x, y, out var blockingTile);


                        if (isBlocked)
                        {
                            // Darken tiles behind the obstruction
                            int shadowX = x;
                            int shadowY = y;
                            while (shadowX >= 0 && shadowX < _gridWidth && shadowY >= 0 && shadowY < _gridHeight)
                            {
                                _alphaAccumulator[shadowX, shadowY] = _backGroundColor.a; // Set alpha to maximum (darkest)
                                _colorAccumulator[shadowX, shadowY] = _backGroundColor; // Set color to black

                                int shadowStepX = (dx > 0) ? 1 : (dx < 0) ? -1 : 0;
                                int shadowStepY = (dy > 0) ? 1 : (dy < 0) ? -1 : 0;
                                shadowX += shadowStepX;
                                shadowY += shadowStepY;
                            }
                        }
                        else if (distance < light.Radius)
                        {
                            float influence = 1 - (distance / light.Radius);
                            _colorAccumulator[x, y] += light.LightColor * influence;
                            float alpha = (distance / light.Radius);
                            _alphaAccumulator[x, y] = Mathf.Min(_alphaAccumulator[x, y], alpha);
                        }
                    }
                }
            }
            
            Color[] colors = chunkMesh.MeshFilter.mesh.colors;
            for (int x = chunkStartX; x < chunkStartX + CHUNK_SIZE; x++)
            {
                for (int y = chunkStartY; y < chunkStartY + CHUNK_SIZE; y++)
                {
                    // Use relative coordinates for index calculation
                    int relX = x - chunkStartX;
                    int relY = y - chunkStartY;
                    int index = (relX * CHUNK_SIZE + relY) * 4;

                    if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
                    {
                        Color newColor = _colorAccumulator[x, y];
                        newColor.a = _alphaAccumulator[x, y]; // Keep the alpha separate

                        // Assign colors in reverse order to flip normals
                        colors[index + 3] = newColor;
                        colors[index + 2] = newColor;
                        colors[index + 1] = newColor;
                        colors[index] = newColor;
                    }
                }
            }


            chunkMesh.MeshFilter.mesh.colors = colors;
            
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            TickBased.Logger.Logger.LogWarning($"Diagnostics: ApplyAccumulatedLight UpdateMesh Executed: {elapsedMilliseconds}ms", "LightManager");
        }


        void UpdateChunkVisibility()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var creatureManager = ServiceLocator.Get<IServiceCreatureManager>();
            foreach (var chunk in _lightGridChunks)
            {
                var chunkPosition = chunk.ChunkPosition + CHUNK_SIZE / 2;
                var playerPosition = creatureManager.Player.TileObject.GridCoordinates;

                float distance = TickBased.Utils.MathUtils.CalculateDistance(playerPosition.X, playerPosition.Y, chunkPosition.X,
                    chunkPosition.Y);
                // TickBased.Logger.Logger.Log(
                //     $"ChunKPos {chunkPosition.X},{chunkPosition.Y} PlayerPos {playerPosition.X},{playerPosition.Y} - dist {distance}");
                if (distance > DISTANCE_FROM_PLAYER_TO_RENDER_CHUNK)
                {
                    chunk.gameObject.SetActive(false);
                }
                else
                {
                    chunk.gameObject.SetActive(true);
                }
            }

            UpdateLights();
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            TickBased.Logger.Logger.LogWarning($"Diagnostics: UpdateChunkVisibility Executed: {elapsedMilliseconds}ms", "LightManager");
        }
        
        public void ResetLightSources()
        {
            _lights.Clear();
        }

        public void RemoveLightSource(string id)
        {
            _lights.Remove(id);
        }

        public void UpdateLightSourcePosition(string id, int newX, int newY)
        {
            if (_lights.ContainsKey(id))
            {
                LightSource existingLight = _lights[id];
                existingLight.X = newX;
                existingLight.Y = newY;
                _lights[id] = existingLight;
            }
        }

       
        public void AddRadialLight(string id, int sourceX, int sourceY, int radius, Color lightColor)
        {
            LightSource newLight = new LightSource(sourceX, sourceY, radius, lightColor);
            _lights[id] = newLight;
        }

        public struct LightSource
        {
            public int X;
            public int Y;
            public int Radius;
            public Color LightColor;

            public LightSource(int x, int y, int radius, Color lightColor)
            {
                X = x;
                Y = y;
                Radius = radius;
                LightColor = lightColor;
            }
        }

    }
}