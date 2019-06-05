using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> { }

public class PrimaryButtonWatcher : MonoBehaviour {
    public  PrimaryButtonEvent               primaryButtonPress;
    private bool                             lastButtonState = false;
    private List<UnityEngine.XR.InputDevice> allDevices;
    private List<UnityEngine.XR.InputDevice> devicesWithPrimaryButton;

    void Start() {
        if (primaryButtonPress == null) {
            primaryButtonPress = new PrimaryButtonEvent();
        }

        allDevices               =  new List<UnityEngine.XR.InputDevice>();
        devicesWithPrimaryButton =  new List<UnityEngine.XR.InputDevice>();
        InputTracking.nodeAdded  += InputTracking_nodeAdded;
    }

    // check for new input devices when new XRNode is added
    private void InputTracking_nodeAdded(XRNodeState obj) {
        updateInputDevices();
    }

    void Update() {
        bool tempState          = false;
        bool invalidDeviceFound = false;
        foreach (var device in devicesWithPrimaryButton) {
            bool buttonState = false;
            tempState = 
                device.isValid && // the device is still valid
                device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonState) && // did get a value
                buttonState || // the value we got
                tempState; // cumulative result from other controllers

            if (!device.isValid) {
                invalidDeviceFound = true;
            }
        }

        // Button state changed since last frame
        if (tempState != lastButtonState) {
            primaryButtonPress.Invoke(tempState);
            lastButtonState = tempState;
        }

        // refresh device lists
        if (invalidDeviceFound || devicesWithPrimaryButton.Count == 0) {
            updateInputDevices();
        }
    }

    // find any devices supporting the desired feature usage
    void updateInputDevices() {
        devicesWithPrimaryButton.Clear();
        UnityEngine.XR.InputDevices.GetDevices(allDevices);
        bool discardedValue;

        foreach (var device in allDevices) {
            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out discardedValue)) {
                devicesWithPrimaryButton.Add(device); // Add any devices that have a primary button.
            }
        }
    }
}