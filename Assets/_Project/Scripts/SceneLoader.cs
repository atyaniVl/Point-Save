using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneLoader - Static helper for scene transitions.
/// 
/// NEW METHODS (use these for scene changes):
///   SceneLoader.TransitionTo("Main Menu");                              // simple fade
///   SceneLoader.TransitionTo("Drugstore", SceneTransitionRequest.WithLoading("Preparing...")); // with loading bar
///   SceneLoader.TransitionTo("Auth Screen", SceneTransitionRequest.Quick);                     // fast fade
/// 
/// OLD METHODS (still work for special cases like UI overlays):
///   SceneLoader.LoadAdditive("SomeOverlay");
///   SceneLoader.UnloadAdditive("SomeOverlay");
/// </summary>
public static class SceneLoader
{
    private const string Source = nameof(SceneLoader);
    private static readonly System.Collections.Generic.HashSet<string> PendingAdditiveLoads = new System.Collections.Generic.HashSet<string>();

    // ═══════════════════════════════════════════
    //  NEW: Transition methods (fade + load/unload)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Transition with default settings (simple fade, no loading screen).
    /// </summary>
    public static void TransitionTo(string sceneName)
    {
        if (!ValidateBootstrap()) return;
        BootstrapManager.Instance.TransitionTo(sceneName);
    }

    /// <summary>
    /// Transition with custom settings (loading screen, custom fade speed, etc.).
    /// </summary>
    public static void TransitionTo(string sceneName, SceneTransitionRequest request)
    {
        if (!ValidateBootstrap()) return;
        BootstrapManager.Instance.TransitionTo(sceneName, request);
    }

    private static bool ValidateBootstrap()
    {
        if (BootstrapManager.Instance != null) return true;

        PharmaLogger.LogError(Source, "BootstrapManager.Instance is null! Is the Bootstrap scene loaded?");
        return false;
    }

    // ═══════════════════════════════════════════
    //  ORIGINAL: Raw scene loading (no fade)
    //  Keep these for UI overlays or special cases
    // ═══════════════════════════════════════════

    public static void LoadSingle(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) { PharmaLogger.LogError(Source, "LoadSingle failed: scene name is null or empty."); return; }
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void LoadSingle(int sceneBuildIndex)
    {
        if (!IsValidBuildIndex(sceneBuildIndex)) { PharmaLogger.LogError(Source, $"LoadSingle failed: invalid build index {sceneBuildIndex}."); return; }
        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
    }

    public static AsyncOperation LoadAdditive(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) { PharmaLogger.LogError(Source, "LoadAdditive failed: scene name is null or empty."); return null; }
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded) { PharmaLogger.LogWarning(Source, $"LoadAdditive skipped: scene '{sceneName}' is already loaded."); return null; }
        if (PendingAdditiveLoads.Contains(sceneName)) { PharmaLogger.LogWarning(Source, $"LoadAdditive skipped: scene '{sceneName}' is already loading."); return null; }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOperation != null)
        {
            PendingAdditiveLoads.Add(sceneName);
            loadOperation.completed += _ => PendingAdditiveLoads.Remove(sceneName);
        }

        return loadOperation;
    }

    public static AsyncOperation LoadAdditive(int sceneBuildIndex)
    {
        if (!IsValidBuildIndex(sceneBuildIndex)) { PharmaLogger.LogWarning(Source, $"LoadAdditive failed: invalid build index {sceneBuildIndex}."); return null; }
        string sceneName = GetSceneNameFromBuildIndex(sceneBuildIndex);
        if (string.IsNullOrEmpty(sceneName)) { PharmaLogger.LogWarning(Source, $"LoadAdditive failed: build index {sceneBuildIndex} does not resolve to a scene name."); return null; }
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded) { PharmaLogger.LogWarning(Source, $"LoadAdditive skipped: scene '{sceneName}' is already loaded."); return null; }
        if (PendingAdditiveLoads.Contains(sceneName)) { PharmaLogger.LogWarning(Source, $"LoadAdditive skipped: scene '{sceneName}' is already loading."); return null; }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
        if (loadOperation != null)
        {
            PendingAdditiveLoads.Add(sceneName);
            loadOperation.completed += _ => PendingAdditiveLoads.Remove(sceneName);
        }

        return loadOperation;
    }

    public static AsyncOperation UnloadAdditive(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) { PharmaLogger.LogError(Source, "UnloadAdditive failed: scene name is null or empty."); return null; }
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded) { PharmaLogger.LogWarning(Source, $"UnloadAdditive skipped: scene '{sceneName}' is not loaded."); return null; }
        if (SceneManager.sceneCount <= 1) { PharmaLogger.LogWarning(Source, "UnloadAdditive skipped: cannot unload the only loaded scene."); return null; }
        return SceneManager.UnloadSceneAsync(sceneName);
    }

    public static AsyncOperation UnloadAdditive(int sceneBuildIndex)
    {
        if (!IsValidBuildIndex(sceneBuildIndex)) { PharmaLogger.LogError(Source, $"UnloadAdditive failed: invalid build index {sceneBuildIndex}."); return null; }
        string sceneName = GetSceneNameFromBuildIndex(sceneBuildIndex);
        if (string.IsNullOrEmpty(sceneName)) { PharmaLogger.LogError(Source, $"UnloadAdditive failed: build index {sceneBuildIndex} does not resolve to a scene name."); return null; }
        return UnloadAdditive(sceneName);
    }

    private static bool IsValidBuildIndex(int sceneBuildIndex) => sceneBuildIndex >= 0 && sceneBuildIndex < SceneManager.sceneCountInBuildSettings;

    private static string GetSceneNameFromBuildIndex(int sceneBuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(sceneBuildIndex);
        if (string.IsNullOrEmpty(path)) return string.Empty;
        int slash = path.LastIndexOf('/');
        int dot = path.LastIndexOf('.');
        if (dot <= slash) return string.Empty;
        return path.Substring(slash + 1, dot - slash - 1);
    }
}
