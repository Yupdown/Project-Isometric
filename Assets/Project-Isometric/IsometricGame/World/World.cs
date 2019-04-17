﻿using System.Collections.Generic;
using UnityEngine;
using Isometric.Interface;
using Isometric.Items;

public class World
{
    private IsometricGame _game;
    public IsometricGame game
    {
        get
        { return _game; }
    }

    private WorldCamera _worldCamera;
    public WorldCamera worldCamera
    {
        get
        { return _worldCamera; }
    }
    public WorldMicrophone worldMicrophone
    {
        get
        { return _worldCamera.worldMicrophone; }
    }

    private float _worldTime;

    private ChunkGenerator _chunkGenerator;
    private LinkedList<Chunk> _chunks;
    private Dictionary<int, Chunk> _chunkMap;

    private LinkedList<CosmeticRenderer> _cosmeticDrawables;

    private List<ITarget> _targets;
    public List<ITarget> targets
    {
        get
        { return _targets; }
    }

    private int _seed;

    public Player player { get; private set; }

    private CameraHUDMenu _cameraHUD;
    public CameraHUDMenu cameraHUD
    {
        get
        { return _cameraHUD; }
    }

    private const float loadChunkRange = 30f;
    private const float unloadChunkRange = 50f;

    private WorldProfiler worldProfiler;

    public World(IsometricGame game)
    {
        _game = game;

        _worldCamera = new WorldCamera(this);

        _worldTime = 0f;

        _chunkGenerator = new ChunkGenerator(this);
        _chunks = new LinkedList<Chunk>();
        _chunkMap = new Dictionary<int, Chunk>(256);

        _cosmeticDrawables = new LinkedList<CosmeticRenderer>();

        _targets = new List<ITarget>();

        player = new Player();
        RequestLoadChunk(new Vector2Int(0, 0));

        _cameraHUD = new CameraHUDMenu(_game, _worldCamera);
        game.AddSubLoopFlow(_cameraHUD);

        // _cameraHUD.Speech(player, "W A S D : Move the character\nSpace : Jump the character\nQ, E : Move the camera\nEsc : Exit the game");

        worldProfiler = new WorldProfiler(this);

        Shader.SetGlobalVector("_Epicenter", new Vector4(0f, 0f, -10f));
    }

    public void Update(float deltaTime)
    {
        _worldTime += deltaTime;

        if (Input.GetKeyDown(KeyCode.F3))
            worldProfiler.updateProfiler.SwitchProfiler();

        worldProfiler.updateProfiler.StartMeasureTime();

        Vector2 playerCoordinate = new Vector2(player.worldPosition.x, player.worldPosition.z);

        int xMin = Mathf.FloorToInt((playerCoordinate.x - loadChunkRange) / Chunk.Length);
        int xMax = Mathf.FloorToInt((playerCoordinate.x + loadChunkRange) / Chunk.Length);
        int yMin = Mathf.FloorToInt((playerCoordinate.y - loadChunkRange) / Chunk.Length);
        int yMax = Mathf.FloorToInt((playerCoordinate.y + loadChunkRange) / Chunk.Length);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                if (((new Vector2(x + 0.5f, y + 0.5f) * Chunk.Length) - playerCoordinate).sqrMagnitude < loadChunkRange * loadChunkRange)
                    RequestLoadChunk(new Vector2Int(x, y));
            }
        }

        for (LinkedListNode<Chunk> node = _chunks.First; node != null; node = node.Next)
        {
            Chunk chunk = node.Value;

            if (chunk.state == ChunkState.Loaded)
            {
                chunk.Update(deltaTime);

                Vector2 chunkDelta = new Vector2(chunk.coordination.x + 0.5f, chunk.coordination.y + 0.5f) * Chunk.Length - playerCoordinate;
                if (chunkDelta.x * chunkDelta.x + chunkDelta.y * chunkDelta.y > unloadChunkRange * unloadChunkRange)
                    chunk.UnloadChunk();
            }
        }

        Queue<Chunk> loadedChunkQueue = _chunkGenerator.GetLoadedChunkQueue();

        while (loadedChunkQueue.Count > 0)
        {
            lock (loadedChunkQueue)
            {
                Chunk loadedChunk = loadedChunkQueue.Dequeue();
                OnChunkGenerated(loadedChunk);
            }
        }

        DrawablesUpdate(deltaTime);

        worldProfiler.updateProfiler.MeasureTime(UpdateProfilerType.ChunkUpdate);

        Shader.SetGlobalFloat("_WorldTime", _worldTime);
        worldCamera.GraphicUpdate(deltaTime);
        worldProfiler.updateProfiler.MeasureTime(UpdateProfilerType.RenderUpdate);
    }

    private void DrawablesUpdate(float deltaTime)
    {
        for (var iterator = _cosmeticDrawables.First; iterator != null; iterator = iterator.Next)
        {
            CosmeticRenderer cosmeticDrawable = iterator.Value;
            
            if (cosmeticDrawable.world == this)
                cosmeticDrawable.Update(deltaTime);
            else
                _cosmeticDrawables.Remove(iterator);
        }
    }

    public void OnTerminate()
    {
        worldCamera.CleanUp();
        worldProfiler.updateProfiler.CleanUp();
    }


    public void RequestLoadChunk(Vector2Int coordination)
    {
        Chunk chunk;
        _chunkMap.TryGetValue((coordination.x << 16) + coordination.y, out chunk);

        if (chunk == null)
        {
            chunk = new Chunk(this, coordination);

            _chunks.AddLast(chunk);
            _chunkMap.Add((coordination.x << 16) + coordination.y, chunk);
            _chunkGenerator.RequestGenerateChunk(chunk);
        }

        else if (chunk.state == ChunkState.Unloaded)
            chunk.LoadChunk();
    }

    public void OnChunkGenerated(Chunk chunk)
    {
        if (chunk.coordination == new Vector2Int(0, 0))
        {
            SpawnEntity(player, new Vector3(1f, GetSurface(new Vector2(1f, 1f)), 1f));

            SpawnEntity(new EntityBoss(), new Vector3(8f, 16f, 8f));

            for (int i = 0; i < 10; i++)
            {
                Vector2 position = Vector2.one * 10f;
                SpawnEntity(new EntityPpyongppyong(), new Vector3(position.x, 30f, position.y));
            }
        }

        int spawnCount = (int)RXRandom.Range(0f, 1.1f);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 position = (chunk.coordination * Chunk.Length) + new Vector2(RXRandom.Range(0f, 16f), RXRandom.Range(0f, 16f));
            SpawnEntity(new EntityPpyongppyong(), new Vector3(position.x, GetSurface(new Vector3(position.x, 0f, position.y)), position.y));
        }

        spawnCount = (int)RXRandom.Range(0f, 1.05f);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 position = (chunk.coordination * Chunk.Length) + new Vector2(RXRandom.Range(0f, 16f), RXRandom.Range(0f, 16f));
            SpawnEntity(new EntityDipper(), new Vector3(position.x, GetSurface(new Vector3(position.x, 0f, position.y)), position.y));
        }

        Vector2Int[] nearbyCoordinations = new Vector2Int[]
        {
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0)
        };

        for (int index = 0; index < nearbyCoordinations.Length; index++)
        {
            Chunk nearbyChunk = GetChunkByCoordinate(chunk.coordination + nearbyCoordinations[index]);

            if (nearbyChunk != null)
            {
                chunk.SetNearbyChunk((index + 4) % 8, nearbyChunk);
                nearbyChunk.SetNearbyChunk(index, chunk);
            }
        }
    }

    public Chunk GetChunkByCoordinate(Vector2Int chunkPosition)
    {
        try
        { return _chunkMap[(chunkPosition.x << 16) + chunkPosition.y]; }
        catch
        { return null; }
    }

    public ChunkRenderer GetChunkGraphicsAtPosition(Vector3Int tilePosition)
    {
        Chunk chunk = GetChunkByCoordinate(Chunk.ToChunkCoordinate(tilePosition));

        if (chunk != null)
            return chunk.chunkRenderer;
        return null;
    }

    public Tile GetTileAtPosition(Vector3Int tilePosition)
    {
        if (tilePosition.y < 0 || tilePosition.y >= Chunk.Height)
            return null;

        Chunk chunk = GetChunkByCoordinate(Chunk.ToChunkCoordinate(tilePosition));

        if (chunk != null)
            return chunk[new Vector3Int(
                tilePosition.x & 0x0F,
                tilePosition.y,
                tilePosition.z & 0x0F)];
        return null;
    }

    public float GetSurface(Vector3 worldPosition)
    {
        Chunk chunk = GetChunkByCoordinate(Chunk.ToChunkCoordinate(worldPosition));

        if (chunk != null)
            return chunk.GetSurface(worldPosition);

        return 0f;
    }

    public RayTrace RayTraceTile(Vector3 startPosition, Vector3 direction, float distance)
    {
        Vector3 tracePosition = startPosition;
        
        while (true)
        {
            Vector3 intersectionDelta = Vector3.positiveInfinity;
            Vector3 faceDirection = Vector3.zero;

            for (int i = 0; i < 3; i++)
            {
                if (direction[i] == 0f)
                    continue;

                int nextStep = direction[i] > 0f ? Mathf.FloorToInt(tracePosition[i]) + 1 : Mathf.CeilToInt(tracePosition[i]) - 1;
                Vector3 v = direction / direction[i] * (nextStep - tracePosition[i]);

                if (v.sqrMagnitude < intersectionDelta.sqrMagnitude)
                {
                    intersectionDelta = v;

                    switch (i)
                    {
                        case 0:
                            faceDirection = direction[0] < 0f ? Vector3.right : Vector3.left;
                            break;

                        case 1:
                            faceDirection = direction[1] < 0f ? Vector3.up : Vector3.down;
                            break;

                        case 2:
                            faceDirection = direction[2] < 0f ? Vector3.forward : Vector3.back;
                            break;
                    }
                }
            }

            tracePosition += intersectionDelta;

            if (tracePosition.y < 0f || tracePosition.y > Chunk.Height)
                return new RayTrace();

            Vector3Int tilePosition = new Vector3Int();
            for (int i = 0; i < 3; i++)
                tilePosition[i] = direction[i] > 0f ? Mathf.FloorToInt(tracePosition[i]) : Mathf.CeilToInt(tracePosition[i]) - 1;

            Tile tile = GetTileAtPosition(tilePosition);
            if (Tile.GetFullTile(tile))
                return new RayTrace(tracePosition, faceDirection, tilePosition);
        }
    }

    public void SpawnEntity(Entity entity, Vector3 position)
    {
        if (entity.chunk == null)
        {
            Chunk chunk = GetChunkByCoordinate(Chunk.ToChunkCoordinate(position));

            if (chunk != null)
            {
                chunk.AddEntity(entity);
                entity.OnSpawn(chunk, position);

                if (entity is ITarget && entity != player)
                    _targets.Add(entity as ITarget);
            }
        }
        else
            Debug.LogWarning(string.Concat("Entity ", entity.GetType(), " has already spawn."));
    }

    public void OnDespawnEntity(Entity entity)
    {
        if (entity is ITarget && entity != player)
            _targets.Remove(entity as ITarget);
    }

    public void AddCosmeticDrawble(CosmeticRenderer cosmeticDrawable)
    {
        _cosmeticDrawables.AddLast(cosmeticDrawable);
        cosmeticDrawable.OnShow(this);
    }

    public void PlaceBlock(Vector3Int tilePosition, Block newBlock)
    {
        Tile tile = GetTileAtPosition(tilePosition);

        tile.SetBlock(newBlock);
    }

    public void DestroyBlock(Vector3Int tilePosition)
    {
        Tile tile = GetTileAtPosition(tilePosition);

        if (tile == null)
            return;

        Item item = Item.GetItemByKey("block_grass");
        Entity droppedItem = new DroppedItem(new ItemStack(item, 1));
        SpawnEntity(droppedItem, tilePosition + Vector3.one * 0.5f);

        tile.SetBlock(Block.GetBlockByKey("air"));
    }

    public void QuakeAtPosition(Vector3 epicenter)
    {
        Vector4 vector = new Vector4();

        vector.x = epicenter.x;
        vector.y = epicenter.z;
        vector.z = _worldTime;

        Shader.SetGlobalVector("_Epicenter", vector);
    }
}
