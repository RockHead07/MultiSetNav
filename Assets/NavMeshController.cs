using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace IEES
{
    [System.Serializable]
    public class ZoneObstacleMapping
    {
        public string zoneId;
        public NavMeshObstacle obstacle; // Unity NavMeshObstacle component for carving
        public string zoneFriendlyName;
    }

    public class NavMeshController : MonoBehaviour
    {
        [Header("NavMesh Mapping")]
        [SerializeField] private List<ZoneObstacleMapping> zoneMappings = new List<ZoneObstacleMapping>();
        
        [Header("Agent & Target Settings")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform[] emergencyExits; // Exits to route towards

        [Header("Path Rendering")]
        [SerializeField] private ARRouteVisualizer routeVisualizer;

        private NavMeshPath calculatedPath;
        private Dictionary<string, NavMeshObstacle> obstacleDict = new Dictionary<string, NavMeshObstacle>();
        private HashSet<string> currentlyBlockedZones = new HashSet<string>();

        void Awake()
        {
            calculatedPath = new NavMeshPath();
            
            // Build fast lookup dictionary
            foreach (var mapping in zoneMappings)
            {
                if (mapping.obstacle != null && !string.IsNullOrEmpty(mapping.zoneId))
                {
                    obstacleDict[mapping.zoneId] = mapping.obstacle;
                    // Ensure carving is enabled
                    mapping.obstacle.carving = true;
                }
            }
        }

        void Start()
        {
            if (routeVisualizer == null)
            {
                routeVisualizer = FindObjectOfType<ARRouteVisualizer>();
            }
            RecalculateEvacuationRoute();
        }

        /// <summary>
        /// Returns whether the zone's obstacle is currently active (i.e. blocking the NavMesh).
        /// </summary>
        public bool IsActive(string zoneId)
        {
            return currentlyBlockedZones.Contains(zoneId);
        }

        /// <summary>
        /// Activates (enables) the NavMeshObstacle for the specified zone and recalculates routes.
        /// </summary>
        public void ActivateObstacle(string zoneId)
        {
            if (obstacleDict.TryGetValue(zoneId, out NavMeshObstacle obstacle))
            {
                if (!currentlyBlockedZones.Contains(zoneId))
                {
                    currentlyBlockedZones.Add(zoneId);
                    obstacle.enabled = true;
                    Debug.Log($"[NavMeshController] Activating obstacle for zone {zoneId}.");
                    RecalculateEvacuationRoute();
                }
            }
            else
            {
                Debug.LogWarning($"[NavMeshController] No obstacle found for zone {zoneId}.");
            }
        }

        /// <summary>
        /// Deactivates (disables) the NavMeshObstacle for the specified zone and recalculates routes.
        /// </summary>
        public void DeactivateObstacle(string zoneId)
        {
            if (obstacleDict.TryGetValue(zoneId, out NavMeshObstacle obstacle))
            {
                if (currentlyBlockedZones.Contains(zoneId))
                {
                    currentlyBlockedZones.Remove(zoneId);
                    obstacle.enabled = false;
                    Debug.Log($"[NavMeshController] Deactivating obstacle for zone {zoneId}.");
                    RecalculateEvacuationRoute();
                }
            }
            else
            {
                Debug.LogWarning($"[NavMeshController] No obstacle found for zone {zoneId}.");
            }
        }

        /// <summary>
        /// Calculates the best path to the nearest unblocked exit using Unity's NavMesh API.
        /// </summary>
        [ContextMenu("Recalculate Route")]
        public void RecalculateEvacuationRoute()
        {
            if (startPoint == null)
            {
                startPoint = this.transform;
            }

            if (emergencyExits == null || emergencyExits.Length == 0)
            {
                Debug.LogWarning("[NavMeshController] No exits configured for navigation routing.");
                return;
            }

            NavMeshPath bestPath = null;
            float shortestDistance = float.MaxValue;
            Transform selectedExit = null;

            foreach (var exit in emergencyExits)
            {
                if (exit == null) continue;

                NavMeshPath path = new NavMeshPath();
                // Calculate path from agent position to exit
                if (NavMesh.CalculatePath(startPoint.position, exit.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        float dist = GetPathLength(path);
                        if (dist < shortestDistance)
                        {
                            shortestDistance = dist;
                            bestPath = path;
                            selectedExit = exit;
                        }
                    }
                }
            }

            if (bestPath != null && routeVisualizer != null)
            {
                Debug.Log($"[NavMeshController] Evacuation path found to {selectedExit.name}. Length: {shortestDistance:F2} meters.");
                routeVisualizer.RenderRoute(bestPath.corners);
            }
            else
            {
                Debug.LogError("[NavMeshController] EVACUATION PATH BLOCKED! No safe route to any exit could be resolved.");
                if (routeVisualizer != null)
                {
                    routeVisualizer.ClearRoute();
                }
            }
        }

        private float GetPathLength(NavMeshPath path)
        {
            if (path.corners.Length < 2) return 0f;
            
            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return length;
        }
    }
}
