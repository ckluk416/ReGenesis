using System.Collections.Generic;
using UnityEngine;

namespace ReGenesis
{
    // Ordered waypoints from the spawn gate to the Energy Core.
    public class EnemyPath : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new List<Transform>();
        [SerializeField] private Color gizmoColor = new Color(1f, 0.55f, 0f);

        public int Count => waypoints.Count;

        public Vector3 GetPosition(int index)
        {
            return waypoints[index].position;
        }

        // Runs when the component is first added, so a Waypoints object fills itself from WP_00, WP_01, ...
        void Reset()
        {
            CollectChildWaypoints();
        }

        [ContextMenu("Collect Child Waypoints")]
        void CollectChildWaypoints()
        {
            waypoints.Clear();
            foreach (Transform child in transform)
                waypoints.Add(child);
        }

        void Awake()
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null)
                    Debug.LogWarning($"EnemyPath on {name} has an empty waypoint slot at index {i}.", this);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null)
                    continue;

                Gizmos.DrawWireSphere(waypoints[i].position, 0.25f);

                if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
