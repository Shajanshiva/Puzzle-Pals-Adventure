using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzlePals.Core;
using PuzzlePals.Backend;

namespace PuzzlePals.Level
{
    [Serializable]
    public class LevelData
    {
        public int LevelIndex;
        public string Name;
        public float TargetTime3Stars;
        public float TargetTime2Stars;
        public int TotalCollectibles;
        public string HintMessageTamil;
        public string HintMessageEnglish;
    }

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Current Level Configuration")]
        [SerializeField] private LevelData currentLevelData;
        public LevelData CurrentLevelData => currentLevelData;

        [Header("Checkpoint System")]
        [SerializeField] private Vector3 activeCheckpointPosition;
        private bool hasCheckpoint = false;

        [Header("Level Data Repository")]
        private Dictionary<int, LevelData> levelsRepository = new Dictionary<int, LevelData>();

        public event Action<Vector3> OnCheckpointActivated;
        public event Action<string> OnHintRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GenerateLevelsRepository();
        }

        public void InitializeLevel(int levelIndex)
        {
            hasCheckpoint = false;
            
            if (levelsRepository.TryGetValue(levelIndex, out LevelData data))
            {
                currentLevelData = data;
                Debug.Log($"[LevelManager] Initialized level {levelIndex}: {data.Name}");
            }
            else
            {
                // Fallback for dynamically generated layouts
                currentLevelData = new LevelData
                {
                    LevelIndex = levelIndex,
                    Name = $"Puzzle Adventure {levelIndex}",
                    TargetTime3Stars = 60f + levelIndex * 2f,
                    TargetTime2Stars = 120f + levelIndex * 3f,
                    TotalCollectibles = 3,
                    HintMessageEnglish = "Use teamwork to activate both levers simultaneously!",
                    HintMessageTamil = "இரு நெம்புகோல்களையும் ஒரே நேரத்தில் செயல்படுத்த கூட்டு முயற்சியை பயன்படுத்தவும்!"
                };
            }
        }

        public void SetCheckpoint(Vector3 position)
        {
            activeCheckpointPosition = position;
            hasCheckpoint = true;
            Debug.Log($"[LevelManager] Checkpoint registered at: {position}");
            OnCheckpointActivated?.Invoke(position);
        }

        public Vector3 GetSpawnPosition(Vector3 defaultPosition)
        {
            return hasCheckpoint ? activeCheckpointPosition : defaultPosition;
        }

        public void RequestHint()
        {
            if (currentLevelData == null) return;

            string hint = (LocalizationManager.Instance.CurrentLanguage == Language.Tamil) 
                ? currentLevelData.HintMessageTamil 
                : currentLevelData.HintMessageEnglish;

            Debug.Log($"[LevelManager] Hint requested: {hint}");
            OnHintRequested?.Invoke(hint);
        }

        public async void CompleteLevel()
        {
            if (currentLevelData == null) return;

            float timeTaken = GameManager.Instance.LevelTimer;
            int coinsCollected = GameManager.Instance.LevelCoinsCollected;
            int collectiblesFound = GameManager.Instance.LevelCollectiblesFound;

            GameManager.Instance.CompleteLevel(currentLevelData.TargetTime3Stars, currentLevelData.TargetTime2Stars);
            
            int starsEarned = GameManager.Instance.LevelStarsEarned;
            int xpEarned = starsEarned * 100 + coinsCollected * 5 + (collectiblesFound * 50);

            Debug.Log($"[LevelManager] Submitting level completion rewards: Coins: {coinsCollected}, XP: {xpEarned}");

            // Reward player and sync cloud save
            if (DatabaseManager.Instance != null)
            {
                await DatabaseManager.Instance.AddRewardsAsync(coinsCollected, 0, xpEarned);
                
                // Unlock next level
                int nextLevel = currentLevelData.LevelIndex + 1;
                if (nextLevel <= GameManager.MaxLevels)
                {
                    PlayerPrefs.SetInt($"Level_{nextLevel}_Unlocked", 1);
                    PlayerPrefs.Save();
                }
            }
        }

        private void GenerateLevelsRepository()
        {
            // Seed a representative sample of levels (up to 100)
            levelsRepository.Add(1, new LevelData
            {
                LevelIndex = 1,
                Name = "First Steps",
                TargetTime3Stars = 45f,
                TargetTime2Stars = 90f,
                TotalCollectibles = 3,
                HintMessageEnglish = "Step on the red button to open the green gate.",
                HintMessageTamil = "பச்சை நிற வாசலைத் திறக்க சிவப்பு பொத்தானை மிதிக்கவும்."
            });

            levelsRepository.Add(2, new LevelData
            {
                LevelIndex = 2,
                Name = "Double Trouble",
                TargetTime3Stars = 60f,
                TargetTime2Stars = 120f,
                TotalCollectibles = 3,
                HintMessageEnglish = "Both players must stand on their respective pressure plates at the same time.",
                HintMessageTamil = "இரு வீரர்களும் ஒரே நேரத்தில் தங்களுக்குரிய அழுத்த தகடுகளில் நிற்க வேண்டும்."
            });

            levelsRepository.Add(3, new LevelData
            {
                LevelIndex = 3,
                Name = "The Great Wall",
                TargetTime3Stars = 80f,
                TargetTime2Stars = 150f,
                TotalCollectibles = 3,
                HintMessageEnglish = "Carry your partner and throw them onto the ledge to drop the ladder.",
                HintMessageTamil = "உங்கள் துணையைத் தூக்கி உச்சிக்கு எறிந்து ஏணியைக் கீழே இறக்கவும்."
            });
        }
    }
}
