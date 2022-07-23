using Tturna.Utility;
using UnityEngine;

namespace Tturna.ThreeD.Weapons
{
    public class Tt_GunEnemy : Tt_Enemy
    {
        [SerializeField] GameObject gun;

        Tt_Weapon goon;
        RaycastHit forwardRayHit;
        Vector3 shotDirection;

        private new void Start()
        {
            base.Start();
            goon = gun.GetComponent<Tt_Weapon>();
        }

        private new void Update()
        {
            base.Update();

            shotDirection = transform.forward * distanceToPlayer;
            shotDirection.y = positionDifferenceToPlayer.y;
            shotDirection.Normalize();

            Debug.DrawLine(headObjectPosition, headObjectPosition + shotDirection * 3, Color.red);
        }

        protected override bool CheckForAggro()
        {
            bool aggro = base.CheckForAggro();

            if (aggro) animator.SetLayerWeight(1, 1);
            return aggro;
        }

        protected override void Attack(bool resetAttack)
        {
            // Raycast forward but to player level
            if (!Physics.Raycast(headObjectPosition, shotDirection, out forwardRayHit, 20, rayMask))
            {
                forwardRayHit.point = headObjectPosition + shotDirection * 30;
            }

            if (!goon.Fire(headObjectPosition, forwardRayHit.transform, forwardRayHit.point, distanceToPlayer, false))
            {
                goon.Reload(() => { if (resetAttack) ResetAttack(); });
                animator.SetTrigger(name: "reload");
                return;
            }
            else if (resetAttack) ResetAttack();

            animator.SetTrigger("shoot");
        }

        public override void TakeDamage(float damage, float knockback, GameObject hitLimb, Vector3 hitPoint)
        {
            base.TakeDamage(damage, knockback, hitLimb, hitPoint);
            if (currentHealth <= 0) return;

            if (currentHealth <= maxHealth * 0.25f)
            {
                int rn = Random.Range(0, 3);
                if (rn <= 1)
                {
                    Debug.Log($"stun{rn + 1}");
                    animator.SetTrigger($"stun{rn + 1}");
                    currentHealth = 0;
                    StartCoroutine(Tt_Helpers.DelayExecute(() => Death(0f, null, Vector3.zero, Vector3.zero), 7));
                    return;
                }
            }

            int rn2 = Random.Range(1, 3);

            string trigger = $"hurt{rn2}";
            animator.SetTrigger(trigger);
        }

        public override void Death()
        {
            base.Death();

            // If the enemy is holding a gun, drop it
            if (gun && gun.transform.root == transform)
            {
                gun.transform.SetParent(null);
                gun.GetComponentInChildren<Animator>().enabled = false;
                gun.GetComponent<Collider>().enabled = true;
                gun.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headObjectPosition + shotDirection * 3, .5f);
        }
    }
}