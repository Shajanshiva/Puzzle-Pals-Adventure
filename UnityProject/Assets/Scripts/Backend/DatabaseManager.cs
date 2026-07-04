using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PuzzlePals.Backend
{
    [Serializable]
    public class PlayerProfile
    {
        public string UserId;
        public string Username;
        public string AvatarId;
        public int Level;
        public int Experience;
        public int Coins;
        public int Gems;
        public List<string> UnlockedSkins = new List<string>();
        public List<string> UnlockedWorlds = new List<string>();
        public List<string> Friends = new List<string>();
        public List<string> Achievements = new List<string>();
        public string OnlineStatus;
    }

    [Serializable]
    public class FriendInvite
    {
        public string InviteId;
        public string SenderId;
        public string SenderUsername;
        public string ReceiverId;
        public string RoomCode;
        public string Status; // "pending", "accepted", "declined"
    }

    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        [Header("Cache")]
        [SerializeField] private PlayerProfile cachedProfile;
        public PlayerProfile CachedProfile => cachedProfile;

        public event Action<PlayerProfile> OnProfileLoaded;
        public event Action OnFriendsListUpdated;

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

        private void OnEnable()
        {
            FirebaseManager.Instance.OnUserSignedIn += HandleUserSignedIn;
            FirebaseManager.Instance.OnUserSignedOut += HandleUserSignedOut;
        }

        private void OnDisable()
        {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.OnUserSignedIn -= HandleUserSignedIn;
                FirebaseManager.Instance.OnUserSignedOut -= HandleUserSignedOut;
            }
        }

        private async void HandleUserSignedIn(string userId)
        {
            await LoadPlayerProfileAsync(userId);
            SetOnlineStatus(true);
        }

        private void HandleUserSignedOut()
        {
            SetOnlineStatus(false);
            cachedProfile = null;
        }

        public async Task<PlayerProfile> LoadPlayerProfileAsync(string userId)
        {
            Debug.Log($"[DatabaseManager] Loading profile for: {userId}");
            await Task.Delay(500); // Simulate network query

            // In production, fetch from Firestore `users/{userId}`
            cachedProfile = new PlayerProfile
            {
                UserId = userId,
                Username = PlayerPrefs.GetString("Profile_Username", "PuzzlePal_" + userId.Substring(Mathf.Max(0, userId.Length - 4))),
                AvatarId = PlayerPrefs.GetString("Profile_AvatarId", "avatar_01"),
                Level = PlayerPrefs.GetInt("Profile_Level", 1),
                Experience = PlayerPrefs.GetInt("Profile_Experience", 0),
                Coins = PlayerPrefs.GetInt("Profile_Coins", 100),
                Gems = PlayerPrefs.GetInt("Profile_Gems", 10),
                OnlineStatus = "online"
            };

            cachedProfile.UnlockedSkins.Add("skin_classic");
            cachedProfile.UnlockedWorlds.Add("World_1"); // Forest always unlocked

            OnProfileLoaded?.Invoke(cachedProfile);
            return cachedProfile;
        }

        public async Task SavePlayerProfileAsync()
        {
            if (cachedProfile == null) return;

            Debug.Log($"[DatabaseManager] Saving profile to Firestore for: {cachedProfile.UserId}");
            
            // Local fallback
            PlayerPrefs.SetString("Profile_Username", cachedProfile.Username);
            PlayerPrefs.SetString("Profile_AvatarId", cachedProfile.AvatarId);
            PlayerPrefs.SetInt("Profile_Level", cachedProfile.Level);
            PlayerPrefs.SetInt("Profile_Experience", cachedProfile.Experience);
            PlayerPrefs.SetInt("Profile_Coins", cachedProfile.Coins);
            PlayerPrefs.SetInt("Profile_Gems", cachedProfile.Gems);
            PlayerPrefs.Save();

            await Task.Delay(300); // Simulate Firestore latency
        }

        public async void SetOnlineStatus(bool isOnline)
        {
            if (cachedProfile == null) return;
            
            string status = isOnline ? "online" : "offline";
            cachedProfile.OnlineStatus = status;

            // In production, write to Firebase RTDB path `/status/{userId}`
            Debug.Log($"[DatabaseManager] Set RTDB status to '{status}' for user {cachedProfile.UserId}");
            await Task.Delay(200);
        }

        public async Task<bool> SendFriendInviteAsync(string targetFriendId)
        {
            if (cachedProfile == null) return false;
            
            Debug.Log($"[DatabaseManager] Sending invite from {cachedProfile.UserId} to {targetFriendId}");
            await Task.Delay(600);

            // In production, write new invite document to Firestore `invites/` collection
            return true;
        }

        public async Task<List<FriendProfile>> FetchFriendsListAsync()
        {
            await Task.Delay(500); // Simulate network query
            var list = new List<FriendProfile>();

            if (cachedProfile == null) return list;

            // Mock friend profiles for testing
            list.Add(new FriendProfile { UserId = "pal_001", Username = "BouncyBunny", Status = "online", AvatarId = "avatar_02" });
            list.Add(new FriendProfile { UserId = "pal_002", Username = "MightyBear", Status = "offline", AvatarId = "avatar_03" });

            return list;
        }

        public async Task AddRewardsAsync(int coinsReward, int gemsReward, int xpReward)
        {
            if (cachedProfile == null) return;

            cachedProfile.Coins += coinsReward;
            cachedProfile.Gems += gemsReward;
            cachedProfile.Experience += xpReward;

            // Level up threshold: 1000 XP per level
            while (cachedProfile.Experience >= 1000)
            {
                cachedProfile.Experience -= 1000;
                cachedProfile.Level++;
                Debug.Log($"[DatabaseManager] Level Up! New Level: {cachedProfile.Level}");
            }

            await SavePlayerProfileAsync();
        }
    }

    [Serializable]
    public class FriendProfile
    {
        public string UserId;
        public string Username;
        public string Status;
        public string AvatarId;
    }
}
