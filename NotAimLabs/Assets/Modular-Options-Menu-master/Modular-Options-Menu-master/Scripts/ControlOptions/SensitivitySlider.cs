using UnityEngine;
using UnityEngine.UI;

namespace ModularOptions
{
    [AddComponentMenu("Modular Options/Controls/Sensitivity Slider")]
    public class SensitivitySlider : MonoBehaviour
    {
        public Slider slider;
        public FirstPersonController cameraController;
        private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";
        private bool updatingValue = false;

        private void Start()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            if (cameraController == null)
                cameraController = FindFirstObjectByType<FirstPersonController>();

            if (cameraController != null)
            {
                // Set up initial values
                float savedSens = cameraController.mouseSensitivity;
                Debug.Log($"Slider - Loading saved sensitivity: {savedSens}");

                updatingValue = true;
                slider.value = savedSens;
                updatingValue = false;


                slider.onValueChanged.AddListener(OnSliderValueChanged);

            }

        }

        private void OnSliderValueChanged(float newValue)
        {
            if (cameraController != null && !updatingValue)
            {
                updatingValue = true;

                Debug.Log($"Slider changed to {newValue}");

                cameraController.mouseSensitivity = newValue;


                PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, newValue);
                PlayerPrefs.Save();

                updatingValue = false;

            }
        }

        private void OnDestroy()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        
        public void ForceUpdateSensitivity()
        {
            if (cameraController != null && slider != null)
            {
                float value = slider.value;
                Debug.Log($"Force update to sensitivity: {value}");
                cameraController.mouseSensitivity = value;
            }
        }
    }
}