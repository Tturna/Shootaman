/*  Script made by Tturna
 *  version 0.1
 *  
 *  This script is designed to be used as a component.
 *  The TturnaInteractor class looks for this component when trying to interact with the object this is attached to.
 */

using Tturna.ThreeD;
using UnityEngine;

namespace Tturna.Interaction
{
    public class Tt_Interactable : MonoBehaviour
    {
        public virtual void Activate(GameObject interactionSource) { }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            Transform root = collision.transform.root;
            if (!root.TryGetComponent(out Rigidbody rb)) return;
            if (!root.TryGetComponent(out Tt_Agent agent)) return;
            if (collision.impulse.magnitude < 8) return;

            agent.TakeDamage(10, 125, collision.gameObject, transform.position);
        }
    }
}
