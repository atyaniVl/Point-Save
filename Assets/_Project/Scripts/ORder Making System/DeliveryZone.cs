using UnityEngine;
using ZombieDiner.Customers;
using ZombieDiner.Orders; // 👈 تم إضافة نطاق الأسماء للوصول إلى PlateManager

namespace ZombieDiner.Delivery
{
    public class DeliveryZone : MonoBehaviour
    {
        private CustomerOrderReceiver customerOrderReceiver;

        private void Awake()
        {
            customerOrderReceiver = GetComponent<CustomerOrderReceiver>();
        }

        private void OnMouseDown()
        {
            Deliver(customerOrderReceiver);
        }

        public void Deliver(CustomerOrderReceiver receiver)
        {
            // 🛑 تصحيح حرف m الصغير إلى M الكبير في PlateManager
            if (receiver == null || PlateManager.Instance == null)
            {
                return;
            }

            if (PlateManager.Instance.Ingredients != null && PlateManager.Instance.Ingredients.Count > 0)
            {
                receiver.ReceivePlateItems(PlateManager.Instance.Ingredients, PlateManager.Instance.CurrentDish);
                PlateManager.Instance.ClearPlate();
            }
        }
    }
}