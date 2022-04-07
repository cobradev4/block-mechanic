using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed = 10.0f;
    public float jumpSpeed = 2.5f;
    public float sensitivity = 0.25f;
    public int renderDistance = 16; //how far to render nearby chunks
    public bool isHead;
    public GameObject body;
    public World world;
    public TransitionHandler transitionHandler;
    public bool inVehicle = false;
    private Chunk previousHitChunk = new Chunk();
    private int previousTriangleIndex;
    private Vector3 previousLookCoordinates;
    private bool previousSet = false;
    private bool canJump = true; //for jump cool down because of not enough sensitivity in ground detection with rays

    void Start() {
        if (isHead) {
            Cursor.visible = false; //hide mouse
            Cursor.lockState = CursorLockMode.Locked;
        } else { //body
            world.LoadPlayerChunks(renderDistance, transform.position);
        }
    }
    
    void FixedUpdate() {
        if (!isHead && !inVehicle) {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool up = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift);

            float appliedSpeed = speed * (sprint ? 1.2f : 1.0f); //increase speed by 20% when left shift is held down

            GetComponent<Rigidbody>().velocity = transform.forward * appliedSpeed * vertical + transform.right * appliedSpeed * horizontal + 
            (transform.up * GetComponent<Rigidbody>().velocity.y + ((up && OnGround() && canJump) ? transform.up * jumpSpeed : Vector3.zero));

            if (up) StartCoroutine("WaitToJump");
        }
    }

    void Update() {
        if (isHead && !inVehicle) {
            //mouse movement
            float xRot = Input.GetAxis("Mouse Y") * sensitivity * -1; //invert
            float yRot = Input.GetAxis("Mouse X") * sensitivity;
            
            // prevent player from inverting controls
            if ((transform.eulerAngles.x + xRot <= 270 && transform.eulerAngles.x + xRot > 200) || (transform.eulerAngles.x + xRot >= 90 && transform.eulerAngles.x + xRot < 270)) xRot = 0;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x + xRot, transform.eulerAngles.y + yRot, 0.0f);
            body.transform.eulerAngles = new Vector3(0.0f, body.transform.eulerAngles.y + yRot, 0.0f); //rotate body with head

            //get mouse position for block editing
            Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hitData;

            if (Physics.Raycast(ray, out hitData, 8)) { // 3rd argument is maximum distance
                Vector3 lookCoordinates = hitData.point;
                int triangleIndex = hitData.triangleIndex;
                Transform hitChunk = hitData.transform;

                if (!previousSet && hitChunk.tag == "Chunk") {
                    world.ModifyBlock("highlight", triangleIndex, hitChunk.GetComponent<Chunk>());
                    previousTriangleIndex = triangleIndex;
                    previousLookCoordinates = lookCoordinates;
                    previousHitChunk = hitData.transform.GetComponent<Chunk>();
                    previousSet = true; //only need to set this to true once to show that the previous variables have all received initial values
                }
                else if (lookCoordinates != previousLookCoordinates && hitChunk.tag == "Chunk") {
                    world.ModifyBlock("removeHighlight", previousTriangleIndex, previousHitChunk);
                    world.ModifyBlock("highlight", triangleIndex, hitChunk.GetComponent<Chunk>()); //show which voxel is being selected (current voxel)
                    previousTriangleIndex = triangleIndex;
                    previousLookCoordinates = lookCoordinates;
                    previousHitChunk = hitChunk.GetComponent<Chunk>();
                } 
                
                //left click --> remove block or delete vehicle
                if (Input.GetMouseButtonDown(0)) {
                    if (hitChunk.tag == "Chunk") world.ModifyBlock("remove", triangleIndex, hitChunk.GetComponent<Chunk>()); //remove block
                    if (hitChunk.tag == "Vehicle") hitChunk.GetComponent<VehicleChunk>().Delete(); //delete vehicle
                }

                //right click --> place block or control vehicle
                if (Input.GetMouseButtonDown(1)) {
                    if (hitChunk.tag == "Chunk") {
                        world.ModifyBlock("removeHighlight", triangleIndex, hitChunk.GetComponent<Chunk>()); //need to remove the higlight before the triangle index becomes obsolete with a new voxel
                        world.ModifyBlock("place", triangleIndex, hitChunk.GetComponent<Chunk>());
                    }
                    if (hitChunk.tag == "Vehicle") hitChunk.GetComponent<VehicleChunk>().Control();
                }

                //middle click --> check for vehicle created
                if (Input.GetKeyDown(KeyCode.Mouse2)) {
                    Debug.Log("checking for vehicle");
                    world.DetectVehicle(hitChunk.GetComponent<Chunk>(), triangleIndex, lookCoordinates);
                }

            } else { //player is not looking at a voxel
                world.ModifyBlock("removeHighlight", previousTriangleIndex, previousHitChunk); //prevent block from remaining highlighted after player looks away into the air
            }

            //show menu if escape is pressed
            if (Input.GetKeyDown(KeyCode.Escape)) transitionHandler.GameToMenu();   
        }
    }

    private IEnumerator WaitToJump() {
        canJump = false;
        yield return new WaitForSeconds(0.5f);
        canJump = true;
    }

    private bool OnGround() {
        Ray ray = new Ray(transform.position, -transform.up); //downwards ray (rays worked better than collisions for some reason)
        return (Physics.Raycast(ray, 1.25f)); //make 1.25f lower for more sensitivity
    }

    //determine when player moves to a new chunk
    private void OnTriggerEnter(Collider collider) {
        Debug.Log("player changed current chunk");
        world.LoadPlayerChunks(renderDistance, transform.position); //now that the player has hit a new chunk we should reload the render distance
    }
}
