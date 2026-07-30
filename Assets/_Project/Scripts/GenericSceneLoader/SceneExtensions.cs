using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace GenericSceneManagement
{
    public static class SceneExtensions
    {
        public static IEnumerable<Scene> LoadedScenes
        {
            get
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    yield return SceneManager.GetSceneAt(i);
            }
        }
    }
}
