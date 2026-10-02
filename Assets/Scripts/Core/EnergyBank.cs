using System;
using UnityEngine;

namespace ReGenesis
{
    // Holds the player's Energy Resource, which pays for towers.
    public class EnergyBank : MonoBehaviour
    {
        public static EnergyBank Instance { get; private set; }

        [SerializeField, Min(0)] private int startingEnergy = 100;

        public int Energy { get; private set; }

        public event Action<int> EnergyChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than one EnergyBank in the scene. Removing the extra one.", this);
                Destroy(this);
                return;
            }

            Instance = this;
            Energy = startingEnergy;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool CanAfford(int amount)
        {
            return amount <= Energy;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > Energy)
                return false;

            Energy -= amount;
            EnergyChanged?.Invoke(Energy);
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            Energy += amount;
            EnergyChanged?.Invoke(Energy);
        }
    }
}
