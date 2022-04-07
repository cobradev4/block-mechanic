using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleChunk : MonoBehaviour {
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>(); // how to unwrap texture
    private Mesh mesh;
    private bool controlling = false;
    public byte[,,] voxelMap;
    public int xSize;
    public int ySize;
    public int zSize;
    public World world;
    public GameObject player;
    private GameObject cam;

    void Start() {
        cam = new GameObject();
        cam.transform.SetParent(transform);
        cam.AddComponent<Camera>();
        cam.GetComponent<Camera>().enabled = false;
    }

    void FixedUpdate() {
        if (controlling) {
            float speed = 20.0f;
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool up = Input.GetKey(KeyCode.LeftShift);
            bool down = Input.GetKey(KeyCode.LeftControl);

            GetComponent<Rigidbody>().velocity = transform.forward * speed * vertical + transform.right * speed * horizontal;
        }   
    }

    void Update() {
        if (controlling) {
            float sensitivity = player.transform.GetChild(0).GetComponent<PlayerController>().sensitivity;
            float xRot = Input.GetAxis("Mouse Y") * sensitivity * -1; //invert
            float yRot = Input.GetAxis("Mouse X") * sensitivity;

            // prevent player from inverting controls
            if ((transform.eulerAngles.x + xRot <= 270 && transform.eulerAngles.x + xRot > 200) || (transform.eulerAngles.x + xRot >= 90 && transform.eulerAngles.x + xRot < 270)) xRot = 0;
            
            transform.eulerAngles = new Vector3(transform.eulerAngles.x + xRot, transform.eulerAngles.y + yRot, 0.0f);

            //leave vehicle if escape is pressed
            if (Input.GetKeyDown(KeyCode.Escape)) {
                controlling = false;
                player.GetComponentsInChildren<PlayerController>()[0].inVehicle = false;
                player.GetComponentsInChildren<PlayerController>()[1].inVehicle = false;
                player.GetComponentInChildren<Camera>().enabled = true;
                cam.GetComponent<Camera>().enabled = false;
                player.transform.GetChild(1).GetComponent<MeshCollider>().enabled = true;
                player.transform.GetChild(1).GetComponent<Rigidbody>().useGravity = true;

                Destroy(player.transform.GetChild(1).GetComponent<BoxCollider>());
            }

            cam.transform.position = transform.position + new Vector3(5, 5, 5);
            cam.transform.rotation = transform.rotation;

            player.transform.GetChild(1).position = transform.position;
        }
    }

    public void Load() {
        gameObject.tag = "Vehicle";

        xSize = voxelMap.GetLength(0);
        ySize = voxelMap.GetLength(1);
        zSize = voxelMap.GetLength(2);

        CreateMeshData();
        CreateMesh();
    }

    public void Control() {
        cam.GetComponent<Camera>().enabled = true;
        player.GetComponentInChildren<Camera>().enabled = false;
        player.GetComponentsInChildren<PlayerController>()[0].inVehicle = true;
        player.GetComponentsInChildren<PlayerController>()[1].inVehicle = true;
        player.transform.GetChild(1).GetComponent<MeshCollider>().enabled = false;
        player.transform.GetChild(1).GetComponent<Rigidbody>().useGravity = false;;

        player.transform.GetChild(1).gameObject.AddComponent<BoxCollider>(); //needed for loading chunks without a mesh collider
        player.transform.GetChild(1).gameObject.GetComponent<BoxCollider>().isTrigger = true;

        controlling = true;
    }

    public void Delete() {
        Destroy(gameObject);
    }

    void CreateMeshData() {
        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                for (int z = 0; z < zSize; z++) {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    bool CheckForVoxel(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x); //round down and return int
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //make sure each index is in range of the array
        if (!(x < 0 || y < 0 || z < 0 || x >= xSize || y >= ySize || z >= zSize))
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
