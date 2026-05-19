using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerTapHandler : MonoBehaviour
{
    private Camera arCamera;

    private void Start()
    {
        arCamera = Camera.main;
    }

    private void Update()
    {
        if (arCamera == null) return;

        // Check for touches or clicks
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 inputPosition = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                inputPosition = Input.GetTouch(0).position;
            }

            // Prevent raycasting if tapping on UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Note: For touches, IsPointerOverGameObject might need the touch fingerId
                if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    return;
                }
                else if (Input.touchCount == 0)
                {
                    return;
                }
            }

            Ray ray = arCamera.ScreenPointToRay(inputPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if hit object or its parent has PlayerSync
                PlayerSync playerSync = hit.collider.GetComponentInParent<PlayerSync>();
                
                if (playerSync != null)
                {
                    // Ensure it's not the local player
                    if (!playerSync.photonView.IsMine)
                    {
                        if (PlayerInfoPopup.instance != null)
                        {
                            PlayerInfoPopup.instance.Show(playerSync);
                        }
                        else
                        {
                            Debug.LogWarning("PlayerInfoPopup instance is missing!");
                        }
                    }
                }
            }
        }
    }
}
