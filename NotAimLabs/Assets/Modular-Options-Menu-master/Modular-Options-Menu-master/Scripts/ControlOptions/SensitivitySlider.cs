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
                // Get sens from PlayerPrefs 
                float currentSens;

                if (PlayerPrefs.HasKey(SENSITIVITY_PREF_KEY))
                {
                    currentSens = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY);
                    Debug.Log($"Slider - Loading saved sensitivity from PlayerPrefs: {currentSens}");

                    if(cameraController.mouseSensitivity != currentSens)
                    {
                        cameraController.mouseSensitivity = currentSens;
                    }

                }
                else
                {
                    currentSens = cameraController.mouseSensitivity;
                    Debug.Log($"Slider - No saved value, using controller sensitivity: {currentSens}");
                }

                updatingValue = true;
                slider.value = currentSens;
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


                PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, value);
                PlayerPrefs.Save();
            }
        }
    }
}