using UnityEngine;
using UnityEngine.UI;
using Tturna.Utility;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] InputField sensitivityField;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Image blackOverlay;

    // Utility
    Color overlayColor;

    private void Start()
    {
        if (pauseMenu.activeInHierarchy) SwitchPauseMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPauseMenu();
        }
    }

    public void SwitchPauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeInHierarchy);

        if (pauseMenu.activeInHierarchy)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void FadeToBlack(float time, System.Action callback)
    {
        StartCoroutine(Tt_Helpers.ExecuteOverTime((t) => { overlayColor.a = 1 - t / time; blackOverlay.color = overlayColor; }, 0, time));
        StartCoroutine(Tt_Helpers.DelayExecute(callback, time));
    }

    // Called by UI button
    public void QuitGame()
    {
        GameController.instance.QuitGame();
    }

    public void OnSensitivitySliderChanged()
    {
        sensitivityField.text = sensitivitySlider.value.ToString();
        GameController.instance.ChangeSensitivity((int)sensitivitySlider.value);
    }
    public void OnSensitivityFieldChanged()
    {
        int value = int.Parse(sensitivityField.text);
        sensitivitySlider.value = value;
        GameController.instance.ChangeSensitivity(value);
    }

    public void SetSensitivityWidgets(int value)
    {
        sensitivityField.text = value.ToString();
        sensitivitySlider.value = value;
    }
}
