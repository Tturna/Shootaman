using Tturna.Interaction;
using UnityEngine;

public class GlorpMachine : Tt_Interactable
{
    [SerializeField] GameObject canPrefab;
    [SerializeField] Vector3 relativeCanSpawnPoint;

    public override void Interact(GameObject interactionSource)
    {
        GameObject can = Instantiate(canPrefab);
        can.transform.localPosition = transform.position + relativeCanSpawnPoint;
        can.transform.localRotation = Quaternion.Euler(Vector3.right * 90);

        // TODO: Make player punch or kick the machine
    }
}
