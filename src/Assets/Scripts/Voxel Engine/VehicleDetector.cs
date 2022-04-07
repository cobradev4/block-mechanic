using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VehicleDetector {
    private byte[,,][,,] worldMap;
    private Chunk chunk;
    private int triangleIndex;
    private int xChunkCount;
    private int yChunkCount;
    private int zChunkCount;
    private List<(int, int, int, int, int, int)> output;

    public VehicleDetector(byte[,,][,,] wm, Chunk c, int ti, int x, int y, int z) {
        worldMap = wm;
        chunk = c;
        triangleIndex = ti;
        xChunkCount = x;
        yChunkCount = y;
        zChunkCount = z;
    }

    public (int, int, int, int, int, int)[] CheckForVehicles() { //return an array of coordinate tuples
        (int, int, int, int) coordinate = chunk.TriangleIndexToCoordinates(triangleIndex); //dont need face but returned anyway
        int xb = coordinate.Item1;
        int yb = coordinate.Item2;
        int zb = coordinate.Item3;

        int x = chunk.chunkX;
        int y = chunk.chunkY;
        int z = chunk.chunkZ;

        if (worldMap[x, y, z][xb, yb, zb] != 14) return null; //make sure blocks are around block 3 (14 is highlighted version of 3)

        output = new List<(int, int, int, int, int, int)>();
        FindAdjacentBlocks(coordinate, x, y, z); //find initial adjacent blocks of selected block

        return output.ToArray();
    }

    private void FindAdjacentBlocks((int, int, int, int) coord, int x, int y, int z) {
        int xb = coord.Item1;
        int yb = coord.Item2;
        int zb = coord.Item3;

        int originalXb = xb;
        int originalX = x;
        int originalYb = yb;
        int originalY = y;
        int originalZb = zb;
        int originalZ = z;

        //loop through x adjacent blocks
        bool xPosComplete = false;
        bool xNegComplete = false;

        xb++;
        while (!xPosComplete) {
            while (xb < VoxelData.ChunkWidth) {
                int val = worldMap[x, y, z][xb, yb, zb];
                if (worldMap[x, y, z][xb, yb, zb] != 3 && worldMap[x, y, z][xb, yb, zb] != 9) { //not equal to block 2 or block 9
                    if (!output.Contains((x, y, z, xb, yb, zb))) {
                        output.Add((x, y, z, xb, yb, zb));
                        FindAdjacentBlocks((xb, yb, zb, 0), x, y, z); //recursion :)
                    }
                } else {
                    xPosComplete = true;
                    break;
                }
                xb++;
            }
            if (!xPosComplete) {
                xb = 0;
                if (x + 1 < xChunkCount) x++;
                else {
                    xPosComplete = true;
                    break;
                }
            }
        }
        xb = originalXb - 1;
        x = originalX;
        while (!xNegComplete) {
            while (xb >= 0) {
                if (worldMap[x, y, z][xb, yb, zb] != 3 && worldMap[x, y, z][xb, yb, zb] != 9) { //not equal to block 2 or block 9
                    if (!output.Contains((x, y, z, xb, yb, zb))) {
                        output.Add((x, y, z, xb, yb, zb));
                        FindAdjacentBlocks((xb, yb, zb, 0), x, y, z); //recursion :)
                    }
                } else {
                    xNegComplete = true;
                    break;
                }
                xb--;
            }
            if (!xNegComplete) {
                xb = VoxelData.ChunkWidth - 1;
                if (x - 1 >= 0) x--;
                else {
                    xNegComplete = true;
                    break;
                }
            }
        }
        x = originalX;
        xb = originalXb; 
        

        //loop through y adjacent blocks
        bool yPosComplete = false;
        bool yNegComplete = false;

        yb++;
        while (!yPosComplete) {
            while (yb < VoxelData.ChunkHeight) {
                if (worldMap[x, y, z][xb, yb, zb] != 3 && worldMap[x, y, z][xb, yb, zb] != 9) { //not equal to block 2 or block 9
                    if (!output.Contains((x, y, z, xb, yb, zb))) {
                        output.Add((x, y, z, xb, yb, zb));
                        FindAdjacentBlocks((xb, yb, zb, 0), x, y, z); //recursion :)
                    }
                } else {
                    yPosComplete = true;
                    break;
                }
                yb++;
            }
            if (!yPosComplete) {
                yb = 0;
                if (y + 1 < yChunkCount) y++;
                else {
                    yPosComplete = true;
                    break;
                }
            }
        }
        yb = originalYb - 1;
        y = originalY;
        while (!yNegComplete) {
            while (yb >= 0) {
                if (worldMap[x, y, z][xb, yb, zb] != 3 && worldMap[x, y, z][xb, yb, zb] != 9) { //not equal to block 2 or block 9
                    if (!output.Contains((x, y, z, xb, yb, zb))) {
                        output.Add((x, y, z, xb, yb, zb));
                        FindAdjacentBlocks((xb, yb, zb, 0), x, y, z); //recursion :)
                    }
                } else {
                    yNegComplete = true;
                    break;
                }
                yb--;
            }
            if (!yNegComplete) {
                yb = VoxelData.ChunkHeight - 1;
                if (y - 1 >= 0) y--;
                else {
                    yNegComplete = true;
                    break;
                }
            }
        }
        y = originalY;
        yb = originalYb; 

        //loop through z adjacent blocks
        bool zPosComplete = false;
        bool zNegComplete = false;

        zb++;
        while (!zPosComplete) {
            while (zb < VoxelData.ChunkWidth) {
                if (worldMap[x, y, z][xb, yb, zb] != 3 && worldMap[x, y, z][xb, yb, zb] != 9) { //not equal to block 2 or block 9
                    if (!output.Contains((x, y, z, xb, yb, zb))) {
                        output.Add((x, y, z, xb, yb, zb));
                        FindAdjacentBlocks((xb, yb, zb, 0), x, y, z); //recursion :)
                    }
                } else {
                    zPosComplete = true;
                    break;
                }
                zb++;
            }
            if (!zPosComplete) {
                zb = 0;
                if (x + 1 < zChunkCount) x++;
                else {
                    zPosComplete = true;
                    break;
                }
            }
        }
        zb = originalZb - 1;
        z = originalZ;
        while (!zNegComplete) {
            while (zb >= 0) {
                if (worldMap[x, y, z][xb, yb, zb] != 3 && worldMap[x, y, z][xb, yb, zb] != 9) { //not equal to block 2 or block 9
                    if (!output.Contains((x, y, z, xb, yb, zb))) {
                        output.Add((x, y, z, xb, yb, zb));
                        FindAdjacentBlocks((xb, yb, zb, 0), x, y, z); //recursion :)
                    }
                } else {
                    zNegComplete = true;
                    break;
                }
                zb--;
            }
            if (!zNegComplete) {
                xb = VoxelData.ChunkWidth - 1;
                if (z - 1 >= 0) z--;
                else {
                    zNegComplete = true;
                    break;
                }
            }
        }
        z = originalZ;
        zb = originalZb;
    }
}