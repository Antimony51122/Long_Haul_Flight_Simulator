using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraMount : MonoBehaviour {
    public Transform Mount = null;
    public float     Speed = 5.0f;

    void Start() { }

    void LateUpdate() {
        transform.position = Vector3.Lerp(transform.position, Mount.position, Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Mount.rotation, Time.deltaTime);
    }
}