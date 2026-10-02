using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReGenesis
{
    [CreateAssetMenu(fileName = "Wave_", menuName = "ReGenesis/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        [Serializable]
        public class SpawnGroup
        {
            public GameObject enemyPrefab;
            [Min(1)] public int count = 5;
            [Min(0f)] public float spawnInterval = 1.5f;
            [Tooltip("Pause after this group before the next group starts.")]
            [Min(0f)] public float delayAfterGroup;
        }

        [SerializeField, Min(0f)] private float startDelay = 2f;
        [SerializeField] private List<SpawnGroup> groups = new List<SpawnGroup>();

        public float StartDelay => startDelay;
        public IReadOnlyList<SpawnGroup> Groups => groups;

        public int TotalEnemies
        {
            get
            {
                int total = 0;
                foreach (SpawnGroup group in groups)
                {
                    if (group.enemyPrefab != null)
                        total += group.count;
                }
                return total;
            }
        }
    }
}
