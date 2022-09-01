using UnityEngine;

namespace Tturna.ThreeD.Weapons
{
    [CreateAssetMenu(fileName = "TturnaWeaponSO", menuName = "ScriptableObjects/TturnaWeaponScriptableObject")]
    public class Tt_WeaponScriptableObject : ScriptableObject
    {
        public enum WeaponType { Pistol, Shotgun, SMG, Rifle}

        [Header("General")]
        public string weaponName;
        public WeaponType weaponType;
        public int weaponId;
        public GameObject modelPrefab;
        public RuntimeAnimatorController animator;
        public Sprite interactIndicatorIcon;

        [Header("Offsets")]
        public Vector3 colliderSize;
        public Vector3 colliderOffset;
        public Vector3 anchorOffset;
        public Vector3 anchorRotationOffset;

        [Header("Statistics")]
        public float damage;
        public float knockback;
        public float useTime;
        public int magazineCapacity;
        public int defaultAmmoReserve;
        public float reloadTime;

        [Header("Juice")]
        public float screenShakeStrength;
        public float screenShakeTime;
        public GameObject shotParticleFX;
        public GameObject muzzleParticleFX;
        public Vector3 shotParticleOffset;
        public Vector3 muzzleParticleOffset;
    }
}