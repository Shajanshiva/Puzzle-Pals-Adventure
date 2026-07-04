using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
// Photon Fusion imports
// using Fusion;
// using Fusion.Sockets;

namespace PuzzlePals.Multiplayer
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Photon Config")]
        [SerializeField] private string gameVersion = "1.0.0";
        private bool isConnecting = false;
        private string currentRoomCode;
        public string CurrentRoomCode => currentRoomCode;

        public event Action OnConnectionStarted;
        public event Action OnConnectionSuccess;
        public event Action<string> OnConnectionFailed;
        public event Action OnPlayerSpawned;
        public event Action OnDisconnected;

        // Mock Photon Fusion references for compiling without SDK pre-installed
        private object mockRunner = null; 

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

        public async Task<bool> CreatePrivateRoomAsync()
        {
            if (isConnecting) return false;
            
            isConnecting = true;
            OnConnectionStarted?.Invoke();
            
            // Generate a random 6-character room code
            currentRoomCode = GenerateRoomCode();
            Debug.Log($"[NetworkManager] Generated Room Code: {currentRoomCode}");
            
            // Simulate connecting to Photon Cloud as host
            await Task.Delay(1500); 

            // Register room code in Firebase Realtime Database
            // RTDB Node: /rooms/{currentRoomCode} -> { "sessionName": "Room_" + currentRoomCode, "hostId": FirebaseManager.Instance.CurrentUserId }
            Debug.Log($"[NetworkManager] Registered room code {currentRoomCode} in Firebase Realtime Database.");

            isConnecting = false;
            OnConnectionSuccess?.Invoke();
            return true;
        }

        public async Task<bool> JoinRoomWithCodeAsync(string roomCode)
        {
            if (isConnecting || string.IsNullOrEmpty(roomCode)) return false;

            isConnecting = true;
            OnConnectionStarted?.Invoke();
            roomCode = roomCode.ToUpper().Trim();
            
            Debug.Log($"[NetworkManager] Querying Firebase Realtime Database for Room Code: {roomCode}");
            await Task.Delay(1000); // Simulate RTDB lookup latency

            // Simulate look up check:
            // Fetch /rooms/{roomCode}
            bool roomExists = roomCode.Length == 6; // Simple validation for mock

            if (!roomExists)
            {
                isConnecting = false;
                OnConnectionFailed?.Invoke("Room code not found or expired.");
                return false;
            }

            currentRoomCode = roomCode;
            Debug.Log($"[NetworkManager] Joining Photon Session: Room_{currentRoomCode}");
            await Task.Delay(1000); // Simulate Photon connection time

            isConnecting = false;
            OnConnectionSuccess?.Invoke();
            return true;
        }

        public async void DisconnectAsync()
        {
            Debug.Log("[NetworkManager] Disconnecting from Photon Fusion Session...");
            await Task.Delay(500);
            currentRoomCode = null;
            OnDisconnected?.Invoke();
        }

        public async void HandleReconnect()
        {
            if (string.IsNullOrEmpty(currentRoomCode)) return;

            Debug.Log("[NetworkManager] Network dropout detected! Initiating reconnection loop...");
            GameManager.Instance.ChangeState(Core.GameState.Reconnecting);

            int retryCount = 0;
            const int maxRetries = 5;
            bool success = false;

            while (retryCount < maxRetries && !success)
            {
                retryCount++;
                Debug.Log($"[NetworkManager] Reconnection attempt {retryCount}/{maxRetries}...");
                await Task.Delay(2000 * retryCount); // Exponential backoff

                // Try to rejoin the session
                success = true; // Simulating successful reconnection
            }

            if (success)
            {
                Debug.Log("[NetworkManager] Successfully reconnected to multiplayer session!");
                GameManager.Instance.ChangeState(Core.GameState.Playing);
            }
            else
            {
                Debug.LogError("[NetworkManager] Reconnection failed. Returning to Main Menu.");
                DisconnectAsync();
                GameManager.Instance.ChangeState(Core.GameState.MainMenu);
            }
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[6];
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return new string(stringChars);
        }

        // Implementation of INetworkRunnerCallbacks
        // During actual compilation, these methods will receive and process Photon network events.
        #region Photon Callbacks
        public void OnPlayerJoined(object runner, object player)
        {
            Debug.Log("[Photon Callback] Player joined session.");
            OnPlayerSpawned?.Invoke();
        }

        public void OnPlayerLeft(object runner, object player)
        {
            Debug.Log("[Photon Callback] Player left session.");
        }

        public void OnShutdown(object runner, int shutdownReason)
        {
            Debug.Log($"[Photon Callback] Connection shutdown. Reason: {shutdownReason}");
            HandleReconnect();
        }
        #endregion
    }
}
