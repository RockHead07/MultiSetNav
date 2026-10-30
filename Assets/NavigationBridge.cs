using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace IEES
{
    [System.Serializable]
    public class ZoneDensity
    {
        public string zone_id;
        public string level;
        public int count;
        public int h_count;
        public int l_count;
    }

    [System.Serializable]
    public class DensityResponse
    {
        public List<ZoneDensity> zones;
        public double t_server;
    }

    public class NavigationBridge : MonoBehaviour
    {
        [Header("Server Config")]
        public string serverUrl = "http://localhost:5000/density";
        public float  pollInterval = 1.0f;  // detik

        [Header("Hysteresis")]
        public int K = 2;  // cycle sebelum AKTIF obstacle
        public int M = 3;  // cycle sebelum NONAKTIF obstacle

        private Dictionary<string, int> hCount = new Dictionary<string, int>();
        private Dictionary<string, int> lCount = new Dictionary<string, int>();

        public NavMeshController navMeshCtrl;
        public LatencyLogger     latencyLogger;

        void Start()
        {
            if (navMeshCtrl == null)
            {
                navMeshCtrl = FindObjectOfType<NavMeshController>();
            }
            if (latencyLogger == null)
            {
                latencyLogger = FindObjectOfType<LatencyLogger>();
            }
            StartCoroutine(PollDensity());
        }

        IEnumerator PollDensity()
        {
            while (true)
            {
                float t4 = Time.realtimeSinceStartup;
                using var req = UnityWebRequest.Get(serverUrl);
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var data = JsonUtility.FromJson<DensityResponse>(req.downloadHandler.text);
                        if (data != null && data.zones != null)
                        {
                            ProcessZoneStates(data, t4);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[NavigationBridge] JSON parsing error: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[NavigationBridge] HTTP request failed: {req.error}");
                }
                yield return new WaitForSeconds(pollInterval);
            }
        }

        void ProcessZoneStates(DensityResponse data, float t4)
        {
            foreach (var zone in data.zones)
            {
                string id    = zone.zone_id;
                string level = zone.level;

                if (!hCount.ContainsKey(id)) 
                { 
                    hCount[id] = 0; 
                    lCount[id] = 0; 
                }

                if (level == "High")
                {
                    hCount[id]++; 
                    lCount[id] = 0;
                    if (hCount[id] >= K && navMeshCtrl != null && !navMeshCtrl.IsActive(id))
                    {
                        float t5 = Time.realtimeSinceStartup;
                        navMeshCtrl.ActivateObstacle(id);
                        float t6 = Time.realtimeSinceStartup;
                        if (latencyLogger != null)
                        {
                            latencyLogger.LogTrigger(id, t4, t5, t6, true);
                        }
                    }
                }
                else
                {
                    lCount[id]++; 
                    hCount[id] = 0;
                    if (lCount[id] >= M && navMeshCtrl != null && navMeshCtrl.IsActive(id))
                    {
                        navMeshCtrl.DeactivateObstacle(id);
                        if (latencyLogger != null)
                        {
                            latencyLogger.LogTrigger(id, t4, 0, 0, false);
                        }
                    }
                }
            }
        }
    }
}
