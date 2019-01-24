﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator
{
    private World _world;

    public ChunkGenerator(World world)
    {
        _world = world;
    }

    public Chunk GenerateChunk(Vector2Int coordination)
    {
        Chunk chunk = new Chunk(_world, coordination);

        chunk.state = ChunkState.Loading;
        
        for (int i = 0; i < Chunk.Length; i++)
        {
            for (int j = 0; j < Chunk.Length; j++)
            {
                int y = Mathf.CeilToInt(Mathf.PerlinNoise((i + coordination.x * Chunk.Length + 1024f) * 0.1f, (j + coordination.y * Chunk.Length + 1024f) * 0.1f) * 12f) + 2;
                //int y = 1;
                //if ((i == 2 && j > 2 && j < 6) || (i > 0 && i < 4 && j == 3)) y = 3;//Hi

                for (int k = 0; k < Chunk.Height; k++)
                {
                    Block block = Block.BlockAir;

                    if (k == 0)
                        block = Block.GetBlockByKey("bedrock");
                    else if (k < y)
                    {
                        int biome = 0; //Mathf.RoundToInt(Mathf.PerlinNoise((i + coordination.x * Chunk.Length + 1024f) * 0.01f, (j + coordination.y * Chunk.Length + 1024f) * 0.01f));

                        if (biome > 0)
                            block = Block.GetBlockByKey("sand");
                        else
                            block = Block.GetBlockByKey(k > 4 + RXRandom.Range(0, 5) ? "stone" : "grass");
                    }

                    chunk[i, k, j].SetBlock(block);
                }
            }
        }

        chunk.state = ChunkState.Loaded;

        return chunk;
    }
}