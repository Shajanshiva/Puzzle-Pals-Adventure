using System;
using System.Threading.Tasks;
using UnityEngine;
using PuzzlePals.Backend;
using PuzzlePals.Core;

namespace PuzzlePals.Multiplayer
{
    [Serializable]
    public class MatchmakingTicket
    {
        public string TicketId;
        public string HostUserId;
        public string HostUsername;
        public string RoomCode;
        public string Status; // "waiting", "matched"
        public long Timestamp;
    }

    public class FreeMatchmaker : MonoBehaviour
    {
        public static FreeMatchmaker Instance { get; private set; }

        [Header("Matchmaking State")]
        public bool IsSearching { get; private set; }
        private string activeTicketId;

        public event Action OnSearchingStarted;
        public event Action OnMatchFound;
        public event Action<string> OnMatchmakingFailed;

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

        public async void StartMatchmaking()
        {
            if (IsSearching) return;

            IsSearching = true;
            OnSearchingStarted?.Invoke();
            Debug.Log("[FreeMatchmaker] Starting free matchmaking process...");

            try
            {
                // Step 1: Query Firebase RTDB path '/matchmaking/queue' for existing tickets
                Debug.Log("[FreeMatchmaker] Querying queue for active tickets...");
                await Task.Delay(1000); // Simulate network query

                // Simulate finding an available ticket (50% chance for simulation testing)
                bool foundTicket = UnityEngine.Random.value > 0.5f;

                if (foundTicket)
                {
                    Debug.Log("[FreeMatchmaker] Found open matchmaking ticket. Attempting to claim atomically...");
                    
                    // In production: Use Realtime Database Transaction block on '/matchmaking/queue/{ticketId}'
                    // Transaction ensures only ONE client claims the ticket. If transaction fails, we retry or create ticket.
                    await Task.Delay(800); // Simulate transaction execution

                    bool transactionSuccess = true; // Assume success for mock

                    if (transactionSuccess)
                    {
                        string matchRoomCode = "ROOM" + UnityEngine.Random.Range(10, 99);
                        Debug.Log($"[FreeMatchmaker] Successfully claimed ticket! Joining room code: {matchRoomCode}");
                        
                        // Join Photon Fusion session
                        bool joinSuccess = await NetworkManager.Instance.JoinRoomWithCodeAsync(matchRoomCode);
                        if (joinSuccess)
                        {
                            IsSearching = false;
                            OnMatchFound?.Invoke();
                            GameManager.Instance.ChangeState(Core.GameState.Playing);
                        }
                        else
                        {
                            throw new Exception("Photon connection failed during match joining.");
                        }
                        return;
                    }
                }

                // Step 2: No ticket found, or claiming failed. Create our own ticket.
                Debug.Log("[FreeMatchmaker] Creating new matchmaking ticket as host...");
                activeTicketId = "ticket_" + FirebaseManager.Instance.CurrentUserId;
                
                string newRoomCode = "ROOM" + UnityEngine.Random.Range(10, 99);
                MatchmakingTicket myTicket = new MatchmakingTicket
                {
                    TicketId = activeTicketId,
                    HostUserId = FirebaseManager.Instance.CurrentUserId,
                    HostUsername = DatabaseManager.Instance.CachedProfile?.Username ?? "PuzzlePalHost",
                    RoomCode = newRoomCode,
                    Status = "waiting",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                // Write to RTDB: /matchmaking/queue/{activeTicketId} -> myTicket
                Debug.Log($"[FreeMatchmaker] Ticket created at /matchmaking/queue/{activeTicketId}. Waiting for an opponent to join...");

                // Step 3: Listen for changes on /matchmaking/queue/{activeTicketId}
                // If another player claims it, they will join the Photon session.
                int pollSeconds = 0;
                const int timeoutLimit = 30; // 30 seconds matchmaking timeout
                bool matchJoined = false;

                while (pollSeconds < timeoutLimit && !matchJoined && IsSearching)
                {
                    await Task.Delay(1000);
                    pollSeconds++;

                    // Simulate opponent joining after 5 seconds
                    if (pollSeconds == 5)
                    {
                        matchJoined = true;
                    }
                }

                if (matchJoined && IsSearching)
                {
                    Debug.Log($"[FreeMatchmaker] Opponent connected! Host starting Photon session with room code: {newRoomCode}");
                    
                    // Create private room in Photon
                    bool hostSuccess = await NetworkManager.Instance.JoinRoomWithCodeAsync(newRoomCode); // Host joins or creates session
                    if (hostSuccess)
                    {
                        // Clean up ticket in RTDB
                        Debug.Log($"[FreeMatchmaker] Match established. Deleting ticket: {activeTicketId}");
                        IsSearching = false;
                        OnMatchFound?.Invoke();
                        GameManager.Instance.ChangeState(Core.GameState.Playing);
                    }
                    else
                    {
                        throw new Exception("Photon Host creation failed.");
                    }
                }
                else if (IsSearching)
                {
                    // Matchmaking timed out
                    CancelMatchmaking("Matchmaking timed out. No players found.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FreeMatchmaker] Error during matchmaking: {ex.Message}");
                CancelMatchmaking(ex.Message);
            }
        }

        public void CancelMatchmaking(string reason = "Cancelled by user")
        {
            if (!IsSearching) return;

            IsSearching = false;
            Debug.Log($"[FreeMatchmaker] Matchmaking cancelled. Reason: {reason}");

            // Clean up ticket in RTDB if it was created
            if (!string.IsNullOrEmpty(activeTicketId))
            {
                Debug.Log($"[FreeMatchmaker] Cleaning up ticket: {activeTicketId}");
                activeTicketId = null;
            }

            OnMatchmakingFailed?.Invoke(reason);
        }
    }
}
