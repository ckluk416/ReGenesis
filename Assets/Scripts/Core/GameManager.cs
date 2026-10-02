using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReGenesis
{
    public enum GameState
    {
        Preparing,
        WaveRunning,
        Victory,
        Defeat,
        Paused
    }

    // Owns the game state and decides victory and defeat.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private EnergyCoreHealth energyCore;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private KeyCode pauseKey = KeyCode.P;

        private GameState stateBeforePause;

        public GameState State { get; private set; } = GameState.Preparing;
        public bool IsGameOver => State == GameState.Victory || State == GameState.Defeat;
        public bool CanBuild => State == GameState.Preparing || State == GameState.WaveRunning;
        public bool CanStartWave => State == GameState.Preparing && waveSpawner != null && waveSpawner.HasNextWave;

        public event Action<GameState> StateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than one GameManager in the scene. Removing the extra one.", this);
                Destroy(this);
                return;
            }

            Instance = this;
            Time.timeScale = 1f;

            if (energyCore == null)
                Debug.LogWarning("GameManager has no Energy Core assigned, so defeat cannot trigger.", this);
            if (waveSpawner == null)
                Debug.LogWarning("GameManager has no Wave Spawner assigned, so waves cannot start.", this);
        }

        void OnEnable()
        {
            if (Instance != this)
                return;

            if (energyCore != null)
                energyCore.Destroyed += HandleCoreDestroyed;

            if (waveSpawner != null)
            {
                waveSpawner.WaveStarted += HandleWaveStarted;
                waveSpawner.WaveCompleted += HandleWaveCompleted;
                waveSpawner.AllWavesCompleted += HandleAllWavesCompleted;
            }
        }

        void OnDisable()
        {
            if (energyCore != null)
                energyCore.Destroyed -= HandleCoreDestroyed;

            if (waveSpawner != null)
            {
                waveSpawner.WaveStarted -= HandleWaveStarted;
                waveSpawner.WaveCompleted -= HandleWaveCompleted;
                waveSpawner.AllWavesCompleted -= HandleAllWavesCompleted;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(pauseKey))
                TogglePause();
        }

        // Hook this to the Start Wave button.
        public void StartNextWave()
        {
            if (CanStartWave)
                waveSpawner.StartNextWave();
        }

        public void TogglePause()
        {
            if (IsGameOver)
                return;

            if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(stateBeforePause);
            }
            else
            {
                stateBeforePause = State;
                Time.timeScale = 0f;
                SetState(GameState.Paused);
            }
        }

        // Hook this to the Restart button. The scene must be in the build profile scene list.
        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void HandleWaveStarted(int waveNumber)
        {
            if (!IsGameOver)
                SetState(GameState.WaveRunning);
        }

        void HandleWaveCompleted(int waveNumber)
        {
            if (!IsGameOver && waveSpawner.HasNextWave)
                SetState(GameState.Preparing);
        }

        void HandleAllWavesCompleted()
        {
            if (State != GameState.Defeat)
                SetState(GameState.Victory);
        }

        void HandleCoreDestroyed()
        {
            if (State != GameState.Victory)
            {
                Time.timeScale = 1f;
                SetState(GameState.Defeat);
            }
        }

        void SetState(GameState newState)
        {
            if (State == newState)
                return;

            State = newState;
            StateChanged?.Invoke(newState);
        }
    }
}
