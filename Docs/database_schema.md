# Database Schema - Puzzle Pals Adventure

This document outlines the Firestore collection schemes and Realtime Database paths used to manage game states on the free Spark plan.

---

## 1. Cloud Firestore Schema

Firestore handles persistent data like player profiles, friend states, and achievement progression.

### Collection: `users`
Each user has a profile document keyed by their Firebase Auth UID: `/users/{userId}`

```json
{
  "userId": "String",
  "username": "String",
  "avatarId": "String",
  "level": "Integer",
  "experience": "Integer",
  "coins": "Integer",
  "gems": "Integer",
  "createdAt": "Timestamp"
}
```

#### Subcollection: `/users/{userId}/friends`
Tracks the user's friend connections: `/users/{userId}/friends/{friendId}`
```json
{
  "friendId": "String",
  "status": "String (friend / blocked)",
  "establishedAt": "Timestamp"
}
```

#### Subcollection: `/users/{userId}/achievements`
Logs achievements unlocked by the player: `/users/{userId}/achievements/{achievementId}`
```json
{
  "achievementId": "String",
  "unlockedAt": "Timestamp"
}
```

### Collection: `invites`
Manages friend requests and room invite notifications: `/invites/{inviteId}`
```json
{
  "inviteId": "String",
  "senderId": "String",
  "senderUsername": "String",
  "receiverId": "String",
  "roomCode": "String (Optional, for room joins)",
  "status": "String (pending / accepted / declined)",
  "timestamp": "Timestamp"
}
```

---

## 2. Realtime Database Schema

Realtime Database handles low-latency transient data: online presence and matchmaking tickets.

### Path: `/matchmaking_queue`
Active public matchmaking tickets in queue: `/matchmaking_queue/{ticketId}`
```json
{
  "ticketId": "String (equals host userId)",
  "hostUserId": "String",
  "hostUsername": "String",
  "roomCode": "String (Photon room code)",
  "status": "String (waiting / matched)",
  "timestamp": "Integer (Unix Epoch)"
}
```

### Path: `/rooms`
Active game lobbies: `/rooms/{roomCode}`
```json
{
  "sessionName": "String (Room_{roomCode})",
  "hostId": "String"
}
```

### Path: `/status`
Stores real-time online presence for users: `/status/{userId}`
```json
"online" | "offline"
```
*(Updated by Unity client on app state pause/resume/quit)*
