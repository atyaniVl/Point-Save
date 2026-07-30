using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenericSceneManagement
{
    public class SceneTransitionService : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeCanvas;
        [SerializeField] private float fadeDuration = 0.35f;

        public void Transition(string sceneName)
        {
            StartCoroutine(TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            yield return Fade(1);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

            while (!op.isDone)
                yield return null;

            yield return Fade(0);
        }

        private IEnumerator Fade(float target)
        {
            if (fadeCanvas == null)
            {
                Debug.LogWarning("Fade CanvasGroup not assigned.");
                yield break;
            }

            float start = fadeCanvas.alpha;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }

            fadeCanvas.alpha = target;
        }
    }
}
