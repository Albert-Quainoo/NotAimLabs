using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModularOptions
{
    [AddComponentMenu("Modular Options/Controls/Sensitivity Slider")]
    [RequireComponent(typeof(Slider))]
    public class SensitivitySlider : MonoBehaviour
    {
        public Slider slider;
        public FirstPersonController cameraController;
        public TextMeshProUGUI valueText;

        [Header("Sensitivity Slider settings")]
        public float minSensitivity = 0.1f;
        public float maxSensitivity = 20f;

        [Header("Decimal precision settings")]
        [Tooltip("Step size for slider increments. Value will snap to multiples of this.")]
        public float sliderStepSize = 0.1f;
        [Tooltip("Number of decimal places to display in the text.")]
        public int decimalPlaces = 2;

        [Header("Persistence")]
        [Tooltip("PlayerPrefs key to save/load sensitivity value")]
        public string sensitivityPrefsKey = "MouseSensitivity";

        private bool internalValueUpdate = false;

        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();
            if (cameraController == null)
                cameraController = FindAnyObjectByType<FirstPersonController>();
            if (valueText == null)
                valueText = GetComponentInChildren<TextMeshProUGUI>();

            if (slider == null) Debug.LogError("SensitivitySlider Error: Slider component not found!", this);
            if (cameraController == null) Debug.LogError("SensitivitySlider Error: FirstPersonController not found in scene!", this);
            if (valueText == null) Debug.LogWarning("SensitivitySlider Warning: TextMeshProUGUI for value display not found.", this);

            if (slider != null) slider.wholeNumbers = false;
        }

        private void Start()
        {
            if (cameraController == null || slider == null)
            {
                Debug.LogError("SensitivitySlider cannot initialize due to missing components.", this);
                enabled = false;
                return;
            }

            ConfigureSliderRange();
            LoadSavedSensitivity();
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void ConfigureSliderRange()
        {
            if (slider != null)
            {
                slider.minValue = minSensitivity;
                slider.maxValue = maxSensitivity;
            }
        }

        private void LoadSavedSensitivity()
        {
           
            float savedSensitivity;

            if (PlayerPrefs.HasKey(sensitivityPrefsKey))
            {
               
                savedSensitivity = PlayerPrefs.GetFloat(sensitivityPrefsKey);
                Debug.Log($"SensitivitySlider: Loaded sensitivity from PlayerPrefs: {savedSensitivity}");
            }
            else
            {
               
                savedSensitivity = cameraController.mouseSensitivity;
                Debug.Log($"SensitivitySlider: No saved value found, using controller value: {savedSensitivity}");

                
                PlayerPrefs.SetFloat(sensitivityPrefsKey, savedSensitivity);
                PlayerPrefs.Save();
            }

           
            savedSensitivity = Mathf.Clamp(savedSensitivity, minSensitivity, maxSensitivity);

            
            cameraController.mouseSensitivity = savedSensitivity;

           
            internalValueUpdate = true;
            slider.value = savedSensitivity;
            UpdateValueDisplay(savedSensitivity);
            internalValueUpdate = false;
        }

        private float RoundToStepSize(float value)
        {
            if (sliderStepSize <= 0) return value;
            return Mathf.Round(value / sliderStepSize) * sliderStepSize;
        }

        private void UpdateValueDisplay(float value)
        {
            if (valueText != null)
            {
                valueText.text = value.ToString($"F{decimalPlaces}");
            }
        }

        private void OnSliderValueChanged(float rawValueFromSlider)
        {
            if (internalValueUpdate) return;

            if (cameraController != null)
            {
                float finalValue = sliderStepSize > 0 ?
                    RoundToStepSize(rawValueFromSlider) :
                    rawValueFromSlider;

                finalValue = Mathf.Clamp(finalValue, minSensitivity, maxSensitivity);

                cameraController.mouseSensitivity = finalValue;

                PlayerPrefs.SetFloat(sensitivityPrefsKey, finalValue);
                PlayerPrefs.Save();

                UpdateValueDisplay(finalValue);

                if (!Mathf.Approximately(rawValueFromSlider, finalValue))
                {
                    internalValueUpdate = true;
                    slider.SetValueWithoutNotify(finalValue);
                    internalValueUpdate = false;
                }

                Debug.Log($"Sensitivity changed and saved: {finalValue}");
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
                float sensitivity = PlayerPrefs.HasKey(sensitivityPrefsKey) ?
                    PlayerPrefs.GetFloat(sensitivityPrefsKey) :
                    cameraController.mouseSensitivity;

                sensitivity = Mathf.Clamp(sensitivity, minSensitivity, maxSensitivity);
                cameraController.mouseSensitivity = sensitivity;

                string formatString = "F" + decimalPlaces.ToString();
                Debug.Log(string.Format("Force update to sensitivity: {0:" + formatString + "}", sensitivity));

                internalValueUpdate = true;
                slider.SetValueWithoutNotify(sensitivity);
                UpdateValueDisplay(sensitivity);
                internalValueUpdate = false;
            }
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat(sensitivityPrefsKey, cameraController.mouseSensitivity);
            PlayerPrefs.Save();
            Debug.Log($"Settings saved: Sensitivity = {cameraController.mouseSensitivity}");
        }

        public void LogCurrentValues()
        {
            if (cameraController != null)
            {
                float playerPrefsValue = PlayerPrefs.HasKey(sensitivityPrefsKey) ?
                    PlayerPrefs.GetFloat(sensitivityPrefsKey) : -1;

                Debug.Log($"Sensitivity values - Controller: {cameraController.mouseSensitivity}, " +
                          $"Slider: {slider.value}, PlayerPrefs: {playerPrefsValue}");
            }
        }
    }
}