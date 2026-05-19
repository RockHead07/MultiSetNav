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

        // Stop any existing POI navigation
        if (navController != null)
        {
            navController.StopNavigation();
        }

        playerTarget = targetTransform;
        isNavigatingToPlayer = true;

        if (navController != null && navController.agent != null)
        {
            // Set the agent's destination immediately
            navController.agent.destination = playerTarget.position;

            // Start ShowPath tracking from the agent to the player
            ShowPath.instance.SetPositionFrom(navController.agent.transform);
            ShowPath.instance.SetPositionTo(playerTarget);
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
