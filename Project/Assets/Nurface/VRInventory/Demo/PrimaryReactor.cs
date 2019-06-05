using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryReactor : MonoBehaviour {
    public PrimaryButtonWatcher watcher;

    public bool    IsPressed        = false; // public to show button state in the Unity Inspector window
    public Vector3 rotationAngle    = new Vector3(45, 45, 45);
    public float   rotationDuration = 0.25f; // seconds

    private Quaternion offRotation;
    private Quaternion onRotation;
    private Coroutine  rotator;

    void Start() {
        watcher.primaryButtonPress.AddListener(onPrimaryButtonEvent);
        offRotation = this.transform.rotation;
        onRotation  = Quaternion.Euler(rotationAngle) * offRotation;
    }

    public void onPrimaryButtonEvent(bool pressed) {
        IsPressed = pressed;
        if (rotator != null)
            StopCoroutine(rotator);

        if (pressed)
            rotator = StartCoroutine(AnimateRotation(this.transform.rotation, onRotation));
        else
            rotator = StartCoroutine(AnimateRotation(this.transform.rotation, offRotation));
    }

    private IEnumerator AnimateRotation(Quaternion fromRotation, Quaternion toRotation) {
        float t = 0;
        while (t < rotationDuration) {
            transform.rotation =  Quaternion.Lerp(fromRotation, toRotation, t / rotationDuration);
            t                  += Time.deltaTime;
            yield return null;
        }
    }
}