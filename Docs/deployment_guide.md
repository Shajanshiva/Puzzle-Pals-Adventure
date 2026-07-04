# Deployment Guide - Puzzle Pals Adventure

This guide explains how to deploy database security rules and build the Android APK for Puzzle Pals Adventure.

---

## 1. Deploying Firebase Rules

To deploy the security rules without using paid services, install the Firebase CLI.

1. **Install Firebase CLI**:
   Ensure Node.js is installed, then run in your terminal:
   ```bash
   npm install -g firebase-tools
   ```
2. **Login to Firebase**:
   ```bash
   firebase login
   ```
3. **Navigate to the Firebase Configuration directory**:
   ```bash
   cd "C:/shajan_tech/Game/Puzzle Pals Adventure/FirebaseConfig"
   ```
4. **Initialize Firebase Project**:
   ```bash
   firebase use --add
   ```
   *Select your created `Puzzle Pals Adventure` project.*
5. **Deploy Rules**:
   Run the following command to deploy all firestore, realtime database, and storage rules simultaneously:
   ```bash
   firebase deploy --only database,firestore,storage
   ```

---

## 2. Building the Android APK

1. In Unity, open the project.
2. Go to **File -> Build Settings**.
3. Select **Android** in the platform list -> Click **Switch Platform**.
4. Click **Player Settings** (bottom left):
   - **Company Name**: `ShajanTech`
   - **Product Name**: `Puzzle Pals Adventure`
   - **Package Name**: `com.shajantech.puzzlepals`
   - **Minimum API Level**: Android 9.0 (API Level 28)
   - **Target API Level**: Android 14.0 (API Level 34) or latest
   - **Graphics APIs**: OpenGLES3 (for low-end mobile optimization)
5. Generate a Release **Keystore** under **Player Settings -> Publishing Settings -> Keystore Manager**.
6. Close Player Settings.
7. Click **Add Open Scenes** to add `MainMenuScene` and all level scenes (`Level_1`, etc.).
8. Click **Build** -> Choose target directory -> Save as `PuzzlePalsAdventure.apk`.
