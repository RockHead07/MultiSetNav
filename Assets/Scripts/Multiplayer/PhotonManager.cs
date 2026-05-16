using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("Photon")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private bool autoConnect = true;

    [Header("Room")]
    [SerializeField] private string buildingId = "GedungA";
    [SerializeField] private string floorId = "Lt1";
    [SerializeField] private int maxPlayers = 20;

    [Header("Scene References")]
    [SerializeField] private Transform mapSpace;
    [Tooltip("AR Camera (from XR Origin). If null, Camera.main will be used")]
    [SerializeField] private Camera arCamera;

    [Header("Player Prefab")]
    [Tooltip("Prefab under Assets/Resources/")]
    [SerializeField] private string playerPrefabName = "PlayerPrefab";

    private bool localizationReady;

    void Start()
    {
        if (autoConnect)
        {
            Connect();
        }
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = GetDefaultNickname();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        TryJoinRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("[PhotonManager] Disconnected: " + cause);
    }

    public void NotifyLocalizationSucceeded(string building, string floor)
    {
        if (!string.IsNullOrWhiteSpace(building))
        {
            buildingId = building;
        }
        if (!string.IsNullOrWhiteSpace(floor))
        {
            floorId = floor;
        }

        localizationReady = true;
        TryJoinRoom();
    }

    private void TryJoinRoom()
    {
        if (!PhotonNetwork.IsConnected || !localizationReady)
        {
            return;
        }

        string roomName = GetRoomName();
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)Mathf.Clamp(maxPlayers, 1, 200)
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (mapSpace == null)
        {
            GameObject mapSpaceGo = GameObject.Find("Map Space");
            mapSpace = mapSpaceGo != null ? mapSpaceGo.transform : null;
        }

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main;
        }

        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, Vector3.zero, Quaternion.identity);
        playerObj.transform.SetParent(mapSpace, false);
        playerObj.transform.localPosition = Vector3.zero;
        playerObj.transform.localRotation = Quaternion.identity;

        PlayerSync sync = playerObj.GetComponent<PlayerSync>();
        if (sync != null)
        {
            sync.SetReferences(mapSpace, arCamera);
        }
    }

    private string GetRoomName()
    {
        return buildingId + "_" + floorId;
    }

    private string GetDefaultNickname()
    {
        string id = SystemInfo.deviceUniqueIdentifier ?? SystemInfo.deviceName ?? "XXXXXX";
        if (id.Length >= 6)
        {
            id = id.Substring(0, 6);
        }
        return "User_" + id;
    }
}
