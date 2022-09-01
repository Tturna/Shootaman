using UnityEngine;
using Tturna.Utility;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] UIController uiController;

    [Header("Settings")]
    [SerializeField, Range(0, 100)] int initialSensitivity;

    [Header("Monitoring")]
    public float sensitivity;

    public static GameController instance;

    private void Awake() => instance = this;

    private void Start()
    {
        sensitivity = initialSensitivity;
        uiController.SetSensitivityWidgets(initialSensitivity);
    }

    public void ChangeSensitivity(int value)
    {
        sensitivity = value;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            uiController.FadeToBlack(4, () => StartCoroutine(Tt_Helpers.DelayExecute(QuitGame, 2)));
        }
    }
}
