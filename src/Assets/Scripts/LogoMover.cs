using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoMover : MonoBehaviour {
    public float targetScale;
    public float timeToLerp;
    private float scaleModifier = 1;
    private Vector3 originalScale;
    private bool backwards = false;

    void Start() {
        originalScale = transform.localScale;
        StartCoroutine(Lerp(targetScale, timeToLerp));
    }

    void OnDisable() {
        StopCoroutine(Lerp(targetScale, timeToLerp));
        transform.localScale = originalScale;
    }

    IEnumerator Lerp(float endValue, float duration) {
        float time = 0;
        float startValue = scaleModifier;
        Vector3 startScale = transform.localScale;

        while (true) {
            scaleModifier = Mathf.Lerp(startValue, endValue, time / duration);
            transform.localScale = backwards ? startScale / scaleModifier : startScale * scaleModifier;
            time += Time.deltaTime;

            if (time >= duration) {
                scaleModifier = 1;
                time = 0;
                startValue = scaleModifier;
                startScale = transform.localScale;
                backwards = !backwards;
            }

            yield return null;
        }
    }
}