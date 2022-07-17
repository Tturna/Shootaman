using System.Collections;
using UnityEngine;

public class Tt_ChildCameraController : MonoBehaviour
{
    public static Tt_ChildCameraController instance;

    Vector3 defaultPosition = Vector3.up;
    Vector3 currentPosition;

    private void Awake()
    {
        instance = this;
        currentPosition = defaultPosition;
    }

    public void CameraShake(float strength, float duration)
    {
        StartCoroutine(Shake(strength, duration));

        IEnumerator Shake(float strength, float duration)
        {
            while (duration > 0)
            {
                transform.localPosition = currentPosition + Random.insideUnitSphere * strength;
                duration -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            transform.localPosition = currentPosition;
        }
    }

    public void SetPosition(Vector3 pos)
    {
        currentPosition = pos;
        transform.localPosition = currentPosition;
    }

    public void ResetPosition()
    {
        currentPosition = defaultPosition;
        transform.localPosition = currentPosition;
    }
}
