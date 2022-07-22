using UnityEngine;
using Tturna.Interaction;
using Tturna.ThreeD.Weapons;

namespace Tturna.ThreeD.FPS
{
    public class Tt_FPSArmsController : MonoBehaviour
    {
        Animator animator;

        void Start()
        {
            animator = GetComponent<Animator>();

            Tt_WeaponInventory.WeaponEquipped += OnWeaponEquipped;
            Tt_WeaponInventory.WeaponUnequipped += OnWeaponUnequipped;
            Tt_WeaponInventory.WeaponFired += OnWeaponFired;
            Tt_WeaponInventory.WeaponReload += OnWeaponReload;
        }

        void OnWeaponEquipped(Tt_Interactable weapon)
        {
            animator.SetBool($"holding{weapon.GetComponent<Tt_Weapon>().weaponSO.weaponType}", true);
        }

        void OnWeaponUnequipped(Tt_Interactable weapon)
        {
            animator.SetBool($"holding{weapon.GetComponent<Tt_Weapon>().weaponSO.weaponType}", false);
        }

        void OnWeaponFired(Tt_Interactable weapon)
        {
            animator.SetTrigger($"shoot{weapon.GetComponent<Tt_Weapon>().weaponSO.weaponType}");
        }

        void OnWeaponReload(Tt_Interactable weapon, Tt_Weapon.ReloadType reloadType)
        {
            Tt_Weapon gun = weapon.GetComponent<Tt_Weapon>();
            string suffix = "Full";
            
            switch (reloadType)
            {
                case Tt_Weapon.ReloadType.Magazine:
                    suffix = "";
                    break;

                case Tt_Weapon.ReloadType.Slide:
                    suffix = "Slide";
                    break;
            }

            animator.SetTrigger($"reload{gun.weaponSO.weaponType + suffix}");
        }
    }
}
