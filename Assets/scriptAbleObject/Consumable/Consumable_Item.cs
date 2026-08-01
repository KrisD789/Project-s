using UnityEngine;

[CreateAssetMenu(fileName = "Consumable_Item", menuName = "Scriptable Objects/Consumable_Item")]
public class Consumable_Item : Base_Item
{
    [Header("Consumable Effect")]
    public float healAmount; // ฟื้นฟูเลือด
    public float MaxHP_Recover; // ฟื้นฟูความเหนื่อย

    // ไอเทมชิ้นนี้ใช้แล้วหมดไปหรือไม่?
    public bool isConsumedOnUse = true;
}
