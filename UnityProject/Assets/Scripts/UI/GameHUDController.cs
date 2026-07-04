using System;
using UnityEngine;
using UnityEngine.UIElements;
using PuzzlePals.Core;
using PuzzlePals.Level;
using PuzzlePals.Player;
using PuzzlePals.Multiplayer;

namespace PuzzlePals.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHUDController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;

        // UI References
        private Label lblLevelName;
        private Label lblCoins;
        private Label lblStars;
        private Label lblTimer;
        private Button btnPause;
        
        private VisualElement hintContainer;
        private Label lblHintText;

        // Pause Popup
        private VisualElement pausePopup;
        private Button btnPauseResume;
        private Button btnPauseHint;
        private Button btnPauseQuit;

        // Virtual Controls
        private VisualElement joystickContainer;
        private VisualElement joystickHandle;
        private Button btnJump;
        private Button btnAction;
        private Button btnEmotes;

        // Emotes Selection
        private VisualElement emotesWheel;
        private Button btnEmoteHappy;
        private Button btnEmoteSad;
        private Button btnEmoteSurprised;
        private Button btnEmoteVictory;

        // Local Player Reference
        [Header("Local Player")]
        [SerializeField] private NetworkPlayerController localPlayer;

        // Joystick dragging parameters
        private bool isJoystickDragging = false;
        private Vector2 joystickStartPos;
        private float maxJoystickDistance = 60f; // in pixels

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            QueryElements();
            BindEvents();
            ConfigureJoystick();
        }

        public void AssignLocalPlayer(NetworkPlayerController player)
        {
            localPlayer = player;
        }

        private void QueryElements()
        {
            lblLevelName = root.Q<Label>("hud-level-name");
            lblCoins = root.Q<Label>("hud-coins-val");
            lblStars = root.Q<Label>("hud-stars-val");
            lblTimer = root.Q<Label>("hud-timer-val");
            btnPause = root.Q<Button>("btn-hud-pause");

            hintContainer = root.Q<VisualElement>("hud-hint-container");
            lblHintText = root.Q<Label>("hud-hint-text");

            pausePopup = root.Q<VisualElement>("pause-popup");
            btnPauseResume = root.Q<Button>("btn-pause-resume");
            btnPauseHint = root.Q<Button>("btn-pause-hint");
            btnPauseQuit = root.Q<Button>("btn-pause-quit");

            joystickContainer = root.Q<VisualElement>("joystick-container");
            joystickHandle = root.Q<VisualElement>("joystick-handle");
            btnJump = root.Q<Button>("btn-hud-jump");
            btnAction = root.Q<Button>("btn-hud-action");
            btnEmotes = root.Q<Button>("btn-hud-emotes");

            emotesWheel = root.Q<VisualElement>("emotes-wheel");
            btnEmoteHappy = root.Q<Button>("btn-emote-happy");
            btnEmoteSad = root.Q<Button>("btn-emote-sad");
            btnEmoteSurprised = root.Q<Button>("btn-emote-surprised");
            btnEmoteVictory = root.Q<Button>("btn-emote-victory");
        }

        private void BindEvents()
        {
            btnPause.clicked += TogglePauseMenu;
            btnPauseResume.clicked += TogglePauseMenu;
            btnPauseHint.clicked += RequestLevelHint;
            btnPauseQuit.clicked += QuitLevel;

            btnJump.clicked += TriggerPlayerJump;
            btnAction.clicked += TriggerPlayerAction;
            btnEmotes.clicked += ToggleEmotesWheel;

            btnEmoteHappy.clicked += () => SendEmote("happy");
            btnEmoteSad.clicked += () => SendEmote("sad");
            btnEmoteSurprised.clicked += () => SendEmote("surprised");
            btnEmoteVictory.clicked += () => SendEmote("victory");
        }

        private void ConfigureJoystick()
        {
            // Register pointer events for joystick dragging
            joystickContainer.RegisterCallback<PointerDownEvent>(OnJoystickPointerDown);
            joystickContainer.RegisterCallback<PointerMoveEvent>(OnJoystickPointerMove);
            joystickContainer.RegisterCallback<PointerUpEvent>(OnJoystickPointerUp);
            joystickContainer.RegisterCallback<PointerLeaveEvent>(OnJoystickPointerUp);
        }

        private void Update()
        {
            // Continuously update gameplay statistics
            if (GameManager.Instance != null)
            {
                lblTimer.text = GameManager.Instance.LevelTimer.ToString("F1");
                lblCoins.text = GameManager.Instance.LevelCoinsCollected.ToString("D2");
            }
        }

        // --- JOYSTICK LOGIC ---
        private void OnJoystickPointerDown(PointerDownEvent evt)
        {
            isJoystickDragging = true;
            joystickStartPos = evt.position;
            joystickContainer.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnJoystickPointerMove(PointerMoveEvent evt)
        {
            if (!isJoystickDragging) return;

            Vector2 currentPos = evt.position;
            Vector2 delta = currentPos - joystickStartPos;

            // Clamp joystick movement to boundaries
            float distance = delta.magnitude;
            if (distance > maxJoystickDistance)
            {
                delta = delta.normalized * maxJoystickDistance;
            }

            // Offset the visual handle element
            joystickHandle.style.left = delta.x;
            joystickHandle.style.top = delta.y;

            // Set input vectors on local player
            if (localPlayer != null)
            {
                Vector2 inputVec = new Vector2(delta.x / maxJoystickDistance, -delta.y / maxJoystickDistance);
                localPlayer.SetInputVector(inputVec);
            }

            evt.StopPropagation();
        }

        private void OnJoystickPointerUp(IPointerEvent evt)
        {
            if (!isJoystickDragging) return;

            isJoystickDragging = false;
            joystickContainer.ReleasePointer(evt.pointerId);

            // Reset joystick visual position
            joystickHandle.style.left = 0;
            joystickHandle.style.top = 0;

            // Reset input vectors on local player
            if (localPlayer != null)
            {
                localPlayer.SetInputVector(Vector2.zero);
            }
        }

        // --- BUTTON ACTIONS ---
        private void TriggerPlayerJump()
        {
            if (localPlayer != null)
            {
                localPlayer.TriggerJump();
            }
        }

        private void TriggerPlayerAction()
        {
            if (localPlayer != null)
            {
                localPlayer.InteractOrThrow();
            }
        }

        private void ToggleEmotesWheel()
        {
            emotesWheel.style.display = (emotesWheel.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void SendEmote(string emoteId)
        {
            if (localPlayer != null)
            {
                localPlayer.Rpc_TriggerEmote(emoteId);
            }
            emotesWheel.style.display = DisplayStyle.None;
        }

        private void TogglePauseMenu()
        {
            bool isPaused = pausePopup.style.display == DisplayStyle.Flex;
            pausePopup.style.display = isPaused ? DisplayStyle.None : DisplayStyle.Flex;
            
            // Toggle game manager state
            if (GameManager.Instance != null)
            {
                if (isPaused)
                    GameManager.Instance.ChangeState(GameState.Playing);
                else
                    GameManager.Instance.ChangeState(GameState.MainMenu); // Simulating paused state as menu state
            }
        }

        private void RequestLevelHint()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnHintRequested += DisplayHintBanner;
                LevelManager.Instance.RequestHint();
            }
        }

        private void DisplayHintBanner(string hint)
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnHintRequested -= DisplayHintBanner;

            lblHintText.text = hint;
            hintContainer.style.display = DisplayStyle.Flex;

            // Hide banner after 5 seconds
            CancelInvoke(nameof(HideHintBanner));
            Invoke(nameof(HideHintBanner), 5f);
        }

        private void HideHintBanner()
        {
            hintContainer.style.display = DisplayStyle.None;
        }

        private void QuitLevel()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.DisconnectAsync();
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.MainMenu);
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }
    }
}
