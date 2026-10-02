using UnityEngine;

namespace ReGenesis
{
    // Homes in on one enemy. If the target dies first, it keeps flying until its lifetime ends.
    public class Projectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 12f;
        [SerializeField, Min(0.1f)] private float lifetime = 3f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.3f;

        private EnemyHealth target;
        private float damage;
        private Vector3 direction;
        private float age;

        public void Launch(EnemyHealth newTarget, float newDamage)
        {
            target = newTarget;
            damage = newDamage;
            age = 0f;
            direction = target != null
                ? (target.AimPoint.position - transform.position).normalized
                : transform.forward;
        }

        void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            float step = speed * Time.deltaTime;

            if (target != null && target.IsAlive)
            {
                Vector3 toTarget = target.AimPoint.position - transform.position;
                if (toTarget.magnitude <= Mathf.Max(hitRadius, step))
                {
                    target.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }

                direction = toTarget.normalized;
            }

            transform.position += direction * step;
            if (direction.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
