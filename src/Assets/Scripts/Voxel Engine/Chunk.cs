using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Based on: https://github.com/b3agz/Code-A-Game-Like-Minecraft-In-Unity */

public class Chunk : MonoBehaviour {
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>(); // how to unwrap texture
    private Mesh mesh;
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public World world;
    public int chunkX = 0; //keep track of which x, y, z chunk is in world
    public int chunkY = 0;
    public int chunkZ = 0;

    public void Load() {
        gameObject.tag = "Chunk";

        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData() {
        for (int x = 0; x < VoxelData.ChunkWidth; x++) {
            for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    private void ResetItems() {
        // reset everything - TODO - Change to reset only surrounding items for better performance
        vertexIndex = 0;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
    }

    bool CheckForVoxel(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x); //round down and return int
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //make sure each index is in range of the array
        if (!(x < 0 || y < 0 || z < 0 || x >= VoxelData.ChunkWidth || y >= VoxelData.ChunkHeight || z >= VoxelData.ChunkWidth))
            return world.blockTypes[voxelMap[x, y, z]].isSolid; 
        return false;
    }

    bool CheckIfSolid(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        return (world.blockTypes[voxelMap[x, y, z]].isSolid);
    }

    void AddVoxelDataToChunk(Vector3 position) {
        for (int f = 0; f < 6; f++) { // 6 faces
            if (!CheckForVoxel(position + VoxelData.faceOffsets[f]) && CheckIfSolid(position)) { //check if voxel is present at location and if is solid
                
                byte blockID = voxelMap[(int) position.x, (int) position.y, (int) position.z];

                //add vertices, uvs, and triangles
                vertices.Add (position + VoxelData.voxelVertices[VoxelData.voxelTriangles[f, 0]]);
				vertices.Add (position + VoxelData.voxelVertices[VoxelData.voxelTriangles[f, 1]]);
				vertices.Add (position + VoxelData.voxelVertices[VoxelData.voxelTriangles[f, 2]]);
				vertices.Add (position + VoxelData.voxelVertices[VoxelData.voxelTriangles[f, 3]]);
                
                AddTexture(world.blockTypes[blockID].GetTextureID(f));

				triangles.Add (vertexIndex);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 2);
				triangles.Add (vertexIndex + 1);
				triangles.Add (vertexIndex + 3);
				vertexIndex += 4;
            }
             
        }
    }

    /*
        Algorithm to convert triangle index of chunk to coordinates in 3d voxel map array
        Returns x, y, z, and the index of which face is being selected
    */
    public (int, int, int, int) TriangleIndexToCoordinates(int triangleIndex) {
        triangleIndex *= 3; // unity documentation shows the triangle index being multiplied by 3 so I tried that it worked https://docs.unity3d.com/ScriptReference/RaycastHit-triangleIndex.html
        // I think this is because the triangle array contains 1 traingle for every 3 points (being vertices)
        int voxelIndex = 0;

        int currentTriangleIndex = -1;
        for (int x = 0; x < VoxelData.ChunkWidth; x++) {
            for (int y = 0; y < VoxelData.ChunkHeight; y++) {
                for (int z = 0; z < VoxelData.ChunkWidth; z++) {
                    for (int f = 0; f < 6; f++) { //6 faces
                        if (!CheckForVoxel(new Vector3(x, y, z) + VoxelData.faceOffsets[f]) && CheckIfSolid(new Vector3(x, y, z))) { //check if voxel is present at location and if is solid
                            for (int i = 0; i < 6; i++) {
                                currentTriangleIndex++;
                                if (triangleIndex == currentTriangleIndex) return (x, y, z, f);
                            }
                        }
                    }
                    voxelIndex++; //voxel every 6 faces (next voxel)
                }
            }
        }
        
        Debug.Log("ERROR - Something went wrong in TriangleIndexToCoordinates!");
        return (0, 0, 0, 0);
    }

    public void Modify(string type, int triangleIndex) {
        ResetItems();
        (int xCoord, int yCoord, int zCoord, int face) = TriangleIndexToCoordinates(triangleIndex);
        int highlightIndexDistance = 10;
        int finalBlockID = 18;

        if (type == "remove") {
            voxelMap[xCoord, yCoord, zCoord] = 9; //air
            world.UpdateWorldMap(chunkX, chunkY, chunkZ, xCoord, yCoord, zCoord, 9);
            CreateMeshData();
            CreateMesh();
        } else if (type == "highlight") {
            int newBlockID = (int) voxelMap[xCoord, yCoord, zCoord] + highlightIndexDistance; //block id for highlighted version of block
            if (newBlockID <= finalBlockID) {
                voxelMap[xCoord, yCoord, zCoord] = (byte) newBlockID;
                CreateMeshData();
                CreateMesh();
            }   
        } else if (type == "removeHighlight") {
            int newBlockID = (int) voxelMap[xCoord, yCoord, zCoord] - highlightIndexDistance;
            if (newBlockID >= 0) {
                voxelMap[xCoord, yCoord, zCoord] = (byte) newBlockID; //block id for non-highlighted version of block
                CreateMeshData();
                CreateMesh();
            }   
        } else if (type == "place") {
            int newBlockID = world.selectedBlockID; //make sure to account for player changing which block they want to place
            Vector3 coordinates = new Vector3(xCoord, yCoord, zCoord);
            coordinates += VoxelData.faceOffsets[face]; //we want to build the block against whatever face the player is viewing

            try {
                voxelMap[(int) coordinates.x, (int) coordinates.y, (int) coordinates.z] = (byte) newBlockID;
                world.UpdateWorldMap(chunkX, chunkY, chunkZ, (int) coordinates.x, (int) coordinates.y, (int) coordinates.z, (byte) newBlockID);
                CreateMeshData();
                CreateMesh();
            } catch (System.IndexOutOfRangeException e) { //voxel needs to be placed on adjacent chunk
                Debug.Log(e);
                world.FixVoxelPlacement(coordinates, this, newBlockID);
            }
        }
        else
            Debug.Log("Error - Invalid Modification Type for Chunk!");
    }

    public void ManualPlace(int x, int y, int z, int newBlockID) {
        voxelMap[x, y, z] = (byte) newBlockID;
        CreateMeshData();
        CreateMesh();
    }

    void CreateMesh() {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray(); //set vertices
        mesh.triangles = triangles.ToArray(); // set triangles
        mesh.uv = uvs.ToArray(); // set texture

        mesh.RecalculateNormals(); // needed to prevent issues

        transform.GetComponent<MeshFilter>().mesh = mesh;
        transform.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void AddTexture (int textureID) {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}
