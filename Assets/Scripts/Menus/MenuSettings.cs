using UnityEngine;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{
    public Slider sensitivitySlider;

    void Start()
    {
        // Load saved sensitivity, or default to 1
        float savedValue = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        sensitivitySlider.value = savedValue;

        // Listen for slider changes
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    public void SetSensitivity(float value)
    {
        // Save the new value to the computer
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }
}