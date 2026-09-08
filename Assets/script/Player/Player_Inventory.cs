using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory Instance { get; private set; }

    // กระเป๋าใบเดียว เก็บได้ทุกอย่างในเกม! (เพราะทุกอย่างสืบทอดมาจาก BaseItem)
    //public List<Base_Item> inventory = new List<Base_Item>();
    public List<Base_Item> KeyItem = new List<Base_Item>();

    public Consumable_Item Item1;
    public Consumable_Item Item2;
    //public Consumable_Item Item3;
    public float ConsumeTimer;
   

    public Consumable_Item currentItemUse = null;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Item1 = Load_out_manager.Instance.selectedConsumable_1;
        Item2 = Load_out_manager.Instance.selectedConsumable_2;

        Item1.CurrentAmount = Item1.MaxAmount;
        Item2.CurrentAmount = Item2.MaxAmount;
    }


    private void Update()
    {
        StartUseItem();

        //print("ConsumeAmount: " + currentItemUse.CurrentAmount);
        //print(currentItemUse);
    }

    // ฟังก์ชันเก็บของฟังก์ชันเดียว รับจบ!
    public void AddItem(Base_Item item)
    {
        if (item is Key_Item)
        {
            KeyItem.Add(item);
        }

        else
        {
            //inventory.Add(item);
        }
    }


    public void StartUseItem()
    {
        // เช็กแค่ว่า ถ้าไม่ได้เป็น Idle ให้กดใช้ไอเทมไม่ได้
        if (Player.Instance.currentState == Player.PlayerState.CarryingBody ||
            Player.Instance.currentState == Player.PlayerState.GrabbingEnemy ||
            Player.Instance.currentState == Player.PlayerState.Aim)
        {
            CancelUseConsumable();
            return;
        }

        if (currentItemUse != null && currentItemUse.CurrentAmount > 0)
        {
            Player.Instance.currentState = Player.PlayerState.Healing;
            UseItem();
            
        }

        else
        {
            Debug.Log("Not Found Item To Consume");
        }
    }

    public void UseItem()
    {
        Debug.Log("ConsumeTimer: " + ConsumeTimer);
        ConsumeTimer += Time.deltaTime;
        if(ConsumeTimer > currentItemUse.TimeToUse && currentItemUse.CurrentAmount > 0)
        {
            Player.Instance.Healing(currentItemUse.healAmount, currentItemUse.MaxHP_Recover);
            currentItemUse.CurrentAmount -= 1;
            Player.Instance.currentState = Player.PlayerState.Idle;
            CancelUseConsumable();
        }
    }

    public void EquipConsumeItem1()
    {
        currentItemUse = Item1;
    }

    public void EquipConsumeItem2()
    {
        currentItemUse = Item2;
    }

    //public void EquipItem3()
    //{
        //currentItemUse = Item3;
    //}

    public void CancelUseConsumable()
    {
        currentItemUse = null;
        ConsumeTimer = 0;
        //Player.Instance.currentState = Player.PlayerState.Idle;
    }
}
