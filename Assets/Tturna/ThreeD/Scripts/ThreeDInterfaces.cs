using UnityEngine;

namespace Tturna.ThreeD
{
    interface IDamageable
    {
        void TakeDamage(float amount = 0, float knockback = 0, GameObject hitObject = null, Vector3 hitPoint = default(Vector3));
    }

    interface IHealth : IDamageable
    {
        void Death(float knockback, GameObject hitLimb, Vector3 hitPoint, Vector3 camToHit);
    }
}