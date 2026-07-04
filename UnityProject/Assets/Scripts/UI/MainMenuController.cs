using System;
using UnityEngine;
using UnityEngine.UIElements;
using PuzzlePals.Core;
using PuzzlePals.Backend;
using PuzzlePals.Multiplayer;

namespace PuzzlePals.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;

        // UI Elements
        private VisualElement titleScreen;
        private VisualElement loginPanel;
        private VisualElement lobbyPanel;
        private VisualElement friendsPanel;
        private VisualElement settingsPopup;

        private TextField inputEmail;
        private TextField inputPassword;
        private TextField inputRoomCode;
        private TextField inputAddFriendId;

        private Button btnEmailLogin;
        private Button btnEmailRegister;
        private Button btnGoogleLogin;
        private Button btnGuestLogin;
        private Button btnMatchmaking;
        private Button btnCreateRoom;
        private Button btnJoinRoom;
        private Button btnAddFriend;
        private Button btnToggleFriends;
        private Button btnToggleSettings;

        private Button btnLangEn;
        private Button btnLangTa;
        private Button btnGfxLow;
        private Button btnGfxMed;
        private Button btnGfxHigh;
        private Button btnSettingsClose;

        private Slider sliderMusic;
        private Slider sliderSfx;
        private Label welcomeLabel;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            QueryElements();
            BindEvents();
            
            // Subscribe to Localization updates
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged += TranslateUI;

            TranslateUI();
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged -= TranslateUI;
        }

        private void QueryElements()
        {
            titleScreen = root.Q<VisualElement>("title-screen");
            loginPanel = root.Q<VisualElement>("login-panel");
            lobbyPanel = root.Q<VisualElement>("lobby-panel");
            friendsPanel = root.Q<VisualElement>("friends-panel");
            settingsPopup = root.Q<VisualElement>("settings-popup");

            inputEmail = root.Q<TextField>("input-email");
            inputPassword = root.Q<TextField>("input-password");
            inputRoomCode = root.Q<TextField>("input-room-code");
            inputAddFriendId = root.Q<TextField>("input-add-friend-id");

            btnEmailLogin = root.Q<Button>("btn-email-login");
            btnEmailRegister = root.Q<Button>("btn-email-register");
            btnGoogleLogin = root.Q<Button>("btn-google-login");
            btnGuestLogin = root.Q<Button>("btn-guest-login");
            btnMatchmaking = root.Q<Button>("btn-matchmaking");
            btnCreateRoom = root.Q<Button>("btn-create-room");
            btnJoinRoom = root.Q<Button>("btn-join-room");
            btnAddFriend = root.Q<Button>("btn-add-friend");
            btnToggleFriends = root.Q<Button>("btn-toggle-friends");
            btnToggleSettings = root.Q<Button>("btn-toggle-settings");

            btnLangEn = root.Q<Button>("btn-lang-en");
            btnLangTa = root.Q<Button>("btn-lang-ta");
            btnGfxLow = root.Q<Button>("btn-gfx-low");
            btnGfxMed = root.Q<Button>("btn-gfx-med");
            btnGfxHigh = root.Q<Button>("btn-gfx-high");
            btnSettingsClose = root.Q<Button>("btn-settings-close");

            sliderMusic = root.Q<Slider>("slider-music");
            sliderSfx = root.Q<Slider>("slider-sfx");
            welcomeLabel = root.Q<Label>("welcome-label");
        }

        private void BindEvents()
        {
            btnEmailLogin.clicked += OnEmailLoginClicked;
            btnEmailRegister.clicked += OnEmailRegisterClicked;
            btnGoogleLogin.clicked += OnGoogleLoginClicked;
            btnGuestLogin.clicked += OnGuestLoginClicked;

            btnMatchmaking.clicked += OnMatchmakingClicked;
            btnCreateRoom.clicked += OnCreateRoomClicked;
            btnJoinRoom.clicked += OnJoinRoomClicked;
            btnAddFriend.clicked += OnAddFriendClicked;

            btnToggleFriends.clicked += ToggleFriendsPanel;
            btnToggleSettings.clicked += ToggleSettingsPopup;

            // Settings binds
            btnLangEn.clicked += () => LocalizationManager.Instance?.SetLanguage(Language.English);
            btnLangTa.clicked += () => LocalizationManager.Instance?.SetLanguage(Language.Tamil);
            btnGfxLow.clicked += () => SettingsManager.Instance?.SetGraphicsQuality(0);
            btnGfxMed.clicked += () => SettingsManager.Instance?.SetGraphicsQuality(1);
            btnGfxHigh.clicked += () => SettingsManager.Instance?.SetGraphicsQuality(2);
            btnSettingsClose.clicked += SaveAndCloseSettings;

            sliderMusic.RegisterValueChangedCallback(evt => AudioManager.Instance?.SetMusicVolume(evt.newValue));
            sliderSfx.RegisterValueChangedCallback(evt => AudioManager.Instance?.SetSFXVolume(evt.newValue));
        }

        private async void OnEmailLoginClicked()
        {
            PlayClickSound();
            string email = inputEmail.value;
            string pw = inputPassword.value;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw)) return;

            bool ok = await FirebaseManager.Instance.SignInWithEmailAsync(email, pw);
            if (ok) ShowLobbyView();
        }

        private async void OnEmailRegisterClicked()
        {
            PlayClickSound();
            string email = inputEmail.value;
            string pw = inputPassword.value;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw)) return;

            bool ok = await FirebaseManager.Instance.RegisterWithEmailAsync(email, pw);
            if (ok) ShowLobbyView();
        }

        private async void OnGoogleLoginClicked()
        {
            PlayClickSound();
            bool ok = await FirebaseManager.Instance.SignInWithGoogleAsync("google_token_sample");
            if (ok) ShowLobbyView();
        }

        private async void OnGuestLoginClicked()
        {
            PlayClickSound();
            bool ok = await FirebaseManager.Instance.SignInGuestAsync();
            if (ok) ShowLobbyView();
        }

        private void OnMatchmakingClicked()
        {
            PlayClickSound();
            if (FreeMatchmaker.Instance != null)
            {
                FreeMatchmaker.Instance.StartMatchmaking();
            }
        }

        private async void OnCreateRoomClicked()
        {
            PlayClickSound();
            bool ok = await NetworkManager.Instance.CreatePrivateRoomAsync();
            if (ok)
            {
                // Go to level 1 for sample game play
                GameManager.Instance.LoadLevel(1);
            }
        }

        private async void OnJoinRoomClicked()
        {
            PlayClickSound();
            string roomCode = inputRoomCode.value;
            if (string.IsNullOrEmpty(roomCode)) return;

            bool ok = await NetworkManager.Instance.JoinRoomWithCodeAsync(roomCode);
            if (ok)
            {
                GameManager.Instance.LoadLevel(1);
            }
        }

        private async void OnAddFriendClicked()
        {
            PlayClickSound();
            string friendId = inputAddFriendId.value;
            if (string.IsNullOrEmpty(friendId)) return;

            bool sent = await DatabaseManager.Instance.SendFriendInviteAsync(friendId);
            if (sent) inputAddFriendId.value = "";
        }

        private void ShowLobbyView()
        {
            loginPanel.style.display = DisplayStyle.None;
            lobbyPanel.style.display = DisplayStyle.Flex;
            
            if (welcomeLabel != null && DatabaseManager.Instance.CachedProfile != null)
            {
                welcomeLabel.text = $"Welcome, {DatabaseManager.Instance.CachedProfile.Username}!";
            }
        }

        private void ToggleFriendsPanel()
        {
            PlayClickSound();
            friendsPanel.style.display = (friendsPanel.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void ToggleSettingsPopup()
        {
            PlayClickSound();
            settingsPopup.style.display = (settingsPopup.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void SaveAndCloseSettings()
        {
            PlayClickSound();
            settingsPopup.style.display = DisplayStyle.None;
        }

        private void TranslateUI()
        {
            if (LocalizationManager.Instance == null) return;

            btnEmailLogin.text = LocalizationManager.Instance.GetTranslation("menu_guest");
            btnMatchmaking.text = LocalizationManager.Instance.GetTranslation("menu_matchmaking");
            btnCreateRoom.text = LocalizationManager.Instance.GetTranslation("lobby_create");
            btnJoinRoom.text = LocalizationManager.Instance.GetTranslation("lobby_join");
            btnToggleFriends.text = LocalizationManager.Instance.GetTranslation("menu_friends");
            btnToggleSettings.text = LocalizationManager.Instance.GetTranslation("menu_settings");
        }

        private void PlayClickSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("click");
            }
        }
    }
}
