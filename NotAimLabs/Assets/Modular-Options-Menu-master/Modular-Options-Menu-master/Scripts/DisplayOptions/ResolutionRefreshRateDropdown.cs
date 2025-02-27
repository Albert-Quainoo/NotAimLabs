using System.Collections.Generic;
using UnityEngine;

namespace ModularOptions
{
    /// <remarks>
    /// Unity saves screen settings by default, so this class doesn't need anything from the Option base classes.
    /// Controlling refresh-rate separate from resolution is possible, but significantly more complex
    /// as not all refreshrates can be achieved at all resolutions.
    /// For example: some screens can do 3840x2160@60Hz and 1920x1080@120Hz, but cannot do 3840x2160@120Hz.
    /// Minor bug: wrong dropdown value is shown until you change resolution if you exit fullscreen mode and exit the application.
    /// </remarks>
    [AddComponentMenu("Modular Options/Display/Resolution & RefreshRate Dropdown")]
#if TMP_PRESENT
	[RequireComponent(typeof(TMPro.TMP_Dropdown))]
#else
    [RequireComponent(typeof(UnityEngine.UI.Dropdown))]
#endif
    public sealed class ResolutionRefreshRateDropdown : MonoBehaviour
    {

        Resolution[] resolutions;
#if TMP_PRESENT
		TMPro.TMP_Dropdown dropdown;
#else
        UnityEngine.UI.Dropdown dropdown;
#endif

        void Awake()
        {
#if TMP_PRESENT
			dropdown = GetComponent<TMPro.TMP_Dropdown>();
#else
            dropdown = GetComponent<UnityEngine.UI.Dropdown>();
#endif
            UpdateResolutions();

            dropdown.onValueChanged.AddListener((int _) => OnValueChange(_)); //UI classes use Unity events, requiring delegates (delegate() { OnValueChange(); }) or lambda expressions (() => OnValueChange()). Listeners are not persistent, so no need to unsub
        }

        void UpdateResolutions()
        { //TODO: OnResolutionChange event to refresh list for supporting dynamic screen swithing?
            resolutions = Screen.resolutions;
            List<string> options = new List<string>();
            int currentResIndex = 0;
            Resolution currentRes = Screen.currentResolution;
            currentRes.width = Screen.width; //Overwrite width/height with screen values to get correct values in Windowed mode
            currentRes.height = Screen.height;

            for (int i = 0, len = resolutions.Length; i < len; i++)
            {
                // Create a formatted string with refreshRateRatio instead of refreshRate
                // Cast double to float
                float refreshRate = (float)resolutions[i].refreshRateRatio.value;
                string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + refreshRate.ToString("F2") + "Hz";
                options.Add(option);

                if (ResolutionsMatch(resolutions[i], currentRes))
                    currentResIndex = i;
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = currentResIndex;
            dropdown.RefreshShownValue();
        }

        // Helper method to compare resolutions accounting for refreshRateRatio
        bool ResolutionsMatch(Resolution a, Resolution b)
        {
            return a.width == b.width &&
                   a.height == b.height &&
                   Mathf.Approximately((float)a.refreshRateRatio.value, (float)b.refreshRateRatio.value);
        }

        void OnValueChange(int _resolutionIndex)
        {
            var res = resolutions[_resolutionIndex];
            // Use the new SetResolution method with RefreshRate
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
        }
    }
}