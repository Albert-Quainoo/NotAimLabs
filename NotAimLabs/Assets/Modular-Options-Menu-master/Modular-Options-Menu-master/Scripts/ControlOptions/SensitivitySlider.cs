using UnityEngine;
using UnityEngine.UI;

namespace ModularOptions {
	/// <summary>
	/// Changes the sensitivity of a camera control script.
	/// A simple reference FPP (First Person Perspective) camera rotation script is included.
	/// Just replace it with your own if it doesn't fit your use-case.
	/// </summary>
	[AddComponentMenu("Modular Options/Controls/Sensitivity Slider")]
	public class SensitivitySlider : SliderOption, ISliderDisplayFormatter {

	 public FirstPersonController cameraController;
	private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";

	private bool updatingValue = false;


        private void Start()
        {
            if (cameraController != null)
			{
				cameraController.OnSensitivityChanged += OnControllerSensitivityChanged;

				float savedSens = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY, cameraController.mouseSensitivity);

			updatingValue = true;
			Value = savedSens;
			cameraController.mouseSensitivity = savedSens;
			updatingValue = false;

			Debug.Log($"Slider Start - Initialised with sensitivity {savedSens}");
			}
			else
			{
				Debug.LogWarning("Camera controller not asssigned to the slider");
			}
        }

        private void OnDestroy()
        {
            if (cameraController != null)
			{
				cameraController.OnSensitivityChanged += OnControllerSensitivityChanged;

			}
        }

		private void OnControllerSensitivityChanged(float newValue)
		{
           if (!updatingValue) {
			updatingValue = true;
			Value = newValue;
			updatingValue = false;
			Debug.Log($"Slider Updated from controller change {newValue}");
		   }
		}
        private void Update ()
	   {
		if (cameraController != null && Input.GetKeyDown(KeyCode.F1))
		{
			Debug.Log($"Current sensitivity: {cameraController.mouseSensitivity}");
		}
	   }


#if UNITY_EDITOR
		/// <summary>
		/// Auto-assign editor reference, if suitable component is found.
		/// </summary>
          

		protected override void Reset(){
			cameraController = Camera.main.GetComponent<FirstPersonController>();
			base.Reset();
		}
#endif

		protected override void ApplySetting(float _value){
			if (cameraController != null  && !updatingValue) 
			{
              updatingValue = true;
			  float oldValue = cameraController.mouseSensitivity;
			  cameraController.mouseSensitivity = _value;
			  PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, _value);
			  PlayerPrefs.Save();
			  updatingValue = false;
			
			Debug.Log($"Sensitivity Changed to {oldValue:F1}, New:{_value:F1}");

			}
		
        }

		public string OverrideFormatting(float _value){
			return _value.ToString("F1");
		}
    }
}