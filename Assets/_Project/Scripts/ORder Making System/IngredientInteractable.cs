using AudioSystem;
using DG.Tweening;
using UnityEngine;
using ZombieDiner.Orders;
using ZombieDiner.Visuals;

public class IngredientInteractable : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemSO item;

    [Header("Feedback Settings")]
    [SerializeField] private string clickSfxName = "TestingSfxClip";
    [SerializeField] private float hoverScaleMultiplier = 1.08f;
    [SerializeField] private float animationDuration = 0.15f;

    [Header("Flying Animation Settings")]
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private float jumpPower = 1.2f;

    private Vector3 originalScale;
    private Tween hoverTween;
    private SpriteRenderer itemSpriteRenderer;

    public ItemSO Item => item;

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
        if (item == null) return;

        // ضغط بصري على الماكينة/الشواية
        transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0f), 0.2f, 8, 1f);

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(clickSfxName))
        {
            AudioManager.Instance.PlaySfx(clickSfxName);
        }

        // 🎯 إيجاد الصحن المطلوب بالطريقة الصحيحة:
        Vector3 targetPosition = transform.position;
        PlateVisual targetPlate = GetActivePlateVisual();

        if (targetPlate != null)
        {
            targetPosition = targetPlate.transform.position;
        }

        AnimateItemFlyingToPlate(targetPosition, () =>
        {
            if (PlateManager.Instance != null)
            {
                PlateManager.Instance.TryAddIngredient(item);
            }
        });
    }

    /// <summary>
    /// 🔍 البحث عن الصحن المرتبط بالـ PlateManager لتحديد الهدف بدقة
    /// </summary>
    private PlateVisual GetActivePlateVisual()
    {
        // 1. محاولة جلب الصحن المحدد داخل PlateManager أولاً
        if (PlateManager.Instance != null)
        {
            // نستخدم Reflection أو نبحث عن الصحن المرتبط بالمدير
            PlateVisual managerPlate = PlateManager.Instance.GetComponentInChildren<PlateVisual>();
            if (managerPlate != null) return managerPlate;
        }

        // 2. إذا لم ينجح، نجلب الصحن الشغال حالياً (الذي يحتوي على مكان فارغ)
        PlateVisual[] allPlates = Object.FindObjectsByType<PlateVisual>(FindObjectsSortMode.None);
        foreach (var plate in allPlates)
        {
            // إرجاع أول صحن نشط ومناسب
            if (plate.gameObject.activeInHierarchy)
            {
                return plate;
            }
        }

        return null;
    }

    /// <summary>
    /// ✈ أنيميشن طيران العنصر بقوس (Arc) مع دوران خفيف نحو الصحن
    /// </summary>
    private void AnimateItemFlyingToPlate(Vector3 targetPos, System.Action onComplete)
    {
        GameObject flyer = new GameObject($"Flyer_{item.itemName}");
        flyer.transform.position = transform.position;
        flyer.transform.localScale = Vector3.one * 0.7f;

        SpriteRenderer flyerSr = flyer.AddComponent<SpriteRenderer>();
        flyerSr.sprite = item.itemIcon;
        flyerSr.sortingOrder = 50; // طبقة علوية أثناء الطيران

        Sequence flySeq = DOTween.Sequence();

        flySeq.Join(flyer.transform.DOJump(targetPos, jumpPower, 1, flyDuration).SetEase(Ease.OutQuad));
        flySeq.Join(flyer.transform.DORotate(new Vector3(0, 0, 360f), flyDuration, RotateMode.FastBeyond360));
        flySeq.Join(flyer.transform.DOScale(Vector3.one * 0.4f, flyDuration));

        flySeq.OnComplete(() =>
        {
            Destroy(flyer);
            onComplete?.Invoke();
        });
    }

    private void OnDestroy()
    {
        hoverTween?.Kill();
    }
}
