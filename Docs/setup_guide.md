# Setup and Installation Guide - Puzzle Pals Adventure

Follow these steps to import the generated codebase, configure third-party services, and build the project in Unity 6.

---

## 1. Prerequisites
- **Unity Hub** installed with **Unity 6 (6000.0.X or later)**.
- **Android Build Support** module installed in Unity.
- A **Firebase Account** (free tier).
- A **Photon Engine Account** (free tier).

---

## 2. Setting Up Unity Project
1. Open **Unity Hub** -> Click **Add** -> Select the `UnityProject/` directory generated under `C:/shajan_tech/Game/Puzzle Pals Adventure/UnityProject`.
2. Wait for Unity 6 to resolve the project dependencies listed in `manifest.json`.

---

## 3. Importing SDKs

### A. Firebase Unity SDK
1. Download the **Firebase Unity SDK** zip from [Firebase console setup](https://firebase.google.com/download/unity).
2. Unzip and import the following packages (`.unitypackage`) via **Assets -> Import Package -> Custom Package**:
   - `FirebaseAuth.unitypackage`
   - `FirebaseFirestore.unitypackage`
   - `FirebaseDatabase.unitypackage`
   - `FirebaseStorage.unitypackage`

### B. Photon Fusion 2 SDK
1. In Unity, open the **Asset Store** or import the Fusion 2 package directly from the scoped registry:
   - Navigate to **Window -> Package Manager**.
   - Select **Packages: In Project** dropdown -> select **My Registries** or click **+ -> Add package from git URL...** and enter `https://github.com/photonengine/fusion-unity-sdk.git`.
2. Alternatively, download the package from [Photon Engine Dashboard](https://dashboard.photonengine.com/).

---

## 4. Configuring Firebase Console (Spark Free Plan)
1. Go to [Firebase Console](https://console.firebase.google.com/).
2. Click **Add Project** -> Name it `Puzzle Pals Adventure`. (Keep it on the **Spark** free tier. Do NOT upgrade to Blaze).
3. Add an **Android app** to the project:
   - Package name: `com.shajantech.puzzlepals`
4. Download the `google-services.json` file.
5. Place the `google-services.json` file inside the `Assets/` directory of your Unity project.
6. Enable Authentication methods:
   - Go to **Build -> Authentication -> Sign-in method** -> Enable **Email/Password**, **Google**, and **Anonymous** (Guest) login.
7. Create Firestore Database:
   - Select **Build -> Firestore Database -> Create database**. Choose a location and start in Test Mode.
8. Create Realtime Database:
   - Select **Build -> Realtime Database -> Create database**. Choose a region.

---

## 5. Configuring Photon Cloud
1. Go to the [Photon Engine Dashboard](https://dashboard.photonengine.com/).
2. Click **Create a New App** -> Select **Photon Fusion** as SDK Type. Name it `Puzzle Pals Adventure`.
3. Copy the **App ID**.
4. In Unity, go to **Tools -> Fusion -> Setup Wizard** and paste your App ID.
