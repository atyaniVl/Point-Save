using UnityEngine;
namespace UIButtonAnimationSystem{
[CreateAssetMenu(menuName="UI/Button Animation Profile")]
public class ButtonAnimationProfile:ScriptableObject{
 public ButtonVisualState normal,hover,pressed,selected,disabled;
}
}