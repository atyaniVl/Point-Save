using System.Collections.Generic;
using UnityEngine;
using ZombieDiner.Orders;
using ZombieDiner.Customers;
using ZombieDiner.Delivery;
using DG.Tweening;
using AudioSystem;

namespace ZombieDiner.Visuals
{
    public class PlateVisual : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer renderer;
        [SerializeField] private string sfxName = "TestingSfxClip";
        [SerializeField] private string wrongOrderSfxName = "WrongOrder";

        [Header("Container Settings")]
        [SerializeField] private ContainerType containerType = ContainerType.Plate;

        [Header("Grid Layout Settings")]
        [SerializeField] private int gridColumns = 2;
        [SerializeField] private float gridSpacingX = 0.55f;
        [SerializeField] private float gridSpacingY = 0.55f;
        [SerializeField] private float gridCenterYOffset = 0.15f;
        [SerializeField] private Vector3 cloneItemScale = new Vector3(0.55f, 0.55f, 1f);

        [Header("Respawn Animation Settings")]
        [SerializeField] private float vanishDuration = 0.2f;
        [SerializeField] private float respawnDuration = 0.35f;

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
                CustomerController customerCtrl = null;

                if (col.TryGetComponent<CustomerController>(out customerCtrl))
                {
                    if (receiver == null) receiver = customerCtrl.GetComponent<CustomerOrderReceiver>();
                }

                if (receiver != null && HasItem)
                {
                    // 🛑 التحقق من التطابق قبل إتمام التسليم
                    if (customerCtrl != null && !IsItemMatchingCustomerOrder(customerCtrl))
                    {
                        // ❌ طلب غير مطابق:
                        // 1. تشغيل صوت الخطأ
                        PlayWrongOrderSFX();

                        // 2. إرجاع الصحن لمكانه الأصلي
                        ReturnToHomePosition();

                        // 3. 🧹 تفريغ بيانات الصحن والمكونات البصرية ليصبح فارغاً وجاهزاً للطلب الجديد
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

                    // ✅ طلب مطابق: تنفيذ عملية التمرير والأنيميشن
                    if (HeldItem != null)
                    {
                        receiver.ReceiveSingleItem(HeldItem);
                    }
                    else if (PlateManager.Instance != null && PlateManager.Instance.Ingredients.Count > 0)
                    {
                        receiver.ReceivePlateItems(PlateManager.Instance.Ingredients, PlateManager.Instance.CurrentDish);
                    }

                    AnimatePlateDeliveryOrClear();
                    handled = true;
                    break;
                }

                // Target 2: Delivery Zone
                if (col.TryGetComponent<DeliveryZone>(out var deliveryZone))
                {
                    deliveryZone.Deliver(col.GetComponent<CustomerOrderReceiver>());
                    AnimatePlateDeliveryOrClear();
                    handled = true;
                    break;
                }

                // Target 3: Trash Bin
                if (col.TryGetComponent<TrashBin>(out var trashBin) || col.CompareTag("Trash"))
                {
                    if (trashBin != null) trashBin.OnTrashPlate();
                    AnimatePlateDeliveryOrClear();
                    handled = true;
                    break;
                }
            }

            // إذا لم يوضع الصحن على هدف صالح
            if (!handled)
            {
                ReturnToHomePosition();
            }
        }

        /// <summary>
        /// 🔍 التحقق مما إذا كانت المكونات أو الطبق يحتاجه الزبون
        /// </summary>
        private bool IsItemMatchingCustomerOrder(CustomerController customer)
        {
            if (customer == null || customer.CurrentOrder == null || customer.CurrentOrder.items == null) return false;

            // 1. في حال كان الصحن يحمل ItemSO مفرد
            if (HeldItem != null)
            {
                return IsItemInOrder(HeldItem, customer.CurrentOrder);
            }

            // 2. في حال كان الصحن يحتوي على DishSO جاهز من الـ PlateManager
            if (PlateManager.Instance != null && PlateManager.Instance.CurrentDish != null)
            {
                if (IsDishInOrder(PlateManager.Instance.CurrentDish, customer.CurrentOrder))
                {
                    return true;
                }
            }

            // 3. في حال كان الصحن يحمل قائمة مكونات مفردة داخل PlateManager
            if (PlateManager.Instance != null && PlateManager.Instance.Ingredients.Count > 0)
            {
                foreach (var ing in PlateManager.Instance.Ingredients)
                {
                    if (ing != null && IsItemInOrder(ing, customer.CurrentOrder))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsItemInOrder(ItemSO item, OrderData order)
        {
            if (item == null || order == null || order.items == null) return false;

            foreach (var orderItem in order.items)
            {
                if (orderItem?.itemData == null) continue;

                ItemSO reqSO = orderItem.itemData;
                if (item == reqSO ||
                    string.Equals(item.name, reqSO.name, System.StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(item.itemId) && string.Equals(item.itemId, reqSO.itemId, System.StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDishInOrder(DishSO dish, OrderData order)
        {
            if (dish == null || order == null || order.items == null) return false;

            foreach (var orderItem in order.items)
            {
                if (orderItem?.itemData == null) continue;

                string dName = !string.IsNullOrEmpty(dish.dishName) ? dish.dishName : dish.name;
                if (string.Equals(dName, orderItem.itemData.name, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(dName, orderItem.itemData.itemName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void PlayWrongOrderSFX()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(wrongOrderSfxName);
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

        public void AnimatePlateDeliveryOrClear()
        {
            transform.DOKill();

            transform.DOScale(Vector3.zero, vanishDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (PlateManager.Instance != null)
                {
                    PlateManager.Instance.ClearPlate();
                }
                else
                {
                    Clear();
                }

                transform.position = homePosition;
                transform.DOScale(originalScale, respawnDuration).SetEase(Ease.OutBack);
            });
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
}