using Tturna.Interaction;
using UnityEngine;

public class GlorpMachine : Tt_Interactable
{
    [SerializeField] GameObject canPrefab;
    [SerializeField] Vector3 relativeCanSpawnPoint;

    public override void Interact(GameObject interactionSource)
    {
        GameObject can = Instantiate(canPrefab);

        Vector3 xzVector = new Vector3(relativeCanSpawnPoint.x, 0, relativeCanSpawnPoint.z);
        can.transform.localPosition = transform.position - transform.forward * xzVector.magnitude + Vector3.up * relativeCanSpawnPoint.y;
        can.transform.localRotation = Quaternion.Euler(Vector3.right * 90);

        // TODO: Make player punch or kick the machine
    }

    private void OnDrawGizmos()
    {
        Vector3 xzVector = new Vector3(relativeCanSpawnPoint.x, 0, relativeCanSpawnPoint.z);
        Gizmos.DrawWireSphere(transform.position - transform.forward * xzVector.magnitude + Vector3.up * relativeCanSpawnPoint.y, .2f);
    }
}
