using System.Collections;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/* Based on: https://github.com/b3agz/Code-A-Game-Like-Minecraft-In-Unity */

public class World : MonoBehaviour {
    public Material material;
    public BlockType[] blockTypes;
    public int xChunkCount;
    public int yChunkCount;
    public int zChunkCount;
    public int selectedBlockID = 0;
    public float perlinScale = 50.0f;
    public bool fromLoad = false; //know if world is being created first time by a world save
    public bool loading = true;
    public bool showLoadingIndicator = false;
    public GameObject player;
    private GameObject[,,] chunks;
    private byte[,,][,,] worldMap; //3d dimensional array of 3d dimesional byte arrays (3d array of 3d array of voxel ids) (multidimensional jagged array)

    void Start() { //fill world with randomly generated chunks
        chunks = new GameObject[xChunkCount, yChunkCount, zChunkCount];

        //initialize all chunk values to null
        for (int x = 0; x < xChunkCount; x++) {
            for (int y = 0; y < yChunkCount; y++) {
                for (int z = 0; z < zChunkCount; z++) {
                    chunks[x, y, z] = null;
                }
            }
        }

        if (!fromLoad) GenerateWorld(); //generate world asynchronously
        else loading = false;
    }

    async private void GenerateWorld() {
        showLoadingIndicator = true;
        WorldGenerator generator = new WorldGenerator(xChunkCount, yChunkCount, zChunkCount, perlinScale);
        worldMap = await Task.Run<byte[,,][,,]>(() => generator.GenerateWorld()); //generate world asynchronously
        loading = false;
        showLoadingIndicator = false;
    }

    public void LoadPlayerChunks(int renderDistance, Vector3 playerPosition) { //TODO - modify this to change based on chunk position with simpler calculations for significant performance improvements
        for (int x = 0; x < xChunkCount; x++) {
            for (int y = 0; y < yChunkCount; y++) {
                for (int z = 0; z < zChunkCount; z++) {
                    int distance = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(playerPosition.x - (float) x * (float) VoxelData.ChunkWidth, 2) + 
                     Mathf.Pow(playerPosition.y - (float) y * (float) VoxelData.ChunkHeight, 2) + 
                     Mathf.Pow(playerPosition.z - (float) z * (float) VoxelData.ChunkWidth, 2))); //distance formula in 3 dimensions
                    
                    if (distance <= renderDistance) {
                        if (chunks[x, y, z] == null) CreateChunk(x, y, z); //only want to recreate chunk if doesn't already exist
                    }
                    else DestroyChunk(x, y, z);
                }
            }
        }
    }

    private void CreateChunk(int x, int y, int z) {
        chunks[x, y, z] = new GameObject("Chunk" + " (" + x + "," + y + "," + z + ")");

        chunks[x, y, z].transform.SetParent(transform); //make chunks children of world game object
        chunks[x, y, z].AddComponent<MeshRenderer>();
        chunks[x, y, z].AddComponent<MeshFilter>();
        chunks[x, y, z].AddComponent<MeshCollider>(); //needed to allow player to "collide" with ground
        chunks[x, y, z].AddComponent<Chunk>();
        chunks[x, y, z].GetComponent<Chunk>().world = GetComponent<World>();
        chunks[x, y, z].GetComponent<MeshRenderer>().material = material;

        //need to use box collider to determine which chunk player is currently in for render distance
        //queries hit triggers must be unchecked in the project physics settings to prevent blocking the mesh collider from rays
        chunks[x, y, z].AddComponent<BoxCollider>();
        chunks[x, y, z].GetComponent<BoxCollider>().size = new Vector3(VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth);
        chunks[x, y, z].GetComponent<BoxCollider>().isTrigger = true;

        chunks[x, y, z].transform.position = new Vector3(VoxelData.ChunkWidth * x, VoxelData.ChunkHeight * y, VoxelData.ChunkWidth * z);
        
        chunks[x, y, z].GetComponent<Chunk>().chunkX = x;
        chunks[x, y, z].GetComponent<Chunk>().chunkY = y;
        chunks[x, y, z].GetComponent<Chunk>().chunkZ = z;

        chunks[x, y, z].GetComponent<Chunk>().voxelMap = worldMap[x, y, z];

        chunks[x, y, z].GetComponent<Chunk>().Load();
    }

    private void DestroyChunk(int x, int y, int z) {
        if (chunks[x, y, z] != null) {
            Destroy(chunks[x, y, z]);
            chunks[x, y, z] = null;
        }
    }

    // look coordinates caused too many issues so using triangle index instead with chunk identification from hit data
    public void ModifyBlock(string modificationType, int triangleIndex, Chunk chunk) {
        try {
            if (modificationType == "remove") 
            chunks[chunk.chunkX, chunk.chunkY, chunk.chunkZ].GetComponent<Chunk>().Modify("remove", triangleIndex);
            else if (modificationType == "highlight")
                chunks[chunk.chunkX, chunk.chunkY, chunk.chunkZ].GetComponent<Chunk>().Modify("highlight", triangleIndex);
            else if (modificationType == "removeHighlight") 
                chunks[chunk.chunkX, chunk.chunkY, chunk.chunkZ].GetComponent<Chunk>().Modify("removeHighlight", triangleIndex);
            else if (modificationType == "place") {
                chunks[chunk.chunkX, chunk.chunkY, chunk.chunkZ].GetComponent<Chunk>().Modify("place", triangleIndex);
            }
            else
                Debug.Log("Error - Invalid Modification Type!");
        } catch {
            Debug.Log("No block to modify!");
        }
    }

    public void UpdateWorldMap(int chunkX, int chunkY, int chunkZ, int x, int y, int z, byte newID) {
        worldMap[chunkX, chunkY, chunkZ][x, y, z] = newID;
    }
    public void DetectVehicle(Chunk chunk, int triangleIndex, Vector3 position) {
        VehicleDetector detector = new VehicleDetector(worldMap, chunk, triangleIndex, xChunkCount, yChunkCount, zChunkCount);
        (int, int, int, int, int, int)[] detectedVehicleCoordinates = detector.CheckForVehicles();

        if (detectedVehicleCoordinates != null) {
            Vector3[] coordinates = new Vector3[detectedVehicleCoordinates.Length];
            byte[] blockIds = new byte[coordinates.Length];

            for (int i = 0; i < detectedVehicleCoordinates.Length; i++) {
                int x = detectedVehicleCoordinates[i].Item1;
                int y = detectedVehicleCoordinates[i].Item2;
                int z = detectedVehicleCoordinates[i].Item3;
                int xb = detectedVehicleCoordinates[i].Item4;
                int yb = detectedVehicleCoordinates[i].Item5;
                int zb = detectedVehicleCoordinates[i].Item6;

                //add coordinates to vector3
                coordinates[i].x = x * VoxelData.ChunkWidth + xb;
                coordinates[i].y = y * VoxelData.ChunkHeight + yb;
                coordinates[i].z = z * VoxelData.ChunkWidth + zb;
                blockIds[i] = worldMap[x, y, z][xb, yb, zb];

                //change blocks to air
                chunks[x, y, z].GetComponent<Chunk>().ManualPlace(xb, yb, zb, 9);
                worldMap[x, y, z][xb, yb, zb] = 9;
            }

            //minimize coordinates
            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float minZ = Mathf.Infinity;
            foreach (Vector3 coord in coordinates) {
                if (coord.x < minX) minX = coord.x; 
                if (coord.y < minY) minY = coord.y;
                if (coord.z < minZ) minZ = coord.z;
            }
            for (int i = 0; i < coordinates.Length; i++) {
                coordinates[i] = new Vector3(coordinates[i].x - minX, coordinates[i].y - minY, coordinates[i].z - minZ);
            }

            //convert coordinate array to 3d byte array
            float maxX = -Mathf.Infinity;
            float maxY = -Mathf.Infinity;
            float maxZ = -Mathf.Infinity;
            foreach (Vector3 coord in coordinates) {
                if (coord.x > maxX) maxX = coord.x;
                if (coord.y > maxY) maxY = coord.y;
                if (coord.z > maxZ) maxZ = coord.z;
            }

            Byte[,,] vehicle = new byte[(int) maxX + 1, (int) maxY + 1, (int) maxZ + 1];

            //set all bytes to air by default
            for (int x = 0; x < vehicle.GetLength(0); x++) {
                for (int y = 0; y < vehicle.GetLength(1); y++) {
                    for (int z = 0; z < vehicle.GetLength(2); z++) {
                        vehicle[x, y, z] = 9; //air
                    }
                }
            }
            
            //load coordinates into new 3d byte array
            for (int i = 0; i < coordinates.Length; i++) {
                vehicle[(int) coordinates[i].x, (int) coordinates[i].y, (int) coordinates[i].z] = blockIds[i]; 
            }

            //create chunk
            GameObject vehicleChunk = new GameObject("vehicle chunk");
            vehicleChunk.transform.SetParent(transform);
            vehicleChunk.AddComponent<MeshRenderer>();
            vehicleChunk.AddComponent<MeshFilter>();
            vehicleChunk.AddComponent<MeshCollider>();
            vehicleChunk.AddComponent<VehicleChunk>();
            vehicleChunk.GetComponent<MeshRenderer>().material = material;
            vehicleChunk.transform.position = position;
            vehicleChunk.GetComponent<VehicleChunk>().voxelMap = vehicle;
            vehicleChunk.GetComponent<VehicleChunk>().world = GetComponent<World>();
            vehicleChunk.GetComponent<VehicleChunk>().player = player;
            
            //needed to change velocity
            vehicleChunk.AddComponent<Rigidbody>();
            vehicleChunk.AddComponent<MeshCollider>();
            vehicleChunk.GetComponent<MeshCollider>().convex = true; //can't use nonconvex mesh collider with rigid body so must use convex with nonconvex

            vehicleChunk.GetComponent<VehicleChunk>().Load();
        }
        else Debug.Log("invalid block");
    }

    public void FixVoxelPlacement(Vector3 coordinates, Chunk chunk, int newBlockID) {
        int x = (int) coordinates.x;
        int y = (int) coordinates.y;
        int z = (int) coordinates.z;
        int chunkX = chunk.chunkX;
        int chunkY = chunk.chunkY;
        int chunkZ = chunk.chunkZ;

        if (x >= VoxelData.ChunkWidth) {
            x = 0;
            chunkX++;
        }
        else if (y >= VoxelData.ChunkHeight) {
            y = 0;
            chunkY++;
        }
        else if (z >= VoxelData.ChunkWidth) {
            z = 0;
            chunkZ++;
        }
        else if (x < 0) {
            x = VoxelData.ChunkWidth - 1;
            chunkX--;
        }
        else if (y < 0) {
            y = VoxelData.ChunkHeight - 1;
            chunkY--;
        }
        else { //z < 0
            z = VoxelData.ChunkWidth - 1;
            chunkZ--;
        }

        //TODO - catch world border (chunk index out of range)
        chunks[chunkX, chunkY, chunkZ].GetComponent<Chunk>().ManualPlace(x, y, z, newBlockID);
        worldMap[chunkX, chunkY, chunkZ][x, y, z] = (byte) newBlockID;
    }

    public void WorldToFile(string path, Vector3 playerPosition) {
        /* https://stackoverflow.com/questions/25598238/save-and-load-a-jagged-array */

        //cannot serialize Vector3s so mucyh serialize individual floats
        //using a tuple to store all the values for seraliziation to be saved to file
        (byte[,,][,,], float x, float y, float z) saveData = (worldMap, playerPosition.x, playerPosition.y, playerPosition.z);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(File.OpenWrite(path), saveData);
    }

    public Vector3 FileToWorld(string path) {
        BinaryFormatter formatter = new BinaryFormatter();
        (byte[,,][,,], float, float, float) saveData = ((byte[,,][,,], float, float, float)) formatter.Deserialize(File.OpenRead(path));

        worldMap = saveData.Item1;
        Vector3 playerPosition = new Vector3(saveData.Item2, saveData.Item3, saveData.Item4);

        if (chunks != null) {
            foreach (GameObject chunk in chunks) {
                Destroy(chunk);
            }
            chunks = null;
        }

        xChunkCount = worldMap.GetLength(0);
        yChunkCount = worldMap.GetLength(1);
        zChunkCount = worldMap.GetLength(2);

        chunks = new GameObject[xChunkCount, yChunkCount, zChunkCount];

        //initialize all chunk values to null
        for (int x = 0; x < xChunkCount; x++) {
            for (int y = 0; y < yChunkCount; y++) {
                for (int z = 0; z < zChunkCount; z++) {
                    chunks[x, y, z] = null;
                }
            }
        }

        return playerPosition;
    }
}

[System.Serializable]
public class BlockType {
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // back, front, top, bottom, left, right

    public int GetTextureID(int faceIndex) {
        switch (faceIndex) {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID");
                return 0;
        }
    }
}