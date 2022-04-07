using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingIndicator : MonoBehaviour {
    public float changeTime;

    void Start() {
        StartCoroutine(UpdateText());
    }

    IEnumerator UpdateText() {
        string text = GetComponent<TextMeshProUGUI>().text;
        if  (text == "Loading...")
            GetComponent<TextMeshProUGUI>().text = "Loading";
        else if (text == "Loading")
            GetComponent<TextMeshProUGUI>().text = "Loading.";
        else if (text == "Loading.")
            GetComponent<TextMeshProUGUI>().text = "Loading..";
        else //loading..
            GetComponent<TextMeshProUGUI>().text = "Loading...";
        yield return new WaitForSeconds(changeTime);

        StartCoroutine(UpdateText());
    }
}
