using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReGenesis
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        private static readonly List<EnemyHealth> active = new List<EnemyHealth>();

        // Every living enemy in the scene. Towers search this instead of using physics queries.
        public static IReadOnlyList<EnemyHealth> Active => active;

        [SerializeField, Min(1f)] private float maxHealth = 30f;
        [SerializeField, Min(0)] private int energyReward = 5;
        [Tooltip("Point towers aim at. Defaults to this object's position.")]
        [SerializeField] private Transform aimPoint;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        public Transform AimPoint => aimPoint != null ? aimPoint : transform;
        public EnemyMovement Movement { get; private set; }

        public event Action<float, float> HealthChanged;
        public event Action<EnemyHealth> Died;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            active.Clear();
        }

        void Awake()
        {
            CurrentHealth = maxHealth;
            Movement = GetComponent<EnemyMovement>();
        }

        void OnEnable()
        {
            active.Add(this);
        }

        void OnDisable()
        {
            active.Remove(this);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
                Die();
        }

        void Die()
        {
            active.Remove(this);

            if (EnergyBank.Instance != null)
                EnergyBank.Instance.Add(energyReward);

            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
