using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenericSceneManagement
{
    public static class SceneValidator
    {
        public static bool IsValidBuildIndex(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"Invalid build index: {buildIndex}");
                return false;
            }

            return true;
        }

        public static bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string file = System.IO.Path.GetFileNameWithoutExtension(path);

                if (file == sceneName)
                    return true;
            }

            return false;
        }
    }
}
