using UnityEngine;
using System.Collections;

namespace MobileVRInventory {
    public class EditorMouseLook : MonoBehaviour {
#if UNITY_EDITOR
        // Mouse inputs
        private float mouseX, mouseY, mouseZ = 0;

        void Start() {
            // Set MouseX, Y, and Z values to current Camera's rotation
            mouseX = transform.rotation.eulerAngles.y;
            mouseY = transform.rotation.eulerAngles.x;
            mouseZ = transform.rotation.eulerAngles.z;
        }

        void Update() {
            // If the ALT button is pressed, rotate head
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
                // Get mouse X input
                mouseX += Input.GetAxis("Mouse X") * 5;
                // Keep mouseX value between 0 and 360
                if (mouseX <= -180) {
                    mouseX += 360;
                } else if (mouseX > 180) {
                    mouseX -= 360;
                }

                // Get mouse Y input
                mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
                // Keep mouseY value between 0 and 360
                if (mouseY <= -180) {
                    mouseY += 360;
                } else if (mouseY > 180) {
                    mouseY -= 360;
                }
            }

            // If CTRL button is pressed, tilt head
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                // Get the mouse X axis
                mouseZ += Input.GetAxis("Mouse X") * 5;
                // Keep mouseZ value between 0 and 360
                if (mouseZ <= -180) {
                    mouseZ += 360;
                } else if (mouseZ > 180) {
                    mouseZ -= 360;
                }
            } else {
                // Auto untilt the head if ALT is not being pressed
                mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
            }

            // Set the rotation of the VR Main Camera
            transform.rotation = Quaternion.Euler(mouseY, mouseX, mouseZ);
        }
#endif
    }
}