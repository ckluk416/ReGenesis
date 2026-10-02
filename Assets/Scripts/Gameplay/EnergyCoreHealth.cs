using System;
using UnityEngine;

namespace ReGenesis
{
    // The city's Energy Core. The player loses when it reaches zero.
    public class EnergyCoreHealth : MonoBehaviour, IDamageable
    {
        public static EnergyCoreHealth Instance { get; private set; }

        [SerializeField, Min(1f)] private float maxHealth = 100f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<float, float> HealthChanged;
        public event Action Destroyed;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("More than one Energy Core in the scene. Enemies will damage the last one loaded.", this);

            Instance = this;
            CurrentHealth = maxHealth;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
                Destroyed?.Invoke();
        }
    }
}
