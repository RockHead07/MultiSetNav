using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfoPopup : MonoBehaviour
{
    public static PlayerInfoPopup instance;

    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_Text playerNameText;
    public TMP_Text distanceText;
    public Button navigateButton;
    public Button closeButton;

    private PlayerSync currentPlayer;
    private Camera arCamera;

    private void Awake()
    {
        if (instance == null) instance = this;

        if (navigateButton != null)
        {
            navigateButton.onClick.AddListener(OnNavigateClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void Start()
    {
        arCamera = Camera.main;
    }

    private void Update()
    {
        if (popupPanel != null && popupPanel.activeSelf && currentPlayer != null)
        {
            UpdateDistanceText();
        }
    }

    public void Show(PlayerSync player)
    {
        currentPlayer = player;
        
        if (playerNameText != null)
        {
            playerNameText.text = player.photonView.Owner != null ? player.photonView.Owner.NickName : "Unknown Player";
        }

        UpdateDistanceText();

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        currentPlayer = null;
    }

    private void UpdateDistanceText()
    {
        if (distanceText == null || arCamera == null || currentPlayer == null) return;

        Vector3 myPos = arCamera.transform.position;
        Vector3 targetPos = currentPlayer.transform.position;
        
        // Use horizontal distance
        myPos.y = 0;
        targetPos.y = 0;

        float distance = Vector3.Distance(myPos, targetPos);
        distanceText.text = $"Jarak: {distance:F1} m";
    }

    private void OnNavigateClicked()
    {
        if (currentPlayer != null && PlayerNavigationController.instance != null)
        {
            string pName = currentPlayer.photonView.Owner != null ? currentPlayer.photonView.Owner.NickName : "Player";
            PlayerNavigationController.instance.NavigateToPlayer(currentPlayer.transform, pName);
            
            // Auto-hide when navigation starts
            Hide();
        }
    }
}
