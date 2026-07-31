using UnityEngine; using DG.Tweening;
namespace UIButtonAnimationSystem{
[System.Serializable]
public class ButtonVisualState{
 public bool overrideScale=true;
 public Vector3 scale=Vector3.one;
 public bool overrideColor;
 public Color color=Color.white;
 public bool overrideSprite;
 public Sprite sprite;
 public float duration=.15f;
 public Ease ease=Ease.OutQuad;
}
}