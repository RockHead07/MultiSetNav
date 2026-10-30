using System.Collections.Generic;
using UnityEngine;

namespace IEES
{
    [RequireComponent(typeof(LineRenderer))]
    public class ARRouteVisualizer : MonoBehaviour
    {
        [Header("Line Styling")]
        [SerializeField] private Color pathColor = new Color(0f, 0.8f, 1f, 1f); // Neon Cyan for premium UI
        [SerializeField] private float lineWidth = 0.15f;
        [SerializeField] private Material lineMaterial;

        [Header("AR Directional Indicators")]
        [SerializeField] private GameObject arrowPrefab; // Prefab with arrow mesh
        [SerializeField] private float arrowSpacing = 1.0f; // Space between arrows in meters
        [SerializeField] private float hoverHeight = 0.05f; // Height above the floor

        private LineRenderer lineRenderer;
        private List<GameObject> instantiatedArrows = new List<GameObject>();

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            ConfigureLineRenderer();
        }

        private void ConfigureLineRenderer()
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;
            
            // Set smooth neon colors
            lineRenderer.startColor = pathColor;
            lineRenderer.endColor = pathColor;
            
            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }
            else
            {
                // Fallback to simple shader if no material is provided
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        /// <summary>
        /// Renders the path line and places directional arrows pointing towards the next waypoints.
        /// </summary>
        public void RenderRoute(Vector3[] corners)
        {
            ClearRoute();

            if (corners == null || corners.Length < 2) return;

            // 1. Draw line using LineRenderer
            lineRenderer.positionCount = corners.Length;
            for (int i = 0; i < corners.Length; i++)
            {
                // Hover line slightly above floor to prevent z-fighting
                Vector3 point = corners[i] + Vector3.up * hoverHeight;
                lineRenderer.SetPosition(i, point);
            }

            // 2. Instantiate and align direction arrows
            if (arrowPrefab != null)
            {
                SpawnPathArrows(corners);
            }
        }

        /// <summary>
        /// Spawns arrows along the calculated segment paths at regular intervals.
        /// </summary>
        private void SpawnPathArrows(Vector3[] corners)
        {
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[i + 1];
                
                float segmentLength = Vector3.Distance(start, end);
                Vector3 direction = (end - start).normalized;
                
                // Avoid placing arrows too close together
                float currentDist = 0.5f; // Initial offset from corner
                
                while (currentDist < segmentLength)
                {
                    Vector3 position = start + (direction * currentDist) + (Vector3.up * hoverHeight);
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    
                    GameObject arrow = Instantiate(arrowPrefab, position, rotation, this.transform);
                    instantiatedArrows.Add(arrow);
                    
                    currentDist += arrowSpacing;
                }
            }
        }

        /// <summary>
        /// Clears the rendered line and destroys all generated arrow indicators.
        /// </summary>
        public void ClearRoute()
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }

            foreach (var arrow in instantiatedArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            instantiatedArrows.Clear();
        }
    }
}
