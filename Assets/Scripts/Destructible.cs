using Tturna.ThreeD;
using UnityEngine;

public class Destructible : MonoBehaviour, IDamageable
{
    [SerializeField] bool destroyParent;
    [SerializeField] bool hideParent;

    public void TakeDamage(float amount = 0, float knockback = 0, GameObject hitObject = null, Vector3 hitPoint = default)
    {
        Destruct();
    }

    public void Destruct()
    {
        Transform child = transform.GetChild(0);
        child.gameObject.SetActive(true);

        if (destroyParent)
        {
            child.SetParent(transform.parent);
            Destroy(gameObject);
            return;
        }

        if (hideParent)
        {
            GetComponent<Renderer>().enabled = false;
        }
    }
}
