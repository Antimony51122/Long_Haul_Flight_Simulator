using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour {
    public float Speed = 0.0f;

    private Animator _controller = null;

    void Start() {
        _controller = GetComponent<Animator>();
    }

    void Update() {
        _controller.SetFloat("Speed", Speed);
    }
}