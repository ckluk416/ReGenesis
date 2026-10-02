using UnityEngine;

namespace ReGenesis
{
    // Picks the living enemy in range that is furthest along the path, and keeps it until it dies or leaves range.
    public class TowerTargeting : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float range = 5f;
        [SerializeField] private Transform rangeOrigin;
        [SerializeField, Min(0.02f)] private float retargetInterval = 0.1f;

        private float nextSearchTime;

        public float Range => range;
        public EnemyHealth CurrentTarget { get; private set; }

        Vector3 Origin => (rangeOrigin != null ? rangeOrigin : transform).position;

        void Update()
        {
            if (CurrentTarget != null && !IsValidTarget(CurrentTarget))
                CurrentTarget = null;

            if (CurrentTarget == null && Time.time >= nextSearchTime)
            {
                nextSearchTime = Time.time + retargetInterval;
                CurrentTarget = FindTarget();
            }
        }

        EnemyHealth FindTarget()
        {
            EnemyHealth best = null;
            float bestProgress = float.MinValue;

            foreach (EnemyHealth enemy in EnemyHealth.Active)
            {
                if (!IsValidTarget(enemy))
                    continue;

                float progress = enemy.Movement != null ? enemy.Movement.DistanceTravelled : 0f;
                if (progress > bestProgress)
                {
                    bestProgress = progress;
                    best = enemy;
                }
            }

            return best;
        }

        bool IsValidTarget(EnemyHealth enemy)
        {
            return enemy != null && enemy.IsAlive && IsInRange(enemy.transform.position);
        }

        // Range is measured on the ground plane so flying enemies count the same as ground enemies.
        bool IsInRange(Vector3 position)
        {
            Vector3 offset = position - Origin;
            offset.y = 0f;
            return offset.sqrMagnitude <= range * range;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Origin, range);
        }
    }
}
