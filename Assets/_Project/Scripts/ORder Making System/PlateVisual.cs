using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;
using ZombieDiner.Customers;
using DG.Tweening;
using AudioSystem;

public class PlateVisual : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private string sfxName = "TestingSfxClip";

    [Header("Container Settings")]
    [SerializeField] private ContainerType containerType = ContainerType.Plate;

    [Header("Grid Layout Settings")]
    [SerializeField] private int gridColumns = 2;
    [SerializeField] private float gridSpacingX = 0.55f;
    [SerializeField] private float gridSpacingY = 0.55f;
    [SerializeField] private float gridCenterYOffset = 0.15f;
    [SerializeField] private Vector3 cloneItemScale = new Vector3(0.55f, 0.55f, 1f);

    private Vector3 homePosition;
    private Vector3 originalScale;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private bool isDragging = false;
    private Sprite defaultPlateSprite;

    private readonly List<GameObject> activeIngredientClones = new List<GameObject>();

    public ContainerType ContainerType => containerType;
    public ItemSO HeldItem { get; private set; }
    public bool HasItem => HeldItem != null || activeIngredientClones.Count > 0;

    private void Awake()
    {
        homePosition = transform.position;
        originalScale = transform.localScale;
        mainCamera = Camera.main;

        if (renderer == null)
        {
            renderer = GetComponent<SpriteRenderer>();
        }

        if (renderer != null)
        {
            defaultPlateSprite = renderer.sprite;
        }

        if (!TryGetComponent<Collider2D>(out _))
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs((mainCamera != null ? mainCamera.transform.position.z : -10f) - transform.position.z);
        return mainCamera != null ? mainCamera.ScreenToWorldPoint(mousePos) : transform.position;
    }

    private void OnMouseDown()
    {
        if (!HasItem) return;

        isDragging = true;
        transform.DOKill();
        dragOffset = transform.position - GetMouseWorldPosition();
        transform.DOScale(originalScale * 1.1f, 0.1f);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 targetPos = GetMouseWorldPosition() + dragOffset;
        targetPos.z = transform.position.z;
        transform.position = targetPos;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        transform.DOScale(originalScale, 0.1f);

        Vector2 dropPos = transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(dropPos, 0.75f);

        bool handled = false;

        foreach (var col in hitColliders)
        {
            if (col == null || col.gameObject == gameObject) continue;

            // Target 1: Customer Order Receiver / Controller
            var receiver = col.GetComponent<CustomerOrderReceiver>();
            if (receiver == null && col.TryGetComponent<CustomerController>(out var customerCtrl))
            {
                receiver = customerCtrl.GetComponent<CustomerOrderReceiver>();
            }

            if (receiver != null && HasItem)
            {
                if (HeldItem != null)
                {
                    receiver.ReceiveSingleItem(HeldItem);
                }
                else if (PlateManager.Instance != null && PlateManager.Instance.Ingredients.Count > 0)
                {
                    receiver.ReceivePlateItems(PlateManager.Instance.Ingredients, PlateManager.Instance.CurrentDish);
                }

                if (PlateManager.Instance != null)
                {
                    PlateManager.Instance.ClearPlate();
                }
                else
                {
                    Clear();
                }

                handled = true;
                break;
            }

            if (col.TryGetComponent<DeliveryZone>(out var deliveryZone))
            {
                deliveryZone.Deliver(col.GetComponent<CustomerOrderReceiver>());
                handled = true;
                break;
            }

            // Target 2: Trash Bin
            if (col.TryGetComponent<TrashBin>(out var trashBin) || col.CompareTag("Trash"))
            {
                if (trashBin != null)
                {
                    trashBin.OnTrashPlate();
                }
                
                if (PlateManager.Instance != null)
                {
                    PlateManager.Instance.ClearPlate();
                }
                else
                {
                    Clear();
                }

                handled = true;
                break;
            }
        }

        if (!handled)
        {
            ReturnToHomePosition();
        }
    }

    public bool TryFillContainer(ItemSO item)
    {
        if (item == null || HeldItem != null) return false;

        HeldItem = item;
        OnIngredientAdded(item);
        return true;
    }

    public void ReturnToHomePosition()
    {
        transform.DOKill();
        transform.DOMove(homePosition, 0.25f).SetEase(Ease.OutQuad);
        transform.DOScale(originalScale, 0.25f).SetEase(Ease.OutQuad);
    }

    public void OnIngredientAdded(ItemSO item)
    {
        transform.DOComplete();
        transform.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.18f);

        if (item != null && item.itemIcon != null)
        {
            GameObject cloneGO = new GameObject($"IngredientClone_{item.itemName}");
            cloneGO.transform.SetParent(transform);

            // Grid calculation for neat 2D grid arrangement
            int index = activeIngredientClones.Count;
            int row = index / gridColumns;
            int col = index % gridColumns;

            float startX = -((gridColumns - 1) * gridSpacingX) / 2.0f;
            float xPos = startX + (col * gridSpacingX);
            float yPos = gridCenterYOffset - (row * gridSpacingY);

            cloneGO.transform.localPosition = new Vector3(xPos, yPos, -0.1f * (index + 1));
            cloneGO.transform.localScale = cloneItemScale;

            SpriteRenderer cloneSr = cloneGO.AddComponent<SpriteRenderer>();
            cloneSr.sprite = item.itemIcon;
            cloneSr.sortingOrder = (renderer != null ? renderer.sortingOrder : 0) + index + 1;

            cloneGO.transform.localScale = Vector3.zero;
            cloneGO.transform.DOScale(cloneItemScale, 0.2f).SetEase(Ease.OutBack);

            activeIngredientClones.Add(cloneGO);
        }
    }

    public void ShowDish(DishSO d)
    {
        ClearIngredientClones();

        if (renderer != null && d != null)
        {
            renderer.sprite = d.sprite;
        }

        transform.DOComplete();
        transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.3f, 6, 1f);
    }

    public void Clear()
    {
        isDragging = false;
        HeldItem = null;
        ClearIngredientClones();

        if (renderer != null && defaultPlateSprite != null)
        {
            renderer.sprite = defaultPlateSprite;
        }

        ReturnToHomePosition();
    }

    private void ClearIngredientClones()
    {
        foreach (var clone in activeIngredientClones)
        {
            if (clone != null)
            {
                clone.transform.DOKill();
                Destroy(clone);
            }
        }
        activeIngredientClones.Clear();
    }
}