using UnityEngine;
using System.IO;
using System.Text;

namespace IEES
{
    public class LatencyLogger : MonoBehaviour
    {
        private string logPath;
        private StringBuilder buffer = new StringBuilder();

        void Awake()
        {
            logPath = Application.persistentDataPath + "/latency_unity.csv";
            buffer.AppendLine("event_id,zone_id,t4_api_received," +
                              "t5_navmesh_done,t6_ar_updated," +
                              "t_navmesh_ms,t_ar_ms,t_total_client_ms,is_activate");
        }

        public void LogTrigger(string zoneId, float t4, float t5,
                               float t6, bool isActivate)
        {
            string line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}," +
                          $"{zoneId},{t4},{t5},{t6}," +
                          $"{(t5-t4)*1000:F2}," +
                          $"{(t6-t5)*1000:F2}," +
                          $"{(t6-t4)*1000:F2}," +
                          $"{isActivate}";
            buffer.AppendLine(line);
            File.WriteAllText(logPath, buffer.ToString());
            Debug.Log($"[LatencyLogger] {line}");
        }
    }
}
