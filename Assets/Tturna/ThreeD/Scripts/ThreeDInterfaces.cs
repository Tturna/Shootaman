using UnityEngine;

namespace Tturna.ThreeD
{
    interface IHealth
    {
        void TakeDamage(float amount, float knockback, GameObject hitLimb, Vector3 hitPoint);

        void Death();
        void Death(float knockback, GameObject hitLimb, Vector3 hitPoint, Vector3 camToHit);
    }
}