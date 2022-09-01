// This script is designed to be inherited by objects that represent living creatures like players, enemies and NPCs

using UnityEngine;
using Tturna.Utility;

namespace Tturna.ThreeD
{
    [RequireComponent(typeof(Rigidbody))]
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
                Death(knockback, hitLimb, hitPoint, hitPoint - Tt_Helpers.MainCamera.transform.position);
            }
        }

        public virtual void AddHealth(float amount, GameObject source)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        // TODO:
        // It's probably better if the Death() function didn't have these specific parameters and instead the deriving classes would implement that shit
        public virtual void Death(float knockback, GameObject hitLimb, Vector3 hitPoint, Vector3 camToHit)
        {
            Debug.LogWarning($"GameObject {gameObject.name} has Agent script with no Death() function implemented.");
        }
    }
}