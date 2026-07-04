using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzlePals.Core
{
    public enum GameState
    {
        MainMenu,
        Lobby,
        Loading,
        Playing,
        Victory,
        Reconnecting
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        public GameState CurrentState => currentState;

        [Header("Level Progress")]
        public const int MaxLevels = 100;
        private int currentLevelIndex = 1;
        public int CurrentLevelIndex => currentLevelIndex;

        // Current level score tracking
        public float LevelTimer { get; private set; }
        public int LevelCoinsCollected { get; private set; }
        public int LevelStarsEarned { get; private set; }
        public int LevelCollectiblesFound { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnLevelChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ChangeState(GameState.MainMenu);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                LevelTimer += Time.deltaTime;
            }
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            Debug.Log($"[GameManager] Game State changed to: {currentState}");

            switch (currentState)
            {
                case GameState.MainMenu:
                    ResetLevelStats();
                    break;
                case GameState.Playing:
                    ResetLevelStats();
                    break;
            }

            OnStateChanged?.Invoke(currentState);
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 1 || levelIndex > MaxLevels)
            {
                Debug.LogError($"[GameManager] Level index {levelIndex} is invalid.");
                return;
            }

            currentLevelIndex = levelIndex;
            ChangeState(GameState.Loading);
            
            // Map index to worlds (e.g., Levels 1-15 Forest, 16-30 Beach, etc.)
            string worldName = GetWorldName(levelIndex);
            Debug.Log($"[GameManager] Loading Level {levelIndex} in World: {worldName}");

            // Load level scene (e.g., Level_1, Level_2)
            SceneManager.LoadScene($"Level_{levelIndex}");
            
            ChangeState(GameState.Playing);
            OnLevelChanged?.Invoke(currentLevelIndex);
        }

        public void CollectCoin(int value = 1)
        {
            LevelCoinsCollected += value;
        }

        public void FindCollectible()
        {
            LevelCollectiblesFound++;
        }

        public void CompleteLevel(float timeLimit3Stars, float timeLimit2Stars)
        {
            if (currentState != GameState.Playing) return;

            // Calculate star rating
            if (LevelTimer <= timeLimit3Stars)
            {
                LevelStarsEarned = 3;
            }
            else if (LevelTimer <= timeLimit2Stars)
            {
                LevelStarsEarned = 2;
            }
            else
            {
                LevelStarsEarned = 1;
            }

            ChangeState(GameState.Victory);
            Debug.Log($"[GameManager] Level {currentLevelIndex} completed! Stars: {LevelStarsEarned}, Coins: {LevelCoinsCollected}, Time: {LevelTimer:F2}s");
        }

        public string GetWorldName(int levelIndex)
        {
            if (levelIndex <= 15) return "Forest";
            if (levelIndex <= 30) return "Beach";
            if (levelIndex <= 45) return "Desert";
            if (levelIndex <= 60) return "Snow";
            if (levelIndex <= 80) return "Space";
            return "Sky Kingdom";
        }

        private void ResetLevelStats()
        {
            LevelTimer = 0f;
            LevelCoinsCollected = 0;
            LevelStarsEarned = 0;
            LevelCollectiblesFound = 0;
        }
    }
}
