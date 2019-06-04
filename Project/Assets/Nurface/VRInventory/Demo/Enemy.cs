using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory
{
    public class Enemy : MonoBehaviour
    {
        public int Health = 100;
        public AudioClip HitSound = null;
        public AudioClip DeathSound = null;
        public bool dead = false;

        public Image healthBarContainer;
        public Slider healthBar;        

        private bool healthBarVisible = false;

        void Start() {            
            UpdateHealthBar(Health);
            HideHealthBarDelayed();            
        }

        public bool TakeDamage(int amount) {
            if (HitSound != null) {
                AudioSource.PlayClipAtPoint(HitSound, this.transform.position);
            }

            if (dead) return false;

            Health -= amount;
            UpdateHealthBarAnimated();

            if (Health < 0) {
                dead = true;
                iTween.RotateTo(this.gameObject, iTween.Hash("rotation", new Vector3(-180f, 45f, 0f), "isLocal", true, "time", 2f, "oncomplete", "DeathAnimationComplete", "easetype", iTween.EaseType.easeOutElastic));
                iTween.MoveTo(this.gameObject, iTween.Hash("position", new Vector3(-4f, 0.25f, -4f), "time", 2f, "easetype", iTween.EaseType.easeOutBounce));

                StartCoroutine(DelayedDeathSound());

                if (healthBarContainer != null) iTween.ScaleTo(healthBarContainer.gameObject, Vector3.zero, 00001f);

                // we've been destroyed
                return true;
            }

            return false;
        }

        IEnumerator DelayedDeathSound() {
            yield return new WaitForSeconds(0.75f);
            if (DeathSound != null) AudioSource.PlayClipAtPoint(DeathSound, this.transform.position, 2f);
        }

        private void DeathAnimationComplete() {            
            // You could, if you wished, destroy the object hear. I've opted to leave the 'corpse' in place.
            //GameObject.Destroy(this.gameObject, 0.25f);
        }

        private void UpdateHealthBar(int value) {
            if(healthBar != null) healthBar.value = value;            
        }

        private void UpdateHealthBarAnimated() {
            if (healthBar == null) return;

            iTween.ValueTo(this.gameObject, iTween.Hash("onupdate", "UpdateHealthBar", "from", healthBar.value, "to", Health, "time", 0.25f, "easetype", iTween.EaseType.easeInBounce));            
        }

        public void ShowHealthBar() {
            healthBarVisible = true;
            iTween.StopByName(this.gameObject, "hideHealthBar");
            iTween.StopByName(this.gameObject, "showHealthBar");

            iTween.ScaleTo(healthBarContainer.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "name", "showHealthBar"));
        }

        public void HideHealthBarDelayed() {            
            healthBarVisible = false;
            iTween.StopByName(this.gameObject, "hideHealthBar");
            iTween.StopByName(this.gameObject, "showHealthBar");

            iTween.ScaleTo(healthBarContainer.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.5f, "delay", 1f, "name", "hideHealthBar"));
        }

        void FixedUpdate() {
            if (dead) return;

            var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);

            if (distance > 3f && healthBarVisible)
            {
                HideHealthBarDelayed();
            } else if(distance <= 3f && !healthBarVisible) {
                ShowHealthBar();
            }            
            
        }
    }
}
