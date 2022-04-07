using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class SettingsController : MonoBehaviour {
    public Transform player;
    private Transform fov;
    private Transform antialiasing;
    private Transform sensitivity;
    private Transform jumpSpeed;
    private Transform speed;
    private Transform renderDistance;

    void Start() {
        fov = transform.GetChild(0);
        antialiasing = transform.GetChild(1);
        sensitivity = transform.GetChild(2);
        jumpSpeed = transform.GetChild(3);
        speed = transform.GetChild(4);
        renderDistance = transform.GetChild(5);
        
        //show current field of view;
        fov.GetChild(1).GetComponent<TMP_InputField>().text = player.GetChild(0).GetComponent<Camera>().fieldOfView.ToString();

        //show current antialiasing setting
        int value;
        if (player.GetChild(0).GetComponent<PostProcessLayer>().antialiasingMode == PostProcessLayer.Antialiasing.None) value = 1;
        if (player.GetChild(0).GetComponent<PostProcessLayer>().antialiasingMode == PostProcessLayer.Antialiasing.FastApproximateAntialiasing) value = 2;
        else value = 3;
        antialiasing.GetChild(1).GetComponent<TMP_Dropdown>().value = value;

        //show current sensitivity setting
        float sensitivityValue = player.GetChild(0).GetComponent<PlayerController>().sensitivity;
        sensitivity.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sensitivity (" + sensitivityValue + ")";
        sensitivity.GetChild(1).GetComponent<Slider>().value = sensitivityValue; //max sensitivity value is 15

        //show current jump speed setting
        jumpSpeed.GetChild(1).GetComponent<TMP_InputField>().text = player.GetChild(1).GetComponent<PlayerController>().jumpSpeed.ToString();

        //show current speed setting
        speed.GetChild(1).GetComponent<TMP_InputField>().text = player.GetChild(1).GetComponent<PlayerController>().speed.ToString();

        //show current render distance setting
        int distanceValue = player.GetChild(1).GetComponent<PlayerController>().renderDistance; //render distance comes from body
        renderDistance.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Render Distance (" + distanceValue + ")";
        renderDistance.GetChild(1).GetComponent<Slider>().value = distanceValue;
    }

    public void OnFieldOfViewUpdate(string newValue) {
        player.GetChild(0).GetComponent<Camera>().fieldOfView = float.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture); //convert string to float
    }

    public void OnAntialiasingUpdate(int newValue) {
        if (newValue == 0) player.GetChild(0).GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.None;
        else if (newValue == 1) player.GetChild(0).GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
        else player.GetChild(0).GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
    }

    public void OnSensitivityUpdate(float newValue) {
        sensitivity.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Sensitivity (" + newValue + ")";
        player.GetChild(0).GetComponent<PlayerController>().sensitivity = newValue;
    }

    public void OnJumpSpeedUpdate(string newValue) {
        player.GetChild(1).GetComponent<PlayerController>().jumpSpeed = float.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture);
    }

    public void OnSpeedUpdate(string newValue) {
        player.GetChild(1).GetComponent<PlayerController>().speed = float.Parse(newValue, System.Globalization.CultureInfo.InvariantCulture);
    }

    public void OnRenderDistanceUpdate(float newValue) {
        renderDistance.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Render Distance (" + newValue + ")";
        player.GetChild(1).GetComponent<PlayerController>().renderDistance = (int) newValue; //render distance comes form body
    }
}
