#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimblendTools
{
    public class MissingScriptsEditor : EditorWindow
    {
        private Vector2 scrollPosWindow = Vector2.zero;
        private Vector2 scrollPosSceneMissingScripts = Vector2.zero;
        private Vector2 scrollPosPrefabMissingScripts = Vector2.zero;
        private List<GameObject> prefabsWithMissingScripts = new List<GameObject>();
        private List<GameObject> objectsWithMissingScripts = new List<GameObject>();

        private bool removedMissingScripts = false;

        [MenuItem("Simblend/Simblend Tools/Scripting/Missing Scripts")]
        public static void ShowWindow()
        {
            GetWindow<MissingScriptsEditor>("Remove Missing Scripts");
        }

        private void OnGUI()
        {

            GUILayout.Label("Remove missing scripts from prefabs and scenes");
            GUILayout.Space(30);

            scrollPosWindow = EditorGUILayout.BeginScrollView(scrollPosWindow);

            if (GUILayout.Button("Find prefabs with Missing Scripts"))
            {
                FindObjectsWithMissingScriptsInPrefabs();
            }

            EditorGUILayout.Space();

            if (prefabsWithMissingScripts.Count > 0)
            {
                scrollPosPrefabMissingScripts = EditorGUILayout.BeginScrollView(scrollPosPrefabMissingScripts, GUILayout.Height(150));
                List<GameObject> objectsToRemoveScripts = new List<GameObject>(prefabsWithMissingScripts);

                foreach (var obj in objectsToRemoveScripts)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    if (GUILayout.Button("Remove Missing Scripts"))
                    {
                        RemoveMissingScripts(obj);
                        prefabsWithMissingScripts.Remove(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            if (prefabsWithMissingScripts.Count > 0)
            {
                if (GUILayout.Button("Remove All"))
                {
                    RemoveAllMissingScripts(prefabsWithMissingScripts);
                }
            }

            EditorGUILayout.Space();

            GUILayout.Label("Make sure the scene is open");

            if (GUILayout.Button("Find missing scripts in this scene"))
            {
                FindObjectsWithMissingScriptsInScenes();
            }

            if (objectsWithMissingScripts.Count > 0)
            {
                scrollPosSceneMissingScripts = EditorGUILayout.BeginScrollView(scrollPosSceneMissingScripts, GUILayout.Height(150));
                List<GameObject> objectsToRemoveScripts = new List<GameObject>(objectsWithMissingScripts);

                foreach (var obj in objectsToRemoveScripts)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    if (GUILayout.Button("Remove Missing Scripts"))
                    {
                        RemoveMissingScripts(obj);
                        objectsWithMissingScripts.Remove(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.Space();
            if (objectsWithMissingScripts.Count > 0)
            {
                if (GUILayout.Button("Remove All"))
                {
                    RemoveAllMissingScripts(objectsWithMissingScripts);
                }
            }
            if (removedMissingScripts)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("Removed missing scripts", MessageType.Info);
            }
            GUILayout.Space(10);
            GUILayout.Label("Simblend");

            EditorGUILayout.EndScrollView();
        }

        private void FindObjectsWithMissingScriptsInPrefabs()
        {
            removedMissingScripts = false;

            prefabsWithMissingScripts.Clear();

            string[] allPrefabPaths = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in allPrefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                FindMissingScriptsInPrefab(prefab);
            }
        }

        private void FindObjectsWithMissingScriptsInScenes()
        {
            removedMissingScripts = false;

            objectsWithMissingScripts.Clear();

            foreach (var gameObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                FindMissingScriptsInGameObject(gameObject);
            }
        }

        private void FindMissingScriptsInPrefab(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    prefabsWithMissingScripts.Add(obj);
                    break;
                }
            }

            foreach (Transform transform in obj.transform)
            {
                FindMissingScriptsInGameObject(transform.gameObject, isPrefab: true);
            }
        }

        private void FindMissingScriptsInGameObject(GameObject obj, bool isPrefab = false)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    objectsWithMissingScripts.Add(obj);
                    break;
                }
            }

            foreach (Transform transform in obj.transform)
            {
                FindMissingScriptsInGameObject(transform.gameObject);
            }
        }

        private void RemoveMissingScripts(GameObject obj)
        {
            if (GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj) != 0)
            {
                removedMissingScripts = true;
            }

            foreach (Transform transform in obj.transform)
            {
                RemoveMissingScripts(transform.gameObject);
            }
        }

        private void RemoveAllMissingScripts(List<GameObject> objList)
        {
            List<GameObject> objectsToRemove = new List<GameObject>();
            foreach (var obj in objList)
            {
                if (obj != null)
                {
                    if (GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj) != 0)
                    {
                        removedMissingScripts = true;
                    }

                    foreach (Transform transform in obj.transform)
                    {
                        RemoveMissingScripts(transform.gameObject);
                    }
                    objectsToRemove.Add(obj);
                }
            }

            if (objectsToRemove.Count > 0)
            {
                objectsToRemove.Clear();
                objList.Clear();
            }
        }
    }
}
#endif
