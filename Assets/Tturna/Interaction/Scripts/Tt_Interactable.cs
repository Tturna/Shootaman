/*  This script is designed to be used as a component.
 *  The TturnaInteractor class looks for this component when trying to interact with the object this is attached to.
 */

using Tturna.ThreeD;
using UnityEngine;

namespace Tturna.Interaction
{
    public class Tt_Interactable : MonoBehaviour
    {
        public Sprite interactIndicatorIcon;

        public virtual void Interact(GameObject interactionSource) { }

        public virtual void Activate(GameObject interactionSource) { }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            IDamageable damageable;
            if (collision.transform.TryGetComponent(out damageable) || collision.transform.root.TryGetComponent(out damageable))
            {
                if (collision.impulse.magnitude < 8) return;

                damageable.TakeDamage(10, 125, collision.gameObject, transform.position);
            }
        }
    }
}
