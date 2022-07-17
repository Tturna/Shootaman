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
            }
            else if (resetAttack) ResetAttack();
        }

        public override void Death()
        {
            base.Death();

            // If the enemy is holding a gun, drop it
            if (gun && gun.transform.root == transform)
            {
                gun.transform.SetParent(null);
                gun.GetComponent<Animator>().enabled = false;
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