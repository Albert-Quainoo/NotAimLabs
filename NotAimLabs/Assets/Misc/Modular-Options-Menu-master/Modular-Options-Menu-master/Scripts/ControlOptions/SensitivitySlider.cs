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

            float initialSensitivity = cameraController.mouseSensitivity;
            float roundedInitialSensitivity = RoundToStepSize(initialSensitivity);
            roundedInitialSensitivity = Mathf.Clamp(roundedInitialSensitivity, minSensitivity, maxSensitivity);

            internalValueUpdate = true;
            slider.value = roundedInitialSensitivity;
            UpdateValueDisplay(roundedInitialSensitivity);
            internalValueUpdate = false;

            if (!Mathf.Approximately(initialSensitivity, roundedInitialSensitivity))
            {
                Debug.Log($"SensitivitySlider: Initial FPC sensitivity ({initialSensitivity:F4}) was not on a step. Adjusting to {roundedInitialSensitivity:F4} and saving.");
                cameraController.mouseSensitivity = roundedInitialSensitivity;
            }

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
                float steppedValue = RoundToStepSize(rawValueFromSlider);
                steppedValue = Mathf.Clamp(steppedValue, minSensitivity, maxSensitivity);

                cameraController.mouseSensitivity = steppedValue;
                UpdateValueDisplay(steppedValue);

                internalValueUpdate = true;
                slider.SetValueWithoutNotify(steppedValue);
                internalValueUpdate = false;
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
                float currentValue = slider.value;
                float steppedValue = RoundToStepSize(currentValue);
                steppedValue = Mathf.Clamp(steppedValue, minSensitivity, maxSensitivity);

                string formatString = "F" + decimalPlaces.ToString();
                Debug.Log(string.Format("Force update to sensitivity: {0:" + formatString + "}", steppedValue));

                cameraController.mouseSensitivity = steppedValue;
                UpdateValueDisplay(steppedValue);

                internalValueUpdate = true;
                slider.SetValueWithoutNotify(steppedValue);
                internalValueUpdate = false;
            }
        }
    }
}