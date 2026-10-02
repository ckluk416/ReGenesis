using UnityEngine;

namespace ReGenesis
{
    [RequireComponent(typeof(TowerTargeting))]
    public class SolarTurret : MonoBehaviour
    {
        [SerializeField] private Transform rotatingHead;
        [SerializeField] private Transform muzzlePoint;
        [Tooltip("Leave empty to fire a small placeholder sphere.")]
        [SerializeField] private Projectile projectilePrefab;

        [Header("Stats")]
        [SerializeField, Min(0.01f)] private float fireRate = 1f;
        [SerializeField, Min(0f)] private float damage = 10f;

        [Header("Aiming")]
        [SerializeField, Min(0f)] private float turnSpeed = 360f;
        [Tooltip("Fire only when the head points within this many degrees of the target.")]
        [SerializeField, Range(0f, 45f)] private float aimTolerance = 10f;
        [Tooltip("Corrects models whose barrel does not point along the head's local +Z axis.")]
        [SerializeField] private float headYawOffset;

        private TowerTargeting targeting;
        private float cooldown;

        void Awake()
        {
            targeting = GetComponent<TowerTargeting>();
        }

        void Update()
        {
            cooldown -= Time.deltaTime;

            EnemyHealth target = targeting.CurrentTarget;
            if (target == null)
                return;

            bool aimed = AimAt(target.AimPoint.position);
            if (aimed && cooldown <= 0f)
            {
                Fire(target);
                cooldown = 1f / fireRate;
            }
        }

        // Turns the head around the vertical axis only. Returns true once it is close enough to fire.
        bool AimAt(Vector3 targetPosition)
        {
            if (rotatingHead == null)
                return true;

            Vector3 direction = targetPosition - rotatingHead.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return true;

            Quaternion desired = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, headYawOffset, 0f);
            rotatingHead.rotation = Quaternion.RotateTowards(rotatingHead.rotation, desired, turnSpeed * Time.deltaTime);
            return Quaternion.Angle(rotatingHead.rotation, desired) <= aimTolerance;
        }

        void Fire(EnemyHealth target)
        {
            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up;
            Quaternion rotation = Quaternion.LookRotation(target.AimPoint.position - origin);

            Projectile projectile = projectilePrefab != null
                ? Instantiate(projectilePrefab, origin, rotation)
                : CreatePlaceholderProjectile(origin, rotation);

            projectile.Launch(target, damage);
        }

        static Projectile CreatePlaceholderProjectile(Vector3 position, Quaternion rotation)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Projectile (Placeholder)";
            sphere.transform.SetPositionAndRotation(position, rotation);
            sphere.transform.localScale = Vector3.one * 0.2f;
            Destroy(sphere.GetComponent<Collider>());
            return sphere.AddComponent<Projectile>();
        }
    }
}
