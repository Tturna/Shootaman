/*  Script made by Tturna
 *  version 0.1
 * 
 *  This script is designed to provide a simple inventory system for 1st person weapons.
 *  Supported features:
 *      - Custom inventory size
 *      - Initial inventory
 *      - Adding items
 *      - Adding items that a player interacts with
 *      - Dropping items into the world
 *      - Changing slots
 *      - Choose item overflow policy
 *  
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tturna.Interaction;

namespace Tturna.ThreeD.Weapons
{
    public class Tt_WeaponInventory : MonoBehaviour
    {
        // Settings
        [Header("Inventory Settings")]

        [SerializeField, Tooltip("Starting inventory items")] List<Tt_Weapon> initialInventory;

        [SerializeField, Tooltip("Maximum number of items allowed in the inventory at once. Set to 0 or lower for unlimited")] int maxInventorySize;

        [SerializeField, Tooltip("What to do when trying to pick up an item with a full inventory")] OverflowPolicy inventoryOverflowPolicy;

        // References
        [Header("Required references")]

        [SerializeField, Tooltip("Gameobject that weapons will be children of. Weapon position will be in relation to this.")] GameObject weaponParent;

        [SerializeField] List<Tt_Interactable> inventory = new List<Tt_Interactable>();
        [SerializeField, Tooltip("Parent object that holds objects with images representing bullets (bullet indicators).")] GameObject magazineIndicator;
        [SerializeField] Text reserveAmmoText;

        // Events
        public static event Action<Tt_Interactable> WeaponEquipped;
        public static event Action<Tt_Interactable> WeaponUnequipped;
        public static event Action<Tt_Interactable> WeaponFired;
        public static event Action<Tt_Interactable> WeaponReload;

        // Utility
        enum OverflowPolicy { Ignore, ReplaceSelected, ReplaceFirst, ReplaceLast }
        Image[] bulledIndicators;
        Tt_Weapon selectedWeapon;
        int selectedIndex;
        bool reloadInAction;

        public static Tt_WeaponInventory Instance;

        private void Awake() => Instance = this;

        void Start()
        {
            if (!magazineIndicator) Debug.LogWarning("Magazine Indicator has not been set.");
            if (!reserveAmmoText) Debug.LogWarning("Reserve ammo indicator has not been set.");

            Tt_Interactor.Interaction += OnInteract;

            // Set up initial inventory

            if (magazineIndicator)
            {
                bulledIndicators = new Image[magazineIndicator.transform.childCount];
                for (int i = 0; i < magazineIndicator.transform.childCount; i++)
                {
                    bulledIndicators[i] = magazineIndicator.transform.GetChild(i).GetComponent<Image>();
                }
            }

            if (selectedWeapon) SwitchMagazineIndicator(true);
        }

        private void Update()
        {
            float scrollDelta = Input.mouseScrollDelta.y;

            if (scrollDelta != 0) ChangeWeapon((int)scrollDelta);
        }

        public bool AddWeapon(Tt_Interactable weapon)
        {
            // If inventory is full
            if (maxInventorySize > 0 && inventory.Count == maxInventorySize)
            {
                switch (inventoryOverflowPolicy)
                {
                    case OverflowPolicy.Ignore:
                        // TODO: indicate that inventory is full
                        return false;

                    case OverflowPolicy.ReplaceSelected:
                        DropWeapon(selectedIndex, false);
                        inventory[selectedIndex] = weapon;

                        AddWeaponToHand(true);
                        break;

                    case OverflowPolicy.ReplaceFirst:
                        inventory[0] = weapon;
                        // TODO: drop first weapon

                        AddWeaponToHand(selectedIndex == 0);
                        break;

                    case OverflowPolicy.ReplaceLast:
                        inventory[inventory.Count - 1] = weapon;
                        // TODO: drop last weapon

                        AddWeaponToHand(selectedIndex == inventory.Count - 1);
                        break;
                }
                return true;
            }

            // Adding a new item
            // Set the weapon as active only if it's the first item added to the inventory
            AddWeaponToHand(inventory.Count == 0);
            inventory.Add(weapon);
            return true;

            // Set up weapon for usage. Update magazine indicator
            void AddWeaponToHand(bool setActive)
            {
                Transform tr = weapon.transform;
                Tt_Weapon tw = weapon.GetComponent<Tt_Weapon>();
                Rigidbody rb = weapon.GetComponent<Rigidbody>();

                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;

                weapon.GetComponent<Collider>().enabled = false;
                weapon.GetComponent<Animator>().enabled = true;

                tr.SetParent(weaponParent.transform);
                tr.localPosition = tw.weaponSO.anchorOffset;
                tr.localEulerAngles = tw.weaponSO.anchorRotationOffset;

                weapon.gameObject.SetActive(setActive);
                if (!setActive) return;
                selectedWeapon = weapon.GetComponent<Tt_Weapon>();
                SwitchMagazineIndicator(true);
                WeaponEquipped?.Invoke(weapon);
            }
        }

        public void DropCurrentWeapon()
        {
            DropWeapon(selectedIndex, true);
        }

        void DropWeapon(int index, bool removeFromList)
        {
            if (reloadInAction) return;

            Tt_Interactable weapon = inventory[index];

            weapon.gameObject.SetActive(true);
            weapon.transform.SetParent(null);
            weapon.GetComponent<Collider>().enabled = true;

            Rigidbody rb = weapon.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(Camera.main.transform.forward * 120, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 100);
            WeaponUnequipped?.Invoke(weapon);

            if (removeFromList)
            {
                inventory.Remove(weapon);

                if (inventory.Count > 0)
                {
                    if (selectedIndex >= inventory.Count)
                        selectedIndex = inventory.Count - 1;

                    Tt_Interactable selected = inventory[selectedIndex];
                    selected.gameObject.SetActive(true);
                    selectedWeapon = selected.GetComponent<Tt_Weapon>();
                    WeaponEquipped?.Invoke(selected);
                }
                else
                {
                    selectedIndex = 0;
                    selectedWeapon = null;
                    SwitchMagazineIndicator(false);
                    return;
                }
            }
            UpdateMagazineIndicator();
        }

        void ChangeWeapon(int indexChangeAmount)
        {
            if (inventory.Count <= 1) return;
            if (reloadInAction) return;

            inventory[selectedIndex].gameObject.SetActive(false);
            selectedIndex = Mathf.Clamp(selectedIndex + indexChangeAmount, 0, inventory.Count - 1);
            inventory[selectedIndex].gameObject.SetActive(true);
            selectedWeapon = inventory[selectedIndex].GetComponent<Tt_Weapon>();

            UpdateMagazineIndicator();
            WeaponEquipped?.Invoke(inventory[selectedIndex]);
        }

        void SwitchMagazineIndicator(bool state)
        {
            magazineIndicator.SetActive(state);
            reserveAmmoText.gameObject.SetActive(state);
            if (state) UpdateMagazineIndicator();
        }

        void UpdateMagazineIndicator()
        {
            for (int i = 0; i < bulledIndicators.Length; i++)
            {
                if (selectedWeapon)
                {
                    bulledIndicators[i].gameObject.SetActive(i < selectedWeapon.currentMagCapacity);
                }
                else
                {
                    bulledIndicators[i].gameObject.SetActive(false);
                }
            }
            reserveAmmoText.text = selectedWeapon?.reserveAmmo.ToString() ?? "";
        }

        public void FireWeapon(Vector3 fireOrigin, Transform lookHitTransform, Vector3 lookPoint, float lookPointDistance)
        {
            if (!selectedWeapon) return;
            if (reloadInAction) return;
            if (!selectedWeapon.Fire(fireOrigin, lookHitTransform, lookPoint, lookPointDistance, true)) return;

            bulledIndicators[selectedWeapon.currentMagCapacity].gameObject.SetActive(false);
            reserveAmmoText.text = selectedWeapon.reserveAmmo.ToString();

            if (selectedWeapon.currentMagCapacity == 0) reserveAmmoText.text = selectedWeapon.reserveAmmo > 0 ? "[RELOAD]" : "[EMPTY]";

            WeaponFired?.Invoke(inventory[selectedIndex]);
        }

        public void ReloadWeapon()
        {
            if (!selectedWeapon) return;

            // Pass a callback function so the weapon script can call it when it's done reloading
            reloadInAction = selectedWeapon.Reload(ReloadDone);
            if (!reloadInAction) return;

            WeaponReload?.Invoke(inventory[selectedIndex]);
        }

        void ReloadDone()
        {
            reloadInAction = false;
            UpdateMagazineIndicator();
        }

        void OnInteract(Tt_Interactable target, GameObject source)
        {
            if (target.TryGetComponent(out Tt_Weapon _))
            {
                AddWeapon(target);
            }
        }
    }
}