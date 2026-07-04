using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzlePals.Core
{
    public enum Language
    {
        English,
        Tamil
    }

    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [SerializeField] private Language currentLanguage = Language.English;
        public Language CurrentLanguage => currentLanguage;

        public event Action OnLanguageChanged;

        private Dictionary<string, string> englishDictionary = new Dictionary<string, string>
        {
            { "menu_play", "PLAY" },
            { "menu_matchmaking", "ONLINE PLAY" },
            { "menu_friends", "FRIENDS" },
            { "menu_settings", "SETTINGS" },
            { "menu_guest", "GUEST PLAY" },
            { "menu_logout", "LOGOUT" },
            { "hud_coins", "Coins" },
            { "hud_time", "Time" },
            { "hud_stars", "Stars" },
            { "hud_victory", "VICTORY!" },
            { "hud_defeat", "DEFEAT" },
            { "settings_music", "Music Volume" },
            { "settings_sfx", "SFX Volume" },
            { "settings_graphics", "Graphics Level" },
            { "settings_language", "Language / மொழி" },
            { "settings_back", "Back" },
            { "friends_title", "My Friends" },
            { "friends_online", "Online" },
            { "friends_offline", "Offline" },
            { "friends_invite", "Invite" },
            { "lobby_create", "Create Room" },
            { "lobby_join", "Join Room" },
            { "lobby_code", "Enter Room Code" },
            { "lobby_matching", "Finding a Match..." }
        };

        private Dictionary<string, string> tamilDictionary = new Dictionary<string, string>
        {
            { "menu_play", "விளையாடு" },
            { "menu_matchmaking", "ஆன்லைன் விளையாட்டு" },
            { "menu_friends", "நண்பர்கள்" },
            { "menu_settings", "அமைப்புகள்" },
            { "menu_guest", "விருந்தினர் விளையாட்டு" },
            { "menu_logout", "வெளியேறு" },
            { "hud_coins", "நாணயங்கள்" },
            { "hud_time", "நேரம்" },
            { "hud_stars", "நட்சத்திரங்கள்" },
            { "hud_victory", "வெற்றி!" },
            { "hud_defeat", "தோல்வி" },
            { "settings_music", "இசை ஒலி அளவு" },
            { "settings_sfx", "ஒலி விளைவுகள் அளவு" },
            { "settings_graphics", "வரைபட தரம்" },
            { "settings_language", "மொழி / Language" },
            { "settings_back", "பின்னால்" },
            { "friends_title", "என் நண்பர்கள்" },
            { "friends_online", "ஆன்லைனில்" },
            { "friends_offline", "ஆஃப்லைனில்" },
            { "friends_invite", "அழைப்பு" },
            { "lobby_create", "அறையை உருவாக்கு" },
            { "lobby_join", "அறையில் இணை" },
            { "lobby_code", "அறை குறியீடு" },
            { "lobby_matching", "போட்டியைத் தேடுகிறது..." }
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved language preference
            string savedLang = PlayerPrefs.GetString("LanguagePreference", "English");
            if (Enum.TryParse(savedLang, out Language loadedLang))
            {
                currentLanguage = loadedLang;
            }
        }

        public void SetLanguage(Language newLanguage)
        {
            if (currentLanguage == newLanguage) return;

            currentLanguage = newLanguage;
            PlayerPrefs.SetString("LanguagePreference", currentLanguage.ToString());
            PlayerPrefs.Save();

            Debug.Log($"[LocalizationManager] Language changed to: {currentLanguage}");
            OnLanguageChanged?.Invoke();
        }

        public string GetTranslation(string key)
        {
            var dictionary = (currentLanguage == Language.Tamil) ? tamilDictionary : englishDictionary;

            if (dictionary.TryGetValue(key, out string translation))
            {
                return translation;
            }

            Debug.LogWarning($"[LocalizationManager] Translation key not found: {key}");
            return key;
        }
    }
}
