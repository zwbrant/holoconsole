using HoloToolkit.Unity.InputModule;
using UnityEditor;
using UnityEngine;
using Valorem.HoloConsole.CustomHoloHands;
using Valorem.HoloHands;

namespace Valorem.HoloConsole.Editor
{
    public class HoloConsoleEditorFunctions
    {
        private const string HoloConsolePrefabPath = "Assets/_Valorem/_HoloConsole/Prefabs/[HoloConsole].prefab";
        private const string KeywordHandlerPrefabPath = "Assets/_Valorem/_HoloConsole/Prefabs/ConsoleSpeechHandler.prefab";
        private const string InputManagerPrefabPath = "Assets/HoloToolkit/Input/Prefabs/InputManager.prefab";

        private static string SuccessDialogMessage = System.String.Format("HoloConsole has been added to scene and is ready for use. These voice commands have been setup by default:" +
            "{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}",
            System.Environment.NewLine, 
            HoloConsoleKeywordHandler.AttachConsoleKeyword,
            HoloConsoleKeywordHandler.DetachConsoleKeyword, 
            HoloConsoleKeywordHandler.EnableConsoleKeyword, 
            HoloConsoleKeywordHandler.DisableConsoleKeyword, 
            HoloConsoleKeywordHandler.OpenPerformanceViewKeyword, 
            HoloConsoleKeywordHandler.OpenConsoleViewKeyword, 
            HoloConsoleKeywordHandler.ClearConsoleLogKeyword);


        [MenuItem("Valorem/Add HoloConsole to Scene")]
        public static void SpawnAndSetupHoloConsole(MenuCommand menuCommand)
        {
            SpawnHoloToolkitPrefabs(menuCommand);
            SetupHoloHands(menuCommand);
            SpawnKeywordHandlerPrefab(menuCommand);
            SpawnConsolePrefab(menuCommand);
            EditorUtility.DisplayDialog("HoloConsole", SuccessDialogMessage, "OK");
        }

        private static void SpawnConsolePrefab(MenuCommand menuCommand)
        {
            Object consoleObj = AssetDatabase.LoadAssetAtPath<GameObject>(HoloConsolePrefabPath);
            if (consoleObj == null)
            {
                throw new System.Exception("Unable to find HoloConsole prefab at " + HoloConsolePrefabPath);
            }

            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(consoleObj);
            go.name = "[HoloConsole]";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeObject = go;
        }

        private static void SetupHoloHands(MenuCommand menuCommand)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new System.Exception("Couldn't find main camera in scene. Please add or specify one to use HoloConsole.");
            }

            HoloHoldController holoHoldController;
            holoHoldController = camera.gameObject.GetComponent<HoloHoldController>();
            if (holoHoldController == null)
            {
                holoHoldController = camera.gameObject.AddComponent<HoloHoldController>();
            }

            holoHoldController.CursorEnabled = true;
            holoHoldController.GizmosEnabled = false;
            holoHoldController.GlobalOverrideControl = false;
        }

        private static void SpawnHoloToolkitPrefabs(MenuCommand menuCommand)
        {
            // check if required components are present
            InputManager inputManager = Object.FindObjectOfType<InputManager>();
            GazeManager gazeManager = Object.FindObjectOfType<GazeManager>();
            if (inputManager != null && gazeManager != null)
            {
                return;
            }

            // if not, spawn InputManager prefab
            Object inputManagerObj = AssetDatabase.LoadAssetAtPath<GameObject>(InputManagerPrefabPath);
            if (inputManagerObj == null)
            {
                throw new System.Exception(string.Format("Unable to find InputManager prefab at {0}. Do you have the HoloToolkit imported?", InputManagerPrefabPath));
            }

            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(inputManagerObj);
            go.name = "InputManager";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        }

        private static void SpawnKeywordHandlerPrefab(MenuCommand menuCommand)
        {
            HoloConsoleKeywordHandler keywordHandler = Object.FindObjectOfType<HoloConsoleKeywordHandler>();
            if (keywordHandler != null)
            {
                return;
            }

            Object keywordHandlerObj = AssetDatabase.LoadAssetAtPath<GameObject>(KeywordHandlerPrefabPath);
            if (keywordHandlerObj == null)
            {
                throw new System.Exception(string.Format("Unable to find HoloConsoleKeywordHandler prefab at {0}. Do you have the HoloToolkit imported?", KeywordHandlerPrefabPath));
            }

            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(keywordHandlerObj);
            go.name = "HoloConsoleKeywordHandler";
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

        }
    }
}
