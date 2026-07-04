using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using PuzzlePals.Core;
using PuzzlePals.Level;
using PuzzlePals.UI;
using PuzzlePals.Multiplayer;
using PuzzlePals.Backend;

namespace PuzzlePals.Editor
{
    public static class SceneInitializer
    {
        [MenuItem("Tools/Initialize All Scenes")]
        public static void InitializeAll()
        {
            // 1. Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            if (!AssetDatabase.IsValidFolder("Assets/UI"))
            {
                AssetDatabase.CreateFolder("Assets", "UI");
            }

            Debug.Log("[SceneInitializer] Starting automatic scene generation...");

            // Create PanelSettings if it doesn't exist
            string settingsPath = "Assets/UI/PanelSettings.asset";
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(settingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;

                AssetDatabase.CreateAsset(panelSettings, settingsPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[SceneInitializer] Created new PanelSettings asset for mobile scaling.");
            }

            // 2. Create MainMenuScene
            CreateMainMenuScene(panelSettings);

            // 3. Create Levels
            CreateLevelScene(1, "First Steps", panelSettings);
            CreateLevelScene(2, "Double Trouble", panelSettings);
            CreateLevelScene(3, "The Great Wall", panelSettings);

            // 4. Update Build Settings
            UpdateBuildSettings();

            Debug.Log("[SceneInitializer] Success! All scenes created and added to Build Settings.");
            EditorUtility.DisplayDialog("Success", "All scenes (MainMenuScene, Level_1, Level_2, Level_3) generated successfully with responsive Panel Settings and registered in Build Settings!", "OK");
        }

        private static void CreateMainMenuScene(PanelSettings panelSettings)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create managers GameObject
            GameObject managersObj = new GameObject("GlobalManagers");
            managersObj.AddComponent<GameManager>();
            managersObj.AddComponent<LocalizationManager>();
            managersObj.AddComponent<AudioManager>();
            managersObj.AddComponent<SettingsManager>();
            managersObj.AddComponent<FirebaseManager>();
            managersObj.AddComponent<DatabaseManager>();
            managersObj.AddComponent<NetworkManager>();
            managersObj.AddComponent<FreeMatchmaker>();

            // Create UI GameObject
            GameObject uiObj = new GameObject("MainMenuUI");
            var uiDoc = uiObj.AddComponent<UIDocument>();
            uiDoc.panelSettings = panelSettings;
            var controller = uiObj.AddComponent<MainMenuController>();

            // Load UXML asset
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/main_menu.uxml");
            if (uxml != null)
            {
                uiDoc.visualTreeAsset = uxml;
                Debug.Log("[SceneInitializer] Assigned main_menu UXML to MainMenuScene.");
            }
            else
            {
                Debug.LogWarning("[SceneInitializer] Could not find Assets/UI/main_menu.uxml");
            }

            // Save the scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenuScene.unity");
        }

        private static void CreateLevelScene(int index, string levelName, PanelSettings panelSettings)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Add LevelManager
            GameObject levelMgrObj = new GameObject("LevelManager");
            var levelMgr = levelMgrObj.AddComponent<LevelManager>();

            // Add UI HUD
            GameObject hudObj = new GameObject("GameHUDUI");
            var uiDoc = hudObj.AddComponent<UIDocument>();
            uiDoc.panelSettings = panelSettings;
            var controller = hudObj.AddComponent<GameHUDController>();

            // Load HUD UXML asset
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/game_hud.uxml");
            if (uxml != null)
            {
                uiDoc.visualTreeAsset = uxml;
                Debug.Log($"[SceneInitializer] Assigned game_hud UXML to Level_{index}.");
            }

            // --- BUILD SIMPLE LEVEL DESIGN ---
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            ground.GetComponent<Renderer>().sharedMaterial.color = Color.gray;

            // Pressure Plate Trigger
            GameObject pressurePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pressurePlate.name = "PressurePlate_Trigger";
            pressurePlate.transform.position = new Vector3(-3f, 0.6f, 0f);
            pressurePlate.transform.localScale = new Vector3(2f, 0.2f, 2f);
            pressurePlate.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            var plateTrigger = pressurePlate.GetComponent<BoxCollider>();
            plateTrigger.isTrigger = true;
            
            var plateElem = pressurePlate.AddComponent<PuzzleElement>();
            SetPrivateField(plateElem, "puzzleId", $"plate_{index}");
            SetPrivateField(plateElem, "elementType", PuzzleElementType.PressurePlate);

            // Sliding Door Obstacle
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "SlidingDoor_Obstacle";
            door.transform.position = new Vector3(3f, 2.5f, 0f);
            door.transform.localScale = new Vector3(1f, 4f, 5f);
            door.GetComponent<Renderer>().sharedMaterial.color = Color.green;

            var doorElem = door.AddComponent<PuzzleElement>();
            SetPrivateField(doorElem, "puzzleId", $"door_{index}");
            SetPrivateField(doorElem, "elementType", PuzzleElementType.SlidingDoor);
            SetPrivateField(doorElem, "activeOffset", new Vector3(0f, 4f, 0f));
            SetPrivateField(doorElem, "moveSpeed", 3f);
            
            // Link pressure plate to door
            var triggerList = new List<PuzzleElement> { plateElem };
            SetPrivateField(doorElem, "triggerSources", triggerList);

            // Goal Zone
            GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "GoalZone";
            goal.transform.position = new Vector3(10f, 1f, 0f);
            goal.transform.localScale = new Vector3(3f, 2f, 3f);
            goal.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
            var goalCol = goal.GetComponent<BoxCollider>();
            goalCol.isTrigger = true;
            goal.AddComponent<GoalTrigger>();

            // Save the scene
            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Level_{index}.unity");
        }

        private static void UpdateBuildSettings()
        {
            var buildScenes = new List<EditorBuildSettingsScene>();
            buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true));
            buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level_1.unity", true));
            buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level_2.unity", true));
            buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Level_3.unity", true));

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }

    public class GoalTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && LevelManager.Instance != null)
            {
                Debug.Log("[GoalTrigger] Player reached the goal! Level Complete!");
                LevelManager.Instance.CompleteLevel();
            }
        }
    }
}
