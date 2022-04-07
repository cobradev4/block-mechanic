using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator {
    private int xChunkCount;
    private int yChunkCount;
    private int zChunkCount;
    private float perlinScale;
    private byte[,,][,,] output;

    public WorldGenerator(int xChunks, int yChunks, int zChunks, float randomScale) {
        xChunkCount = xChunks;
        yChunkCount = yChunks;
        zChunkCount = zChunks;
        perlinScale = randomScale;
    }

    public byte[,,][,,] GenerateWorld() { //use perlin noise to create a randomly generated world
        output = new byte[xChunkCount, yChunkCount, zChunkCount][,,];
        float[,] heightMap = new float[xChunkCount * VoxelData.ChunkWidth, zChunkCount * VoxelData.ChunkWidth];

        //initialize output
        for (int x = 0; x < xChunkCount; x++) {
            for (int y = 0; y < yChunkCount; y++) {
                for (int z = 0; z < zChunkCount; z++) {
                    output[x, y, z] = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
                }
            }
        }

        //generate height map randomness offset
        int maxOffset = 1000;
        System.Random r = new System.Random(); //cannot use unity random off of main thread
        float xOffset = r.Next(0, maxOffset); 
        float yOffset = r.Next(0, maxOffset);

        //generate height map
        for (int y = 0; y < heightMap.GetLength(1); y++) {
            for (int x = 0; x < heightMap.GetLength(0); x++) {
                float xCoord = ((float) x * 5.0f + xOffset) / (float) heightMap.GetLength(0);
                float yCoord = ((float) y * 5.0f + yOffset) / (float) heightMap.GetLength(1);
                float height = Mathf.PerlinNoise(xCoord, yCoord) * perlinScale; //use perlin noise to randomize
                heightMap[x, y] = height;
            }
        }

        //set heights
        for (int x = 0; x < xChunkCount; x++) {
            for (int z = 0; z < zChunkCount; z++) {
                for (int xb = 0; xb < VoxelData.ChunkWidth; xb++) {
                    for (int zb = 0; zb < VoxelData.ChunkWidth; zb++) {
                        int height = Mathf.RoundToInt(heightMap[x * VoxelData.ChunkWidth + xb, z * VoxelData.ChunkWidth + zb]);
                        int currentHeight = 0;

                        for (int y = 0; y < yChunkCount; y++) {
                            for (int yb = 0; yb < VoxelData.ChunkHeight; yb++) {
                                if (currentHeight < height / 2) output[x, y, z][xb, yb, zb] = 2; //stone
                                else if (currentHeight < height) output[x, y, z][xb, yb, zb] = 1; //dirt
                                else if (currentHeight == height) {
                                    output[x, y, z][xb, yb, zb] = 0; //grass
                                    bool tree = (r.Next(0, 500) == 3); // 1/500 chance of tree
                                    if (tree) { //create tree
                                        Debug.Log("tree!");
                                        //base
                                        TryToPlace(x, y, z, xb, yb + 1, zb, 5);
                                        TryToPlace(x, y, z, xb, yb + 2, zb, 5);
                                        TryToPlace(x, y, z, xb, yb + 3, zb, 5);
                                        TryToPlace(x, y, z, xb, yb + 4, zb, 6);
                                        TryToPlace(x, y, z, xb, yb + 5, zb, 6);

                                        //x-leaves
                                        TryToPlace(x, y, z, xb + 1, yb + 4, zb, 6);
                                        TryToPlace(x, y, z, xb - 1, yb + 4, zb, 6);
                                        TryToPlace(x, y, z, xb + 1, yb + 5, zb, 6);
                                        TryToPlace(x, y, z, xb - 1, yb + 5, zb, 6);
                                        
                                        //z-leaves
                                        TryToPlace(x, y, z, xb, yb + 4, zb + 1, 6);
                                        TryToPlace(x, y, z, xb, yb + 4, zb - 1, 6);
                                        TryToPlace(x, y, z, xb, yb + 5, zb + 1, 6);
                                        TryToPlace(x, y, z, xb, yb + 5, zb - 1, 6);

                                        //both x and z leaves
                                        TryToPlace(x, y, z, xb + 1, yb + 4, zb + 1, 6);
                                        TryToPlace(x, y, z, xb - 1, yb + 4, zb - 1, 6);
                                        TryToPlace(x, y, z, xb + 1, yb + 5, zb + 1, 6);
                                        TryToPlace(x, y, z, xb - 1, yb + 5, zb - 1, 6);

                                        TryToPlace(x, y, z, xb + 1, yb + 4, zb - 1, 6);
                                        TryToPlace(x, y, z, xb - 1, yb + 4, zb + 1, 6);
                                        TryToPlace(x, y, z, xb + 1, yb + 5, zb - 1, 6);
                                        TryToPlace(x, y, z, xb - 1, yb + 5, zb + 1, 6);
                                    }
                                }
                                else if (output[x, y, z][xb, yb, zb] == 0) output[x, y, z][xb, yb, zb] = 9; //air

                                currentHeight++;
                            }
                        }
                    }
                }
            }
        }
        return output;
    }

    private void TryToPlace(int x, int y, int z, int xb, int yb, int zb, byte blockID) { //accounts for overflows from voxel positions into other chunks
        if (xb >= VoxelData.ChunkWidth) { 
            xb = xb - VoxelData.ChunkWidth;
            x++;
        }

        if (yb >= VoxelData.ChunkHeight) {
            yb = yb - VoxelData.ChunkHeight;
            y++;
        }

        if (zb >= VoxelData.ChunkWidth) {
            zb = zb - VoxelData.ChunkWidth;
            zb++;
        }

        if (xb < 0) {
            xb = VoxelData.ChunkWidth + xb;
            x--;
        }

        if (yb < 0) {
            yb = VoxelData.ChunkHeight + yb;
            y--;
        }

        if (zb < 0) {
            zb = VoxelData.ChunkWidth + zb;
            z--;
        }
        
        if (x < xChunkCount && y < yChunkCount && z < zChunkCount && x >= 0 && y >= 0 && z >= 0) output[x, y, z][xb, yb, zb] = blockID;
    }
}
