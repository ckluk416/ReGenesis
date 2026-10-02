using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReGenesis
{
    // Spawns waves one at a time and reports when each wave is cleared.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private EnemyPath path;
        [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();

        private readonly HashSet<GameObject> aliveEnemies = new HashSet<GameObject>();
        private int nextWaveIndex;
        private bool spawning;

        public int CurrentWaveNumber => nextWaveIndex;
        public int TotalWaves => waves.Count;
        public bool HasNextWave => nextWaveIndex < waves.Count;
        public bool IsWaveActive { get; private set; }
        public int AliveEnemies => aliveEnemies.Count;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action AllWavesCompleted;

        bool IsGameOver => GameManager.Instance != null && GameManager.Instance.IsGameOver;

        public void StartNextWave()
        {
            if (IsWaveActive || !HasNextWave || IsGameOver)
                return;

            if (spawnPoint == null || path == null)
            {
                Debug.LogError("WaveSpawner needs both a Spawn Point and an Enemy Path.", this);
                return;
            }

            WaveDefinition wave = waves[nextWaveIndex];
            if (wave == null)
            {
                Debug.LogError($"WaveSpawner wave slot {nextWaveIndex} is empty.", this);
                return;
            }

            nextWaveIndex++;
            StartCoroutine(RunWave(wave));
        }

        IEnumerator RunWave(WaveDefinition wave)
        {
            IsWaveActive = true;
            spawning = true;
            WaveStarted?.Invoke(CurrentWaveNumber);

            if (wave.StartDelay > 0f)
                yield return new WaitForSeconds(wave.StartDelay);

            foreach (WaveDefinition.SpawnGroup group in wave.Groups)
            {
                if (group.enemyPrefab == null)
                {
                    Debug.LogWarning($"{wave.name} has a spawn group with no enemy prefab. Skipping it.", wave);
                    continue;
                }

                for (int i = 0; i < group.count; i++)
                {
                    if (IsGameOver)
                        yield break;

                    Spawn(group.enemyPrefab);

                    if (i < group.count - 1 && group.spawnInterval > 0f)
                        yield return new WaitForSeconds(group.spawnInterval);
                }

                if (group.delayAfterGroup > 0f)
                    yield return new WaitForSeconds(group.delayAfterGroup);
            }

            spawning = false;
            CheckWaveComplete();
        }

        void Spawn(GameObject prefab)
        {
            GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();

            if (movement == null || health == null)
            {
                Debug.LogError($"{prefab.name} needs both EnemyMovement and EnemyHealth on its root.", prefab);
                Destroy(enemy);
                return;
            }

            movement.SetPath(path);
            movement.ReachedEnd += _ => RemoveEnemy(enemy);
            health.Died += _ => RemoveEnemy(enemy);
            aliveEnemies.Add(enemy);
        }

        void Update()
        {
            // Catch enemies removed some other way, such as being deleted in the editor during Play Mode.
            if (IsWaveActive && !spawning && aliveEnemies.RemoveWhere(enemy => enemy == null) > 0)
                CheckWaveComplete();
        }

        void RemoveEnemy(GameObject enemy)
        {
            if (aliveEnemies.Remove(enemy))
                CheckWaveComplete();
        }

        void CheckWaveComplete()
        {
            if (!IsWaveActive || spawning || aliveEnemies.Count > 0)
                return;

            IsWaveActive = false;
            WaveCompleted?.Invoke(CurrentWaveNumber);

            if (!HasNextWave)
                AllWavesCompleted?.Invoke();
        }
    }
}
