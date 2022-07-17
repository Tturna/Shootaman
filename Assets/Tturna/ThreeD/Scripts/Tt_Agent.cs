// This script is designed to be inherited by objects that represent living creatures like players, enemies and NPCs

using UnityEngine;

namespace Tturna.ThreeD
{
    public class Tt_Agent : MonoBehaviour, IHealth
    {
        public float currentHealth;
        public float maxHealth;

        public virtual void TakeDamage(float amount, float knockback, GameObject hitLimb, Vector3 hitPoint)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Death();
            }
        }

        public virtual void Death() { }

        public virtual void Death(float knockback, GameObject hitLimb, Vector3 hitPoint, Vector3 camToHit)
        {
            throw new System.NotImplementedException();
        }
    }
}