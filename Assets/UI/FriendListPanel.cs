using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class FriendListPanel : MonoBehaviour
{
    public static FriendListPanel instance;

    [Header("UI References")]
    public GameObject panelRoot;
    public Transform contentContainer;
    public GameObject friendEntryPrefab;
    public GameObject emptyStateText;

    [Header("Settings")]
    public float updateInterval = 1f;

    private Camera arCamera;
    private Coroutine updateCoroutine;

    // We'll store active entries to avoid destroying/reinstantiating every second
    private Dictionary<int, FriendListEntry> activeEntries = new Dictionary<int, FriendListEntry>();

    private void Awake()
    {
        if (instance == null) instance = this;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void Start()
    {
        arCamera = Camera.main;
    }

    public void TogglePanel()
    {
        if (panelRoot == null) return;

        bool willShow = !panelRoot.activeSelf;
        panelRoot.SetActive(willShow);

        if (willShow)
        {
            RefreshList();
            if (updateCoroutine != null) StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(UpdateDistancesRoutine());
        }
        else
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }
    }

    private void RefreshList()
    {
        if (contentContainer == null || friendEntryPrefab == null) return;

        PlayerSync[] allPlayers = Object.FindObjectsByType<PlayerSync>(FindObjectsSortMode.None);
        List<PlayerSync> remotePlayers = new List<PlayerSync>();

        foreach (var p in allPlayers)
        {
            if (p.photonView != null && !p.photonView.IsMine)
            {
                remotePlayers.Add(p);
            }
        }

        if (remotePlayers.Count == 0)
        {
            if (emptyStateText != null) emptyStateText.SetActive(true);
            ClearAllEntries();
            return;
        }

        if (emptyStateText != null) emptyStateText.SetActive(false);

        // Keep track of which IDs are still present
        HashSet<int> currentViewIDs = new HashSet<int>();

        foreach (var p in remotePlayers)
        {
            int viewID = p.photonView.ViewID;
            currentViewIDs.Add(viewID);

            if (!activeEntries.ContainsKey(viewID))
            {
                GameObject newEntryObj = Instantiate(friendEntryPrefab, contentContainer);
                FriendListEntry entry = newEntryObj.GetComponent<FriendListEntry>();
                if (entry != null)
                {
                    entry.Setup(p, arCamera);
                    activeEntries[viewID] = entry;
                }
            }
        }

        // Remove old entries
        List<int> toRemove = new List<int>();
        foreach (var kvp in activeEntries)
        {
            if (!currentViewIDs.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (int id in toRemove)
        {
            if (activeEntries[id] != null && activeEntries[id].gameObject != null)
            {
                Destroy(activeEntries[id].gameObject);
            }
            activeEntries.Remove(id);
        }
    }

    private void ClearAllEntries()
    {
        foreach (var kvp in activeEntries)
        {
            if (kvp.Value != null && kvp.Value.gameObject != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        activeEntries.Clear();
    }

    private IEnumerator UpdateDistancesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            RefreshList();
            
            foreach (var kvp in activeEntries)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.UpdateDistance();
                }
            }
        }
    }
}
