using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FriendListEntry : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text distanceText;
    public Button navigateButton;

    private PlayerSync targetPlayer;
    private Camera arCamera;
    private string pName;

    public void Setup(PlayerSync player, Camera cam)
    {
        targetPlayer = player;
        arCamera = cam;

        pName = player.photonView.Owner != null ? player.photonView.Owner.NickName : "Player";
        if (playerNameText != null)
        {
            playerNameText.text = pName;
        }

        if (navigateButton != null)
        {
            navigateButton.onClick.RemoveAllListeners();
            navigateButton.onClick.AddListener(OnNavigateClicked);
        }

        UpdateDistance();
    }

    public void UpdateDistance()
    {
        if (targetPlayer == null || arCamera == null || distanceText == null) return;

        Vector3 myPos = arCamera.transform.position;
        Vector3 targetPos = targetPlayer.transform.position;
        
        myPos.y = 0;
        targetPos.y = 0;

        float distance = Vector3.Distance(myPos, targetPos);
        distanceText.text = $"{distance:F1} m";
    }

    private void OnNavigateClicked()
    {
        if (targetPlayer != null && PlayerNavigationController.instance != null)
        {
            PlayerNavigationController.instance.NavigateToPlayer(targetPlayer.transform, pName);

            // Optional: Auto-close the panel after selecting a player
            if (FriendListPanel.instance != null)
            {
                FriendListPanel.instance.TogglePanel();
            }
        }
    }
}
