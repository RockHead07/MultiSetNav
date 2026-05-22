using UnityEngine;

public class NavigationUIExtension : MonoBehaviour
{
    public static NavigationUIExtension instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void StartPlayerNavigation(string playerName)
    {
        // Access the REAL NavigationUIController from the package (MultiSet-SDK.Core)
        var navUI = FindObjectOfType<NavigationUIController>();
        if (navUI != null)
        {
            // ShowNavigationUIElements is private, so access public fields directly
            navUI.navigationProgressSlider.SetActive(true);
            navUI.stopButton.SetActive(true);
        }
        else
        {
            Debug.LogError("NavigationUIController not found in scene!");
        }
    }

    public void StopPlayerNavigation()
    {
        var navUI = FindObjectOfType<NavigationUIController>();
        if (navUI != null)
        {
            navUI.navigationProgressSlider.SetActive(false);
            navUI.stopButton.SetActive(false);
        }
    }
}
