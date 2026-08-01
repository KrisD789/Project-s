using UnityEngine;

[CreateAssetMenu(fileName = "Armor_Item", menuName = "Scriptable Objects/Armor_Item")]
public class Armor_Item : Base_Item
{
    public enum ArmorType { Light, Medium, Heavy }

    [Header("Armor Stats")]
    public ArmorType type;
    public float damageReduction; // ลดดาเมจลงกี่เปอร์เซ็นต์ (เช่น 0.2 คือลดดาเมจ 20%)

    [Header("Stealth & Movement Penalties")]
    public float speedMultiplier = 1f; // ตัวคูณความเร็วเดิน (เกราะหนักอาจจะเหลือ 0.8f)
    public float noiseMultiplier = 1f; // ตัวคูณเสียงเดิน (เกราะหนักเดินแล้วเสียงดังขึ้น 1.5f)
}
