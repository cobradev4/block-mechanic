using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransitionHandler : MonoBehaviour {
    public GameObject menu;
    public GameObject game;
    public GameObject createWorldButton;

    public void GameToMenu() {
        menu.SetActive(true);
        game.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        createWorldButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Resume World";
    }

    // for after inital game load
    public void MenuToGame() {
        game.SetActive(true);
        menu.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
