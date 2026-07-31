using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; using DG.Tweening;
namespace UIButtonAnimationSystem{
[RequireComponent(typeof(Button))]
public class UIButtonAnimator:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler{
 public RectTransform targetTransform; public Image targetImage; public ButtonAnimationProfile profile;
        void Awake()
        {
            if (!targetTransform) targetTransform = GetComponent<RectTransform>();
            if (!targetImage) targetImage = GetComponent<Image>(); Apply(profile.normal);
        }
        void Apply(ButtonVisualState s)
        {
            if (s == null) return; targetTransform.DOKill(); if (targetImage) targetImage.DOKill();
            if (s.overrideScale) targetTransform.DOScale(s.scale, s.duration).SetEase(s.ease);
            if (targetImage && s.overrideColor) targetImage.DOColor(s.color, s.duration).SetEase(s.ease);
            if (targetImage && s.overrideSprite && s.sprite) targetImage.sprite = s.sprite;
        }
     public void OnPointerEnter(PointerEventData e)=>Apply(profile.hover);
     public void OnPointerExit(PointerEventData e)=>Apply(profile.normal);
     public void OnPointerDown(PointerEventData e)=>Apply(profile.pressed);
     public void OnPointerUp(PointerEventData e)=>Apply(profile.hover);
     public void OnSelect(BaseEventData e)=>Apply(profile.selected);
     public void OnDeselect(BaseEventData e)=>Apply(profile.normal);
}
}