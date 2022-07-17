using Tturna.Interaction;
using Tturna.ThreeD;
using UnityEngine;

public class Glorp : Tt_Interactable
{
    public override void Interact(GameObject interactionSource)
    {
        // Drink glorp for health(?)

        if (interactionSource.TryGetComponent(out Tt_Agent agent))
        {
            agent.AddHealth(25, gameObject);

            // TODO: Play drink animation
        }
    }
}
