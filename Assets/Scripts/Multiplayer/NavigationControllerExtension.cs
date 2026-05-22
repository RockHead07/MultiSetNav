using UnityEngine;
using UnityEngine.AI;

public class NavigationControllerExtension : MonoBehaviour
{
    public static NavigationControllerExtension instance;

    public Transform playerTarget;
    public bool isNavigatingToPlayer;

    private NavigationController navController;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        navController = NavigationController.instance;
    }

    private void Update()
    {
        if (isNavigatingToPlayer && playerTarget != null && navController != null && navController.agent != null)
        {
            navController.agent.destination = playerTarget.position;
        }
    }

    public void SetTransformForNavigation(Transform targetTransform)
    {
        if (navController == null) navController = NavigationController.instance;
        
        Debug.Log($"SetTransformForNavigation called. " +
            $"navController: {navController != null}, " +
            $"ShowPath: {ShowPath.instance != null}, " +
            $"agent: {navController?.agent != null}, " +
            $"isOnNavMesh: {navController?.agent?.isOnNavMesh}");
        
        if (navController != null)
            navController.StopNavigation();
        
        playerTarget = targetTransform;
        isNavigatingToPlayer = true;
        
        if (navController?.agent != null && ShowPath.instance != null)
        {
            navController.agent.destination = playerTarget.position;
            ShowPath.instance.SetPositionFrom(navController.agent.transform);
            ShowPath.instance.SetPositionTo(playerTarget);
            Debug.Log("ShowPath initialized for player navigation");
        }
        else
        {
            Debug.LogError($"Cannot initialize ShowPath. " +
                $"ShowPath.instance: {ShowPath.instance != null}, " +
                $"agent: {navController?.agent != null}");
        }
    }

    public void StopPlayerNavigation()
    {
        isNavigatingToPlayer = false;
        playerTarget = null;
        
        // Reset path visualization
        if (ShowPath.instance != null)
        {
            ShowPath.instance.ResetPath();
        }
        
        if (PathEstimationUtils.instance != null)
        {
            PathEstimationUtils.instance.ResetEstimation();
        }
    }
}
