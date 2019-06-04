using UnityEngine;
using System.Collections;

namespace MobileVRInventory
{
    public class DemoDoorInventory : MonoBehaviour
    {
        private Animator myAnim;
        private VRInventory vrInventory;

        public AudioClip doorOpenSound;
        public VRInventoryExampleSceneController exampleController;

        private bool open = false;


        // Use this for initialization
        void Start()
        {
            myAnim = GetComponent<Animator>();
            vrInventory = GameObject.FindObjectOfType<VRInventory>();
            exampleController = GameObject.FindObjectOfType<VRInventoryExampleSceneController>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OpenDoor()
        {
            if (open) return;

            // ignore attempts to open the door from to far away
            var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);
            if (distance > 3f) return;

            // This checks to see if the player is holding the key
            // alternatively, we could have just used 'vrInventory.HasItem("Key")' if we just want to see that they have it
            if (vrInventory.GetEquippedItemName() == "Key") {
                open = true;

                // open the door
                myAnim.SetTrigger("OpenDoor");

                // put the key away
                vrInventory.UnEquipItem();

                // Get rid of the key, we don't need it anymore
                vrInventory.RemoveItem("Key");

                if (doorOpenSound != null) {
                    AudioSource.PlayClipAtPoint(doorOpenSound, this.transform.position);
                }                
            } else {
                // show message
                if(vrInventory.HasItem("Key")) {
                    exampleController.ShowMessage("Equip the key to open this door.");
                } else {                    
                    exampleController.ShowMessage("A key is required to open this door.");
                }
            }

        }
    }
}
