using System.Collections.Generic;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory Instance { get; private set; }

    public List<Base_Item> KeyItem = new List<Base_Item>();

    public Consumable_Item Item1;
    public Consumable_Item Item2;
    public float ConsumeTimer;

    public Consumable_Item currentItemUse = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 1. ดัก Null ป้องกันเกมแคช ถ้าผู้เล่นไม่ได้พกยามา
        Item1 = Load_out_manager.Instance.selectedConsumable_1;
        if (Item1 != null)
        {
            Item1.CurrentAmount = Item1.MaxAmount;
        }

        Item2 = Load_out_manager.Instance.selectedConsumable_2;
        if (Item2 != null)
        {
            Item2.CurrentAmount = Item2.MaxAmount;
        }
    }

    private void Update()
    {
        StartUseItem();
    }

    public void AddItem(Base_Item item)
    {
        if (item is Key_Item)
        {
            KeyItem.Add(item);
        }
    }

    public void StartUseItem()
    {
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
    }

    public void UseItem()
    {
        ConsumeTimer += Time.deltaTime;

        if (ConsumeTimer >= currentItemUse.TimeToUse && currentItemUse.CurrentAmount > 0)
        {
            Player.Instance.Healing(currentItemUse.healAmount, currentItemUse.MaxHP_Recover);
            currentItemUse.CurrentAmount -= 1;
            Player.Instance.currentState = Player.PlayerState.Idle;

            // ฮีลเสร็จแล้วก็เคลียร์ค่าทิ้ง
            CancelUseConsumable();
        }
    }

    // 1. เพิ่มฟังก์ชันผู้ช่วยสำหรับตรวจเช็กเงื่อนไขการใช้ยา 
    private bool CanUseItem(Consumable_Item item)
    {
        if (item == null || item.CurrentAmount <= 0)
            return false;

        // เงื่อนไขที่ 1: ยานี้มีค่าฟื้นฟูเลือดเป้าหมาย และ เลือดปัจจุบันยังไม่เต็ม
        bool needHP = (item.healAmount > 0) && (Player.Instance.currentHP < Player.Instance.currentMaxHP);

        // เงื่อนไขที่ 2: ยานี้มีค่าฟื้นฟูความเหนื่อย และ MaxHP ปัจจุบันยังไม่เท่ากับ MaxHP สูงสุดดั้งเดิม
        bool needMaxHP = (item.MaxHP_Recover > 0) && (Player.Instance.currentMaxHP < Player.Instance.MaxHP);

        // ถ้าต้องการฮีลเลือด หรือ ต้องการฟื้นฟู MaxHP อย่างใดอย่างหนึ่ง ก็ให้ผ่าน!
        return needHP || needMaxHP;
    }

    public void EquipConsumeItem1()
    {
        if (CanUseItem(Item1))
        {
            currentItemUse = Item1;
            ConsumeTimer = 0f;
        }
    }

    public void EquipConsumeItem2()
    {
        if (CanUseItem(Item2))
        {
            currentItemUse = Item2;
            ConsumeTimer = 0f;
        }
    }

    public void CancelUseConsumable()
    {
        currentItemUse = null;
        ConsumeTimer = 0f;
        // ถ้าไม่อยากให้มันค้างสถานะ Healing เวลายกเลิกกลางคัน ให้คืนค่าสถานะด้วยครับ
        if (Player.Instance.currentState == Player.PlayerState.Healing)
        {
            Player.Instance.currentState = Player.PlayerState.Idle;
        }
    }
}