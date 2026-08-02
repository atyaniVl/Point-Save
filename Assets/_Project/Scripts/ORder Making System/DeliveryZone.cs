using UnityEngine;

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
