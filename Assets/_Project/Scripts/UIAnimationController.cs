using DG.Tweening;
using UnityEngine;

public class UIAnimationController : MonoBehaviour
{
    [SerializeField] private Transform root;

    [SerializeField] private DOTweenAnimation[] animations;

    private void Awake()
    {
        animations = root.GetComponentsInChildren<DOTweenAnimation>(true);
    }

    public void PlayAll()
    {
        foreach (var animation in animations)
            animation.DOPlayForward();
    }

    public void RewindAll()
    {
        foreach (var animation in animations)
            animation.DORewind();
    }

    public void RestartAll()
    {
        foreach (var animation in animations)
            animation.DORestart();
    }
}