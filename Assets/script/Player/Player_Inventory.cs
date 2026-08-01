using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{
    // กระเป๋าใบเดียว เก็บได้ทุกอย่างในเกม! (เพราะทุกอย่างสืบทอดมาจาก BaseItem)
    public List<Base_Item> inventory = new List<Base_Item>();
    public List<Base_Item> KeyItem = new List<Base_Item>();

    // ฟังก์ชันเก็บของฟังก์ชันเดียว รับจบ!
    public void AddItem(Base_Item item)
    {
        if (item is Key_Item)
        {
            KeyItem.Add(item);
        }

        else
        {
            inventory.Add(item);
        }
    }
}
