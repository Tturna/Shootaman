using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Tt_RandomDecalChooser : MonoBehaviour
{
    [SerializeField] Material[] possibleDecals;

    void Start()
    {
        GetComponent<DecalProjector>().material = possibleDecals[Random.Range(0, possibleDecals.Length)];
        possibleDecals = null; // Epic memory optimization?
    }
}
