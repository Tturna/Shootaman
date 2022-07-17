using Tturna.Interaction;
using UnityEngine;
using System;
using System.Linq;
using Tturna.Utility;

namespace Tturna.ThreeD.Weapons
{
    [Serializable]
    public class Tt_Weapon : Tt_Interactable
    {
        public Tt_WeaponScriptableObject weaponSO;
        public int currentMagCapacity;
        public int reserveAmmo;

        ParticleSystem ps;
        Animator animator;

        private void Start()
        {
            Instantiate(weaponSO.modelPrefab, transform);
            BoxCollider bc = GetComponent<BoxCollider>();
            bc.size = weaponSO.colliderSize;
            bc.center = weaponSO.colliderOffset;
            currentMagCapacity = weaponSO.magazineCapacity;
            reserveAmmo = weaponSO.defaultAmmoReserve;
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = weaponSO.animator;
        }

        public bool Fire(Vector3 fireOrigin, Transform lookHitTransform, Vector3 lookPoint, float lookPointDistance, bool shakeCamera)
        {
            // TODO: Refactor this function as it is used by both enemies and the player

            if (currentMagCapacity == 0) return false;

            currentMagCapacity--;

            // Hit registration

            if (lookHitTransform)
            {
                if (lookHitTransform.root.gameObject.TryGetComponent(out IHealth target))
                {
                    GameObject hitObject = lookHitTransform.gameObject.TryGetComponent(out Rigidbody _) ? lookHitTransform.gameObject : null;
                    target.TakeDamage(weaponSO.damage, weaponSO.knockback, hitObject, lookPoint);
                }
                else if (lookHitTransform.gameObject.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce((lookPoint - fireOrigin).normalized * weaponSO.knockback, ForceMode.Impulse);
                }
            }

            // Juice and visuals
            if (shakeCamera) Tt_ChildCameraController.instance.CameraShake(weaponSO.screenShakeStrength, weaponSO.screenShakeTime);

            if (weaponSO.shotParticleFX)
            {
                GameObject pfxObject = Instantiate(weaponSO.shotParticleFX);
                pfxObject.transform.position = transform.position + weaponSO.shotParticleOffset;
                pfxObject.transform.LookAt(lookPoint);
                ps = pfxObject.GetComponent<ParticleSystem>();
                float lineLength = Mathf.Clamp(lookPointDistance / 2f, 1, 100);

                ParticleSystem.EmissionModule emissionModule = ps.emission;
                emissionModule.rateOverTime = lineLength * 33;

                ParticleSystem.ShapeModule shapeModule = ps.shape;
                shapeModule.radius = lineLength;

                Vector3 position = shapeModule.position;
                position.z = lineLength;
                shapeModule.position = position;

                ps.Play();
                StartCoroutine(Tt_Helpers.DelayDestroy(pfxObject, ps.main.startLifetime.constant));
            }

            if (weaponSO.muzzleParticleFX)
            {
                GameObject pfxObject = Instantiate(weaponSO.muzzleParticleFX, transform);
                pfxObject.transform.position = transform.position;
                pfxObject.transform.localPosition = weaponSO.muzzleParticleOffset;
                pfxObject.transform.LookAt(lookPoint);
                ps = pfxObject.GetComponent<ParticleSystem>();

                ps.Play();
                StartCoroutine(Tt_Helpers.DelayDestroy(pfxObject, ps.main.startLifetime.constant));
            }

            animator.SetTrigger("shoot");

            if (currentMagCapacity == 0) animator.SetLayerWeight(1, 1);

            return true;
        }

        public bool Reload(Action reloadDoneCallback)
        {
            if (currentMagCapacity == weaponSO.magazineCapacity) return false;
            if (reserveAmmo == 0) return false;

            int missing = weaponSO.magazineCapacity - currentMagCapacity;
            currentMagCapacity = reserveAmmo - missing >= 0 ? weaponSO.magazineCapacity : reserveAmmo;
            reserveAmmo = Mathf.Clamp(reserveAmmo - missing, 0, 999);

            animator.SetLayerWeight(1, 0);
            animator.SetTrigger("reload");
            float l = animator.runtimeAnimatorController.animationClips.Where(n => n.name == "PistolReload").ToArray()[0].length;
            StartCoroutine(Tt_Helpers.DelayExecute(reloadDoneCallback, l));
            return true;
        }
    }
}