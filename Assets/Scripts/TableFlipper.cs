using UnityEngine;
using Tturna.Interaction;

public class TableFlipper : Tt_Interactable
{
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float angle = Mathf.Atan2(transform.forward.y, transform.forward.z) * Mathf.Rad2Deg;

        if (angle < -90 && angle > -100)
        {
            rb.AddRelativeTorque(Vector3.left * 20);
        }
        else if (angle > 90 && angle < 100)
        {
            rb.AddRelativeTorque(Vector3.right * 20);
        }
    }

    public override void Activate(GameObject interactionSource)
    {
        float distToForward = Vector3.Distance(interactionSource.transform.forward, transform.forward);
        float distToBackward = Vector3.Distance(interactionSource.transform.forward, -transform.forward);

        Vector3 rotationDirection = distToForward < distToBackward ? Vector3.right : Vector3.left;

        rb.AddForce(Vector3.up * 30, ForceMode.Impulse);
        rb.AddRelativeTorque(rotationDirection * 250, ForceMode.Impulse);
    }
}
