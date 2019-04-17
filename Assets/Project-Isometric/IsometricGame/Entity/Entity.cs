﻿using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : IPositionable
{
    private Chunk _chunk;
    public Chunk chunk
    {
        get
        { return _chunk; }
    }
    public World world
    {
        get
        { return _chunk.world; }
    }
    public WorldCamera worldCamera
    {
        get
        { return _chunk.worldCamera; }
    }
    public IsometricGame game
    {
        get
        { return _chunk.game; }
    }

    private Vector3 _worldPosition;
    public Vector3 worldPosition
    {
        get
        { return _worldPosition; }
        set
        { _worldPosition = value; }
    }

    public Vector3Int tilePosition
    {
        get
        { return Vector3Int.FloorToInt(_worldPosition); }
    }
    
    public Vector3 velocity
    {
        get
        { return _physics.velocity; }
        set
        { _physics.velocity = value; }
    }

    public Vector2 screenPosition
    {
        get
        { return worldCamera.GetScreenPosition(_worldPosition) + worldCamera.worldContainer.GetPosition(); }
    }

    private bool _spawned;
    public bool spawned
    {
        get
        { return _spawned; }
    }

    private float _damagedCooldown;
    public float damagedCooldown
    {
        get
        { return _damagedCooldown; }
        set
        { _damagedCooldown = value; }
    }

    private float _time;
    public float time
    {
        get
        { return _time; }
    }

    protected EntityAABBCollider _collider;
    public EntityAABBCollider collider
    {
        get
        { return _collider; }
    }

    protected EntityPhysics _physics;
    public EntityPhysics physics
    {
        get
        { return _physics; }
    }

    private Shadow shadow;
    protected List<EntityPart> entityParts;

    private FLabel debugLabel;

    public Entity(float shadowScale)
    {
        _chunk = null;

        _worldPosition = Vector3.zero;

        _spawned = false;
        _damagedCooldown = 0f;

        if (shadowScale > 0f)
            shadow = new Shadow(this, shadowScale);

        entityParts = new List<EntityPart>();
    }

    public virtual void OnSpawn(Chunk chunk, Vector3 position)
    {
        _spawned = true;
        _time = 0f;

        OnOtherChunk(chunk);

        worldPosition = position;

        if (shadow != null)
            world.AddCosmeticDrawble(shadow);

        for (int index = 0; index < entityParts.Count; index++)
            world.AddCosmeticDrawble(entityParts[index]);

        if (IsometricMain.doesDebugging)
        {
            debugLabel = new FLabel("font", "DEBUG");
            debugLabel.scale = 0.5f;
            debugLabel.color = Color.red;
            debugLabel.alignment = FLabelAlignment.Left;
            world.worldCamera.AddDebugRenderer(debugLabel);
        }
    }

    public virtual void OnDespawn()
    {
        _chunk = null;

        if (shadow != null)
            shadow.Erase();

        for (int index = 0; index < entityParts.Count; index++)
            entityParts[index].Erase();
    }

    public virtual void OnOtherChunk(Chunk chunk)
    {
        _chunk = chunk;
    }

    public virtual void Update(float deltaTime)
    {
        _time = _time + deltaTime;
        _damagedCooldown = _damagedCooldown - deltaTime;

        if (_physics != null)
            _physics.ApplyPhysics(chunk, deltaTime, ref _worldPosition);

        if (IsometricMain.doesDebugging)
        {
            debugLabel.SetPosition(worldCamera.GetScreenPosition(worldPosition));
            debugLabel.text = debugString;
        }
    }

    public void DespawnEntity()
    {
        _spawned = false;
    }

    public void MoveToOtherChunk()
    {
        Chunk newChunk = world.GetChunkByCoordinate(Chunk.ToChunkCoordinate(worldPosition));

        if (newChunk != null)
        {
            newChunk.AddEntity(this);
            OnOtherChunk(newChunk);
        }
        else
            DespawnEntity();
    }

    public void ApplyDamage(Damage damage)
    {
        if (damagedCooldown < 0f)
            damage.OnApplyDamage(this);
    }

    public void AttachCollider(float width, float height)
    {
        _collider = new EntityAABBCollider(this, width, height);
    }

    public void AttachPhysics(float width, float height, float gravityModifier = 1f, System.Action onTileCallback = null)
    {
        _physics = new EntityPhysics(new EntityAABBCollider(this, width, height), gravityModifier, onTileCallback);
    }

    public Tile GetTileAtWorldPosition(Vector3Int position)
    {
        return chunk.GetTileAtWorldPosition(position);
    }

    public Tile GetTileAtWorldPosition(Vector3 position)
    {
        return GetTileAtWorldPosition(Vector3Int.FloorToInt(position));
    }

    public virtual string debugString
    {
        get
        {
            return string.Concat(
                "chunk : ", chunk.coordination, "\n",
                "position : ", worldPosition, "\n",
                "tilePosition : ", tilePosition, "\n"
                );
        }
    }
}