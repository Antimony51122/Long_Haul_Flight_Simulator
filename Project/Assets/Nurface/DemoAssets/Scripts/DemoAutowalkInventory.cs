using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class DemoAutowalkInventory : MonoBehaviour {
    // ==============================================================================
    // Config Params
    // ==============================================================================

    // Move speed
    public float moveSpeed = 3f;
    // If the player is allowed to move (don't allow movement when looking at UI)
    public bool allowMovement = true;
    // My character controller
    private CharacterController myCC;
    // Should I move forward or not
    private bool moveForward;
    // Gaze Input Module, to know what object user is looking at
    private GazeInputModuleInventory gazeInputModule;

    // ==============================================================================
    // Main Loop
    // ==============================================================================

    void Start () {
        // Get CharacterController
        myCC = GetComponent<CharacterController>();
        
        // Find GazeInputModule
        gazeInputModule = GameObject.FindObjectOfType<GazeInputModuleInventory>();
    }

    void Update() {
        // If there is an interactive object at gaze and it's within distance, don't move towards it
        if (allowMovement == false || GameObject.Find("InventoryUI(Clone)") != null) {
            // do not allow movement
            if (moveForward == true) {
                moveForward = false;
            }
        }
        // Otherwise if the Google VR button, or the Gear VR touchpad is pressed
        else if (Input.GetButtonDown("Fire1") /* ||
                 // returns true if the primary button (typically “A”) was pressed this frame.
                 OVRInput.GetDown(OVRInput.Button.One) */) {
            GameObject currentGazeObject = gazeInputModule.GetCurrentGameObject();
            if (currentGazeObject != null) {
                if (currentGazeObject.layer == 5) {
                    return;
                }
            }

            // Change the state of moveForward
            moveForward = !moveForward;
            if (moveForward == false) {
                myCC.SimpleMove(Vector3.zero);
            }
        }

        // Check to see if I should move
        if (moveForward) {
            // Find the forward direction
            Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
            // Tell CharacterController to move forward
            myCC.SimpleMove(forward * moveSpeed);
        }
    }

    // ==============================================================================
    // Custom Methods
    // ==============================================================================

    public void AllowMovement(bool status) {
        allowMovement = status;
        if (moveForward == true) {
            moveForward = false;
        }
    }
}
