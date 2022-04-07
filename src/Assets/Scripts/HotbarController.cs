using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour {
    private int selectedSlot = 0;
    private int previousSelectedSlot = 0;
    private Transform[] slots;
    public byte[] slotBlockIDs = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8}; //glad that I put them in order, but may not be in order or may not be static later

    void Start() {
        slots = new Transform[] { transform.GetChild(0), transform.GetChild(1), transform.GetChild(2), 
        transform.GetChild(3), transform.GetChild(4), transform.GetChild(5), transform.GetChild(6),
        transform.GetChild(7), transform.GetChild(8)
        };
    }

    void Update() { //might be a more efficient way to write this later TODO
        if (Input.GetKeyDown(KeyCode.Alpha1)) UpdateSlots(0); //using array index values for easier connection to slots array
        if (Input.GetKeyDown(KeyCode.Alpha2)) UpdateSlots(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UpdateSlots(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UpdateSlots(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UpdateSlots(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UpdateSlots(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) UpdateSlots(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) UpdateSlots(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) UpdateSlots(8);

        //scroll wheel control - will need to implement sensitivity settings for this later TODO
        if (Input.mouseScrollDelta.y != 0) {
            int newSlot = selectedSlot + Mathf.CeilToInt(Input.mouseScrollDelta.y);
            if (newSlot > 8) newSlot = 8;
            if (newSlot < 0) newSlot = 0;
            UpdateSlots(newSlot);
        }
    }

    private void UpdateSlots(int newSlot) {
        previousSelectedSlot = selectedSlot;
        selectedSlot = newSlot;

        slots[previousSelectedSlot].GetChild(0).gameObject.SetActive(false);
        slots[selectedSlot].GetChild(0).gameObject.SetActive(true);

        //change block id that player is placing
        World world = GameObject.Find("World").GetComponent<World>(); //can change to tag system later for better performance
        world.selectedBlockID = slotBlockIDs[selectedSlot];
    }
}
