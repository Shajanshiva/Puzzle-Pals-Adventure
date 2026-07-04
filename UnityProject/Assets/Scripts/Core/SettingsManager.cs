using UnityEngine;

namespace PuzzlePals.Core
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("Graphics Presets")]
        private int graphicsQualityIndex = 1; // 0: Low, 1: Medium, 2: High
        public int GraphicsQualityIndex => graphicsQualityIndex;

        [Header("Notifications")]
        private bool notificationsEnabled = true;
        public bool NotificationsEnabled => notificationsEnabled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        private void Start()
        {
            ApplyFPSCap();
        }

        public void ApplyFPSCap()
        {
            // Optimize for 60 FPS gameplay as requested
            Application.targetFrameRate = 60;
            
            // On mobile, also adjust vSync count to prevent frame drops
            QualitySettings.vSyncCount = 0; 
            
            Debug.Log($"[SettingsManager] Target Framerate set to: {Application.targetFrameRate} FPS");
        }

        public void SetGraphicsQuality(int qualityIndex)
        {
            graphicsQualityIndex = Mathf.Clamp(qualityIndex, 0, 2);
            QualitySettings.SetQualityLevel(graphicsQualityIndex, true);
            
            // Adjust specific parameters for low-end devices
            if (graphicsQualityIndex == 0) // Low Quality
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
            }
            else if (graphicsQualityIndex == 1) // Medium Quality
            {
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.antiAliasing = 2;
            }
            else // High Quality
            {
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 4;
            }

            PlayerPrefs.SetInt("GraphicsQuality", graphicsQualityIndex);
            PlayerPrefs.Save();
            Debug.Log($"[SettingsManager] Graphics quality index changed to: {graphicsQualityIndex}");
        }

        public void SetNotificationsEnabled(bool enabled)
        {
            notificationsEnabled = enabled;
            PlayerPrefs.SetInt("NotificationsEnabled", notificationsEnabled ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[SettingsManager] Notifications enabled: {notificationsEnabled}");
        }

        private void LoadSettings()
        {
            graphicsQualityIndex = PlayerPrefs.GetInt("GraphicsQuality", 1);
            SetGraphicsQuality(graphicsQualityIndex);

            notificationsEnabled = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
        }
    }
}
