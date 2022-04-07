using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLinker : MonoBehaviour {
    public GameObject linkedObject;
    public Vector3 offsets;
    
    void Update() {
        transform.position = new Vector3(linkedObject.transform.position.x + offsets.x, linkedObject.transform.position.y + offsets.y, linkedObject.transform.position.z + offsets.z);
    }
}
