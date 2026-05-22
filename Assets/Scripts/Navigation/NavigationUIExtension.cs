using UnityEngine;
public class NavigationUIExtension : MonoBehaviour
{
    public static NavigationUIExtension instance;
    private NavigationUIController _cachedNavUI;
    
    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
    }
    
    private void Start()
    {
        // Cache once at start instead of every frame
        _cachedNavUI = FindObjectOfType<NavigationUIController>();
        if (_cachedNavUI == null)
            Debug.LogWarning("NavigationUIExtension: NavigationUIController not found!");
    }
    
    private void Update()
    {
        if (NavigationControllerExtension.instance != null && 
            NavigationControllerExtension.instance.isNavigatingToPlayer)
        {
            if (_cachedNavUI != null && _cachedNavUI.remainingDistance != null)
            {
                int distance = PathEstimationUtils.instance != null ? 
                    PathEstimationUtils.instance.getRemainingDistanceMeters() : 0;
                _cachedNavUI.remainingDistance.text = distance + " m remaining";
            }
        }
    }
    
    public void StartPlayerNavigation(string playerName)
    {
        if (_cachedNavUI == null) 
            _cachedNavUI = FindObjectOfType<NavigationUIController>();
            
        if (_cachedNavUI != null)
        {
            _cachedNavUI.navigationProgressSlider.SetActive(true);
            _cachedNavUI.stopButton.SetActive(true);
        }
        else
        {
            Debug.LogError("NavigationUIController not found in scene!");
        }
    }
    
    public void StopPlayerNavigation()
    {
        if (_cachedNavUI == null)
            _cachedNavUI = FindObjectOfType<NavigationUIController>();
            
        if (_cachedNavUI != null)
        {
            _cachedNavUI.navigationProgressSlider.SetActive(false);
            _cachedNavUI.stopButton.SetActive(false);
        }
    }
}
