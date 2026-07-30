using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenericSceneManagement
{
    public static class SceneLoader
    {
        private static readonly HashSet<string> _pendingLoads = new();

        public static event System.Action<Scene> SceneLoaded;
        public static event System.Action<Scene> SceneUnloaded;
        public static event System.Action<Scene, Scene> ActiveSceneChanged;

        static SceneLoader()
        {
            SceneManager.sceneLoaded += (scene, mode) => SceneLoaded?.Invoke(scene);
            SceneManager.sceneUnloaded += scene => SceneUnloaded?.Invoke(scene);
            SceneManager.activeSceneChanged += (oldScene, newScene) => ActiveSceneChanged?.Invoke(oldScene, newScene);
        }

        public static void Load(string sceneName)
        {
            if (!ValidateSceneName(sceneName)) return;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public static void Load(int buildIndex)
        {
            if (!SceneValidator.IsValidBuildIndex(buildIndex)) return;
            SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
        }

        public static AsyncOperation LoadAsync(string sceneName)
        {
            if (!ValidateSceneName(sceneName)) return null;
            return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        public static AsyncOperation LoadAdditive(string sceneName)
        {
            if (!ValidateSceneName(sceneName)) return null;

            var scene = SceneManager.GetSceneByName(sceneName);

            if (scene.IsValid() && scene.isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneName}' is already loaded.");
                return null;
            }

            if (_pendingLoads.Contains(sceneName))
            {
                Debug.LogWarning($"Scene '{sceneName}' is already loading.");
                return null;
            }

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (op != null)
            {
                _pendingLoads.Add(sceneName);
                op.completed += _ => _pendingLoads.Remove(sceneName);
            }

            return op;
        }

        public static AsyncOperation Unload(string sceneName)
        {
            if (!ValidateSceneName(sceneName)) return null;

            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneName}' is not loaded.");
                return null;
            }

            return SceneManager.UnloadSceneAsync(sceneName);
        }

        public static bool IsLoaded(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        public static void SetActive(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"Cannot set '{sceneName}' active.");
                return;
            }

            SceneManager.SetActiveScene(scene);
        }

        public static Scene ActiveScene => SceneManager.GetActiveScene();

        private static bool ValidateSceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("Scene name is null or empty.");
                return false;
            }

            return true;
        }
    }
}
