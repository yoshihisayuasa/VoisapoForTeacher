using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class RemoveMissingScripts
{
    [MenuItem("Tools/Remove Missing Scripts From Scene")]
    public static void RemoveFromOpenScenes()
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            foreach (var go in scene.GetRootGameObjects())
            {
                RemoveMissingFromGameObject(go);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("✅ Missing Scriptをすべての開いているシーンから削除しました。");
    }

    [MenuItem("Tools/Remove Missing Scripts From All Prefabs")]
    public static void RemoveFromAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var contents = PrefabUtility.LoadPrefabContents(path);
            RemoveMissingFromGameObject(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }
        Debug.Log("✅ Missing Scriptをすべてのプレハブから削除しました。");
    }

    private static void RemoveMissingFromGameObject(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0)
            Debug.Log($"🧹 Removed {removed} missing scripts from {go.name}", go);
        foreach (Transform child in go.transform)
            RemoveMissingFromGameObject(child.gameObject);
    }
}
