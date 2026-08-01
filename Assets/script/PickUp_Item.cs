using UnityEngine;

public class PickUp_Item : MonoBehaviour
{
    public Base_Item KeyItem_Data;

    public void PickUp()
    {
        if(Player.Instance.TryGetComponent<Player_Inventory>(out Player_Inventory player_Inventory))
        {
            player_Inventory.AddItem(KeyItem_Data);
        }

        else
        {
            Destroy(gameObject);
            Debug.LogWarning("หา Player_Inventory ที่ตัวผู้เล่นไม่เจอ!");
        }
    }
}
