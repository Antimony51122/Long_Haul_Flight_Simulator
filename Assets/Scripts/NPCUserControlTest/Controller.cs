using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    private Animator _animator = null;

    // hashes functions for tuning movements in games
    private int _HorizontalHash = 0;
    private int _VerticalHash   = 0;

    private int _vomitHash = 0;

    void Start() {
        _animator       = GetComponent<Animator>();
        _HorizontalHash = Animator.StringToHash("Horizontal");
        _VerticalHash   = Animator.StringToHash("Vertical");
        _vomitHash      = Animator.StringToHash("Vomit Trigger");
    }

    void Update() {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical")   * 5.66f;

        // use mouse click to control the Player Vomit
        if (Input.GetMouseButtonDown(0)) {
            _animator.SetTrigger(_vomitHash);
        }

        // use up, down, left, right arrows, or wsad keys to control zombie Jill blended movements
        _animator.SetFloat(_HorizontalHash, xAxis, 0.1f, Time.deltaTime);
        _animator.SetFloat(_VerticalHash,   yAxis, 1.0f, Time.deltaTime);
    }
}