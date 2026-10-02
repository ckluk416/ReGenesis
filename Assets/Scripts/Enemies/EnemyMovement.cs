using System;
using UnityEngine;

namespace ReGenesis
{
    // Moves an enemy along an EnemyPath and damages the Energy Core at the end.
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private EnemyPath path;
        [SerializeField, Min(0f)] private float speed = 2f;
        [SerializeField, Min(0f)] private float turnSpeed = 540f;
        [Tooltip("Height above the waypoints. Use this for flying enemies such as the Malware Drone.")]
        [SerializeField] private float heightOffset;
        [SerializeField, Min(0f)] private float coreDamage = 10f;

        private EnemyHealth health;
        private int targetIndex;
        private bool finished;

        // Used by towers to find the enemy furthest along the path.
        public float DistanceTravelled { get; private set; }

        public event Action<EnemyMovement> ReachedEnd;

        void Awake()
        {
            health = GetComponent<EnemyHealth>();
        }

        public void SetPath(EnemyPath newPath)
        {
            path = newPath;
            targetIndex = 0;
            DistanceTravelled = 0f;
            finished = false;
        }

        void Update()
        {
            if (finished || path == null || path.Count == 0)
                return;
            if (health != null && !health.IsAlive)
                return;

            Vector3 position = transform.position;
            float step = speed * Time.deltaTime;

            // Carry leftover movement into the next segment so enemies do not pause at corners.
            while (step > 0f)
            {
                Vector3 target = WaypointPosition(targetIndex);
                Vector3 offset = target - position;
                float distance = offset.magnitude;

                if (distance > step)
                {
                    position += offset / distance * step;
                    DistanceTravelled += step;
                    FaceDirection(offset);
                    break;
                }

                position = target;
                DistanceTravelled += distance;
                step -= distance;
                targetIndex++;

                if (targetIndex >= path.Count)
                {
                    transform.position = position;
                    ArriveAtEnd();
                    return;
                }
            }

            transform.position = position;
        }

        Vector3 WaypointPosition(int index)
        {
            return path.GetPosition(index) + Vector3.up * heightOffset;
        }

        void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion desired = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
        }

        void ArriveAtEnd()
        {
            finished = true;

            if (EnergyCoreHealth.Instance != null)
                EnergyCoreHealth.Instance.TakeDamage(coreDamage);

            ReachedEnd?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
