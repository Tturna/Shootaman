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

        void OnWeaponReload(Tt_Interactable weapon)
        {

            animator.SetTrigger($"reload{weapon.GetComponent<Tt_Weapon>().weaponSO.weaponType}");
        }
    }
}
