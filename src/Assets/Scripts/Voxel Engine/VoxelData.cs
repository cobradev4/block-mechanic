using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Based on: https://github.com/b3agz/Code-A-Game-Like-Minecraft-In-Unity */

public static class VoxelData {
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 16;
    public static readonly int TextureAtlasSizeInBlocks = 8;
    public static float NormalizedBlockTextureSize {
        get { return 1f / (float) TextureAtlasSizeInBlocks; }
    }

    // coordinates of all 8 vertices on voxel
    public static readonly Vector3[] voxelVertices = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    /* Offsets for six faces to be used to prevent them from being drawn unnecessarily */
    public static readonly Vector3[] faceOffsets = new Vector3[6] {
        new Vector3(0.0f, 0.0f, -1.0f), //back
        new Vector3(0.0f, 0.0f, 1.0f), //front
        new Vector3(0.0f, 1.0f, 0.0f), //top
        new Vector3(0.0f, -1.0f, 0.0f), //bottom
        new Vector3(-1.0f, 0.0f, 0.0f), //left
        new Vector3(1.0f, 0.0f, 0.0f) //right
    };

    // order of vertices to form each face of voxel
    public static readonly int[,] voxelTriangles = new int[6, 4] { // 6 faces, 6 vertices per face (only need 4 because some vertices are shared by triangles)
        // Back, Front, Top, Bottom, Left, Right (0 1 2 2 1 3)
        {0, 3, 1, 2}, //back face
        {5, 6, 4, 7}, //front face
        {3, 7, 2, 6}, //top face
        {1, 5, 0, 4}, //bottom face
        {4, 7, 0, 3}, //left face
        {1, 2, 5, 6} //right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4] { //textures
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };
}
