using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class PlayerNavigationController : MonoBehaviourPunCallbacks
{
    public static PlayerNavigationController instance;

    [Header("Navigation State")]
    public bool isNavigatingToPlayer = false;
    public string currentTargetPlayerName = "";
    private Transform playerTarget;

    [Header("Settings")]
    public float arrivalDistance = 1.5f;

    [Header("Events")]
    public UnityEvent OnArrived;

    private Camera arCamera;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        arCamera = Camera.main;
    }

    public void NavigateToPlayer(Transform playerCapsuleTransform, string playerName)
    {
        currentTargetPlayerName = playerName;
        playerTarget = playerCapsuleTransform;
        isNavigatingToPlayer = true;

        if (NavigationControllerExtension.instance != null)
        {
            NavigationControllerExtension.instance.SetTransformForNavigation(playerCapsuleTransform);
        }
    }

    public void StopNavigation()
    {
        isNavigatingToPlayer = false;
        playerTarget = null;
        currentTargetPlayerName = "";

        if (NavigationControllerExtension.instance != null)
        {
            NavigationControllerExtension.instance.StopPlayerNavigation();
        }
    }

    public float GetDistanceToTarget()
    {
        if (playerTarget == null || arCamera == null) return 0f;

        Vector3 myPos = arCamera.transform.position;
        Vector3 targetPos = playerTarget.position;
        
        // Use horizontal distance
        myPos.y = 0;
        targetPos.y = 0;

        return Vector3.Distance(myPos, targetPos);
    }

    private void Update()
    {
        if (isNavigatingToPlayer)
        {
            if (playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
            {
                StopNavigation();
                return;
            }
            
            float distance = GetDistanceToTarget();
            if (distance <= arrivalDistance)
            {
                // We have arrived
                OnArrived?.Invoke();
                StopNavigation();
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!isNavigatingToPlayer || playerTarget == null) return;
        
        // Check if the player we're navigating to has left
        PlayerSync targetSync = playerTarget.GetComponentInParent<PlayerSync>();
        if (targetSync != null && !targetSync.photonView.IsMine)
        {
            if (targetSync.photonView.Owner == otherPlayer)
            {
                StopNavigation();
                Debug.Log("Target player left the room. Navigation stopped.");
                // Optional: show toast/notification to user
            }
        }
    }
}
