// This is the base enemy script that can be inherited for additional features and shit

/* TODO:
 * - Consider using an animation controller state machine for behavior states
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tturna.Utility;

namespace Tturna.ThreeD
{
    public class Tt_Enemy : Tt_Agent
    {
        public enum AttackIntervalType { AfterAttack, BeforeAttack}

        [Header("Settings")]
        [SerializeField] protected float moveSpeed;
        [SerializeField] float gravityMultiplier;
        [SerializeField] protected float lookSmoothSpeed;
        [SerializeField] float aggroDistance;
        [SerializeField] protected AttackIntervalType attackIntervalType;
        [SerializeField] protected float attackInterval;
        [SerializeField, Tooltip("How far the enemy has to be from the player before it starts following the player.")] protected float followPlayerThreshold;
        [SerializeField, Tooltip("How close the enemy has to be to the player before it stops following the player.")] protected float stopFollowPlayerThreshold;

        [Header("References")]
        [SerializeField, Tooltip("The point from which the enemy should look at the player.")] protected GameObject lookSource;
        [SerializeField, Tooltip("Array of objects that cause critical hits when damaged.")] protected GameObject[] criticalObjects;
        [SerializeField] GameObject healthBar;
        [SerializeField] GameObject healthBarForeground;
        [SerializeField] GameObject rigObject;
        [SerializeField] ParticleSystem bloodPFX;
        [SerializeField] GameObject bloodDecal;

        // Utility
        protected Transform playerTransform;
        protected CharacterController cc;
        protected Rigidbody[] ragdollRigidbodies;
        protected List<Coroutine> coroutines = new List<Coroutine>();

        protected Vector3 moveVector;
        protected Vector3 lookDirection;
        protected Vector3 positionDifferenceToPlayer;
        protected Vector3 headObjectPosition => lookSource.transform.position;
        protected Animator animator;
        protected bool isAggrevated, isWalking, canAttack, isPlayerVisible, isStunned;
        protected float distanceToPlayer;
        protected int rayMask => LayerMask.GetMask(new string[] { "Default", "Player", "Interactable" });

        protected void Start()
        {
            animator = GetComponent<Animator>();
            cc = GetComponent<CharacterController>();

            currentHealth = maxHealth;

            ragdollRigidbodies = rigObject.GetComponentsInChildren<Rigidbody>();
            SwitchRagdoll(false);

            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (!playerTransform) Debug.LogWarning("Can't find object with tag 'Player'. Enemy behavior disabled");
        }

        protected void Update()
        {
            if (currentHealth <= 0) return;

            // Gravity
            cc.Move(Vector3.down * gravityMultiplier * Time.deltaTime);

            // Turn healthbar towards main camera
            if (healthBar.activeInHierarchy)
            {
                healthBar.transform.LookAt(headObjectPosition + (headObjectPosition - Tt_Helpers.MainCamera.transform.position).normalized);
            }

            if (!playerTransform) return;
            if (isStunned) return;

            positionDifferenceToPlayer = playerTransform.position - headObjectPosition;
            distanceToPlayer = positionDifferenceToPlayer.magnitude;

            // Raycast towards player to see if they're directly visible
            bool playerRayHit = Physics.Raycast(headObjectPosition, positionDifferenceToPlayer.normalized, out RaycastHit playerRayResult, aggroDistance, rayMask);

            isPlayerVisible = playerRayHit && playerRayResult.transform.CompareTag("Player");

            CheckForAggro();

            if (isAggrevated)
                AggroBehavior();
            else
                IdleBehavior();
        }

        protected virtual bool CheckForAggro()
        {
            if (!isAggrevated && isPlayerVisible)
            {
                isAggrevated = true;
                canAttack = true;
                return true;
            }

            return false;
        }

        protected virtual void AggroBehavior()
        {
            if (!isAggrevated) return;

            lookDirection = Vector3.Lerp(lookDirection, new Vector3(positionDifferenceToPlayer.x, 0, positionDifferenceToPlayer.z), Time.deltaTime * lookSmoothSpeed);
            transform.LookAt(transform.position + lookDirection);

            if (distanceToPlayer > followPlayerThreshold) isWalking = true;
            if (distanceToPlayer < stopFollowPlayerThreshold) isWalking = false;
            animator.SetBool("walking", isWalking);

            moveVector = isWalking ? transform.forward * moveSpeed : Vector3.zero;

            cc.Move(moveVector * Time.deltaTime);

            if (isPlayerVisible && canAttack)
            {
                canAttack = false;

                switch (attackIntervalType)
                {
                    case AttackIntervalType.BeforeAttack:
                        coroutines.Add(StartCoroutine(Tt_Helpers.DelayExecute(() => Attack(true), attackInterval)));
                        break;

                    case AttackIntervalType.AfterAttack:
                        Attack(false);
                        StartCoroutine(Tt_Helpers.DelayExecute(ResetAttack, attackInterval));
                        break;
                }
            }
        }

        protected virtual void IdleBehavior()
        {
            lookDirection = transform.forward;
        }

        protected virtual void Attack(bool resetAttack)
        {
            if (resetAttack) ResetAttack();
        }

        protected void ResetAttack()
        {
            canAttack = true;
        }

        public override void TakeDamage(float damage, float knockback, GameObject hitLimb, Vector3 hitPoint)
        {
            // Check for critical hits
            if (hitLimb && criticalObjects.Contains(hitLimb))
            {
                damage *= 5;
            }

            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;

            Vector3 camToHit = hitPoint - Tt_Helpers.MainCamera.transform.position;

            // Blood PFX
            ParticleSystem ps = Instantiate(bloodPFX, hitPoint, Quaternion.LookRotation(camToHit, Vector3.up));
            ps.Play();
            StartCoroutine(Tt_Helpers.DelayDestroy(ps.gameObject, ps.main.startLifetime.constant));

            // Bullet wound decal
            GameObject decal = Tt_ObjectPooler.Instance.GetPoolObjectByPoolName("Bullet Wound");
            decal.transform.position = hitPoint - camToHit.normalized * 0.1f;
            decal.transform.rotation = Quaternion.LookRotation(camToHit, Vector3.up);
            decal.transform.parent = hitLimb ? hitLimb.transform : rigObject.transform.GetChild(0);
            StartCoroutine(Tt_Helpers.DelayExecute(() => decal.SetActive(false), 30));

            // Blood splatter decal
            if (Physics.Raycast(hitPoint, camToHit, out RaycastHit splatRayHit, 4, 1))
            {
                //GameObject splatDecal = Tt_ObjectPooler.Instance.GetPoolObjectByPoolName("Blood Splatter");

                // TODO: Idk maybe instantiate like a shit load of these but don't like pool them. They should stick around
                GameObject splatDecal = Instantiate(Tt_ObjectPooler.Instance.GetPoolObjectByPoolName("Blood Splatter"));

                splatDecal.transform.position = splatRayHit.point - camToHit.normalized * .3f;
                splatDecal.transform.rotation = Quaternion.LookRotation(camToHit, Vector3.up);
                splatDecal.transform.SetParent(splatRayHit.transform);


                //StartCoroutine(Tt_Helpers.DelayExecute(() => splatDecal.SetActive(false), 90));
            }
            
            if (currentHealth <= 0) { Death(knockback, hitLimb, hitPoint, camToHit); return; }

            Vector3 scale = healthBarForeground.transform.localScale;
            scale.x = currentHealth / maxHealth;
            healthBarForeground.transform.localScale = scale;
        }

        protected void DisableHealthBar()
        {
            healthBar.SetActive(false);
        }

        public override void Death(float knockback, GameObject hitLimb, Vector3 hitPoint, Vector3 camToHit)
        {
            // Set health to 0 in case death is called without taking damage (e.g. after being stunned for a moment)
            currentHealth = 0;

            DisableHealthBar();
            SwitchRagdoll(true);

            hitLimb?.GetComponent<Rigidbody>().AddForceAtPosition(camToHit.normalized * knockback * 2, hitPoint, ForceMode.Impulse);
            StopAllLocalCoroutines();
        }

        public void StopAllLocalCoroutines()
        {
            foreach (Coroutine c in coroutines)
            {
                StopCoroutine(c);
            }
            coroutines.Clear();
        }

        void SwitchRagdoll(bool state)
        {
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                rb.isKinematic = !state;
                //rb.gameObject.GetComponent<Collider>().isTrigger = !state;
            }

            animator.enabled = !state;
            cc.enabled = !state;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position + lookDirection, .5f);
        }
    }
}