using UnityEngine;
using ZombieDiner.Orders;
using DG.Tweening;
using AudioSystem;

public class IngredientInteractable : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    [SerializeField] private string clickSfxName = "TestingSfxClip";
    [SerializeField] private float hoverScaleMultiplier = 1.08f;
    [SerializeField] private float animationDuration = 0.15f;

    private Vector3 originalScale;
    private Tween hoverTween;

    public ItemSO Item => item;

    private SpriteRenderer itemSpriteRenderer;

    private void Awake()
    {
        originalScale = transform.localScale;
        if (!TryGetComponent<Collider2D>(out _))
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        SetupChildVisual();
    }

    private void Start()
    {
        SetupChildVisual();
    }

    private void SetupChildVisual()
    {
        if (item == null || item.itemIcon == null) return;

        itemSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (itemSpriteRenderer == null)
        {
            GameObject visualGO = new GameObject("ItemVisual");
            visualGO.transform.SetParent(transform);
            visualGO.transform.localPosition = Vector3.zero;
            visualGO.transform.localScale = Vector3.one;
            itemSpriteRenderer = visualGO.AddComponent<SpriteRenderer>();
        }

        if (itemSpriteRenderer != null && itemSpriteRenderer.sprite == null)
        {
            itemSpriteRenderer.sprite = item.itemIcon;
        }
    }

    private void OnMouseEnter()
    {
        hoverTween?.Kill();
        hoverTween = transform.DOScale(originalScale * hoverScaleMultiplier, animationDuration).SetEase(Ease.OutQuad);
    }

    private void OnMouseExit()
    {
        hoverTween?.Kill();
        hoverTween = transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuad);
    }

    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        transform.DOPunchScale(new Vector3(0.18f, 0.18f, 0f), 0.2f, 8, 1f);

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(clickSfxName))
        {
            AudioManager.Instance.PlaySfx(clickSfxName);
        }

        if (PlateManager.Instance != null)
        {
            PlateManager.Instance.TryAddIngredient(item);
        }
    }

    private void OnDestroy()
    {
        hoverTween?.Kill();
    }
}
