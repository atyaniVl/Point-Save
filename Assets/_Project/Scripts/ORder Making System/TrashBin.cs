using UnityEngine;
using DG.Tweening;
using AudioSystem;

public class TrashBin : MonoBehaviour
{
    public static TrashBin Instance { get; private set; }

    [SerializeField] private string trashSfxName = "TestingSfxClip";

    private Vector3 originalScale;

    private void Awake()
    {
        Instance = this;
        originalScale = transform.localScale;

        if (!TryGetComponent<Collider2D>(out _))
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 1.5f);
        }

        gameObject.tag = "Trash";
    }

    public void OnTrashPlate()
    {
        transform.DOKill();
        transform.DOPunchScale(new Vector3(0.25f, -0.25f, 0f), 0.3f, 8, 1f);

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(trashSfxName))
        {
            AudioManager.Instance.PlaySfx(trashSfxName);
        }

        if (PlateManager.Instance != null)
        {
            PlateManager.Instance.ClearPlate();
        }
    }
}
